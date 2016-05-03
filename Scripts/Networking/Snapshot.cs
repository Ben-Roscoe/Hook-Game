using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class Snapshot
    {


        // Public:


        public struct SnapshotRPC
        {
            public RPCCall      call;
            public int          snapshotNumAdded;


            public SnapshotRPC( RPCCall call, int snapshotNumAdded )
            {
                this.call               = call;
                this.snapshotNumAdded   = snapshotNumAdded;
            }


            public SnapshotRPC( NetIncomingMessage msg )
            {
                call                    = null;
                snapshotNumAdded        = -1;
                Read( msg );
            }


            public void Write( NetOutgoingMessage msg )
            {
                msg.Write( snapshotNumAdded );
                call.WriteRPC( msg );
            }


            public void Read( NetIncomingMessage msg )
            {
                snapshotNumAdded = msg.ReadInt32();
                call             = new RPCCall( msg );
            }
        }


        public Snapshot()
        {
            type                    = SnapshotType.Dummy;
            this.snapshotNum        = -1;
        }


        public Snapshot( Snapshot last, int snapshotNum )
        {
            this.type                       = SnapshotType.UnACK;
            this.snapshotNum                = snapshotNum;
        }


        public Snapshot( NetIncomingMessage msg, Snapshot last, int snapshotNum )
        {
            type                    = SnapshotType.Client;
            this.snapshotNum        = snapshotNum;
            ReadDelta( msg, last );
        }


        public void AddRPC( RPCCall call )
        {
            calls.Add( new SnapshotRPC( call, snapshotNum ) );
        }


        public void RemoveRPCsFrom( int networkStep )
        {
            for( int i = 0; i < calls.Count; ++i )
            {
                if( calls[i].snapshotNumAdded <= networkStep )
                {
                    calls.RemoveAt( i );
                    --i;
                }
            }
        }


        public void CopyClientWorldState( ClientState client, UInt16 actorCount )
        {
            // How large is the actor list?
            this.actorCount                     = actorCount;

            actorStates.Clear();

            buffer = new NetBuffer();

            // Write every relevant actor for this client.
            for( int i = 0; i < client.Relevancies.Count; ++i )
            {
                if( client.Relevancies[i].Type == RelevancyType.Deactive )
                {
                    continue;
                }

                ActorState state = new ActorState( client.Relevancies[i].Actor.Id, buffer.LengthBits, 0 );
                client.Relevancies[i].Actor.WriteSnapshot( buffer );
                buffer.WritePadBits();
                state.End = buffer.LengthBits;
                actorStates.Add( state );
            }
        }


        public void WriteDelta( NetOutgoingMessage msg, Snapshot last, List<SnapshotRPC> missedCalls )
        {
            // Snapshot number.
            msg.Write( snapshotNum );

            // What snapshot is this a delta from?
            msg.Write( last.SnapshotNum );

            msg.WriteTime( true );
            msg.Write( ( UInt16 )actorCount );

            // RPC calls. Get previously missed calls.
            msg.Write( ( byte )( calls.Count + missedCalls.Count ) );
            for( int i = 0; i < missedCalls.Count; ++i )
            {
                missedCalls[i].Write( msg );
            }
            for( byte i = 0; i < calls.Count; ++i )
            {
                calls[i].Write( msg );
            }

            // Write the delta actor data to the message.
            for( int i = 0; i < actorStates.Count; ++i )
            {
                var id              = actorStates[i].ActorId;
                var lastState       = last.actorStates.Find( x => x.ActorId == id );
                WriteDeltaActor( msg, last, lastState, actorStates[i] );
            }
        }


        public void WriteDeltaActor( NetOutgoingMessage msg, Snapshot last, ActorState from, ActorState to )
        {
            // Unique ID.
            msg.Write( ( UInt16 )to.ActorId );

            msg.Write( ( Int32 )to.LengthBits );
            
            // If we don't have a matching length, then we just write the entire state again.
            var compare = ( from == null || to.LengthBits != from.LengthBits ) ? ( UInt16 )0 : ( UInt16 )to.LengthBits;// Mathf.Min( to.Buffer.LengthBytes, ( from == null ? 0 : from.Buffer.LengthBytes ) );

            if( from != null )
            {
                for( int i = 0; i < compare / 8; ++i )
                {
                    // Get each bytes delta.
                    msg.Write( ( byte )( buffer.Data[to.Start / 8 + i] - last.buffer.Data[from.Start / 8 + i] ) );
                }
            }

            // The things we're not delta compressing.
            for( int i = compare / 8; i < to.LengthBits / 8; ++i )
            {
                msg.Write( buffer.Data[to.Start / 8 + i] );
            }
        }


        public void ReadDelta( NetIncomingMessage msg, Snapshot last )
        {
            buffer          = new NetBuffer();
            getTime         = NetTime.Now;
            receivedTime    = msg.ReadTime( true );
            actorCount      = msg.ReadUInt16();
            // receivedTime    = msg.ReceiveTime - ( msg.SenderConnection.AverageRoundtripTime / 2.0f );

            // RPC calls.
            byte callCount = msg.ReadByte();
            for( byte i = 0; i < callCount; ++i )
            {
                SnapshotRPC call = new SnapshotRPC( msg );
                calls.Add( call );
            }

            while( msg.Position != msg.LengthBits )
            {
                var id          = msg.ReadUInt16();
                var lastState   = last.actorStates.Find( x => x.ActorId == id );
                var newState    = ReadDeltaActor( msg, last, lastState, id );
                ActorStates.Add( newState );
            }
        }


        private ActorState ReadDeltaActor( NetIncomingMessage msg, Snapshot last, ActorState from, UInt16 actorId )
        {
            var length  = msg.ReadInt32();
            var compare = ( from == null || length != from.End - from.Start ) ? ( UInt16 )0 : ( UInt16 )length;

            var state   = new ActorState( actorId, buffer.LengthBits, 0 );

            if( from != null )
            {
                for( int i = 0; i < compare / 8; ++i )
                {
                    buffer.Write( ( byte )( last.buffer.Data[from.Start / 8 + i] + msg.ReadByte() ) );
                }
            }

            for( int i = compare / 8; i < length / 8; ++i )
            {
                buffer.Write( msg.ReadByte() );
            }

            state.End = buffer.LengthBits;

            return state;
        }


        public void Acknowledge()
        {
            if( type == SnapshotType.UnACK )
            {
                type = SnapshotType.ACK;
            }
        }


        public List<SnapshotRPC> RPCCalls
        {
            get
            {
                return calls;
            }
        }


        public List<ActorState> ActorStates
        {
            get
            {
                return actorStates;
            }
        }


        public int SnapshotNum
        {
            get
            {
                return snapshotNum;
            }
        }


        public SnapshotType Type
        {
            get
            {
                return type;
            }
        }


        public double Timestamp
        {
            get
            {
                return receivedTime;
            }
            set
            {
                receivedTime = value;
            }
        }


        public UInt16 ActorListSize
        {
            get
            {
                return actorCount;
            }
            set
            {
                actorCount = value;
            }
        }


        public NetBuffer Buffer
        {
            get
            {
                return buffer;
            }
        }


        public double GetTime
        {
            get
            {
                return getTime;
            }
        }


        // Private:


        private SnapshotType            type                        = SnapshotType.UnACK;
        private int                     snapshotNum                 = 0;

        private List<SnapshotRPC>       calls               = new List<SnapshotRPC>();
        private List<ActorState>        actorStates         = new List<ActorState>();

        private double                  receivedTime        = 0.0;
        private double                  getTime             = 0.0;
        private UInt16                  actorCount          = 0;

        private NetBuffer               buffer              = null;
    }


    public class ActorState
    {

        public ActorState( UInt16 actorId, long start, long end )
        {
            this.actorId    = actorId;
            this.start      = start;
            this.end        = end;
        }


        public UInt16 ActorId
        {
            get
            {
                return actorId;
            }
        }


        public long Start
        {
            get
            {
                return start;
            }
            set
            {
                start = value;
            }
        }


        public long End
        {
            get
            {
                return end;
            }
            set
            {
                end = value;
            }
        }


        public long LengthBits
        {
            get
            {
                return end - start;
            }
        }


        private UInt16                      actorId;
        private long                        start;
        private long                        end;
    }


    public enum SnapshotType
    {
        Dummy,
        Client,
        UnACK,
        ACK,
    }
}

using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public enum RelevancyType
    {
        Active,
        Deactive,
    }

    public class Relevancy
    {
        public ClientState          Client              { get; set; }
        public Actor                Actor               { get; set; }


        public Relevancy( ClientState client, Actor actor, RelevancyType type )
        {
            Client                  = client;
            Actor                   = actor;
            this.type               = type;

            if( type == RelevancyType.Active )
            {
                MoveRelevantRPCs();
            }
        }


        public RelevancyType Type
        {
            get
            {
                return type;
            }
            set
            {
                // Send all the buffered rpcs the client has been missing out on.
                if( type != value && value == RelevancyType.Active )
                {
                    MoveRelevantRPCs();
                }
                type = value;
            }
        }


        private RelevancyType type = RelevancyType.Deactive;


        private void MoveRelevantRPCs()
        {
            for( int i = 0; i < Client.BufferedRPCs.Count; ++i )
            {
                if( Client.BufferedRPCs[i].DestinationActorId != Actor.Id )
                {
                    continue;
                }

                Client.SnapshotBuffer.CurrentSnapshot.AddRPC( Client.BufferedRPCs[i] );
                Client.BufferedRPCs.RemoveAt( i );
                --i;
            }
        }
    }

    public class ClientState
    {


        // Public:


        public ClientState( NetConnection netConnection )
        {
            this.netConnection              = netConnection;

            snapshotBuffer      = new SnapshotBuffer();
        }


        public long Id
        {
            get
            {
                return netConnection.RemoteUniqueIdentifier;
            }
        }


        public NetConnection NetConnection
        {
            get
            {
                return netConnection;
            }
        }


        public List<Relevancy> Relevancies
        {
            get
            {
                return relevancies;
            }
        }


        public List<RPCCall> BufferedRPCs
        {
            get
            {
                return bufferedRPCs;
            }
        }


        public SnapshotBuffer SnapshotBuffer
        {
            get
            {
                return snapshotBuffer;
            }
        }


        public int LastAchknowledgedSnapshot
        {
            get
            {
                return lastAcknowledgedSnapshot;
            }
            set
            {
                lastAcknowledgedSnapshot = value;
            }
        }


        public bool IsLoadingCurrentMap
        {
            get
            {
                return isLoadingCurrentMap;
            }
            set
            {
                isLoadingCurrentMap = value;
            }
        }


        // Private:
        

        private NetConnection               netConnection       = null;
        private SnapshotBuffer              snapshotBuffer      = null;

        private int                         lastAcknowledgedSnapshot = -1;
        private bool                        isLoadingCurrentMap      = true;

        private List<Relevancy>             relevancies         = new List<Relevancy>();
        private List<RPCCall>               bufferedRPCs        = new List<RPCCall>();
    }
}

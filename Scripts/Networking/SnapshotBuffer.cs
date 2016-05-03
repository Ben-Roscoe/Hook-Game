using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class SnapshotBuffer
    {


        // Public:

        
        public SnapshotBuffer()
        {
            ClearSnapshots();
        }


        public List<Snapshot.SnapshotRPC> GetMissedCalls()
        {
            var lastAckd = GetLastACKSnapshot();
            if( lastAckd == dummy )
            {
                return dummy.RPCCalls;
            }

            var calls    = new List<List<Snapshot.SnapshotRPC>>();

            for( int i = 1; i < snapshots.Length; ++i )
            {
                if( snapshots[i].Type == SnapshotType.ACK )
                {
                    break;
                }
                calls.Add( snapshots[i].RPCCalls );
            }

            // Copy them over to the final list in the correct order.
            var orderedCalls = new List<Snapshot.SnapshotRPC>();
            for( int i = calls.Count - 1; i >= 0; --i )
            {
                for( int c = 0; c < calls[i].Count; ++c )
                {
                    orderedCalls.Add( calls[i][c] );
                }
            }
            return orderedCalls;
        }


        public Snapshot GetSnapshotWithNum( int num )
        {
            for( int i = 0; i < snapshots.Length; ++i )
            {
                if( snapshots[i].SnapshotNum == num )
                {
                    return snapshots[i];
                }
            }
            return DummySnapshot;
        }


        public Snapshot GetLastACKSnapshot()
        {
            for( int i = 0; i < snapshots.Length; ++i )
            {
                if( snapshots[i].Type == SnapshotType.ACK )
                {
                    return snapshots[i];
                }
            }
            return DummySnapshot;
        }


        public void AddSnapshotRPCsToDummy( Snapshot snapshot )
        {
            for( int i = 0; i < snapshot.RPCCalls.Count; ++i )
            {
                dummy.AddRPC( snapshot.RPCCalls[i].call );
            }
        }


        public void NewSnapshot( int snapshotNum )
        {
            InsertSnapshot( new Snapshot( CurrentSnapshot, snapshotNum ) );
        }


        public void Acknowledge( int snapshotNum )
        {
            for( int i = 0; i < snapshots.Length; ++i )
            {
                if( snapshots[i].SnapshotNum == snapshotNum )
                {
                    snapshots[i].Acknowledge();
                    break;
                }
            }

            // Rebuild our list of unacknowledged RPCs.
            dummy.RPCCalls.Clear();
            for( int i = 0; i < snapshots.Length; ++i )
            {
                if( snapshots[i].Type == SnapshotType.ACK )
                {
                    for( int s = i - 1; s >= 0; --s )
                    {
                        for( int c = 0; c < snapshots[s].RPCCalls.Count; ++c )
                        {
                            dummy.AddRPC( snapshots[s].RPCCalls[c].call );
                        }
                    }
                }
            }
        }


        public void AddSnapshot( Snapshot snapshot )
        {
            // Out of sequence?
            if( snapshot.SnapshotNum <= LastSnapshot.SnapshotNum )
            {
                NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Networking, "Attempt to add out of sequence snapshot. Num: " + snapshot.SnapshotNum + " Last: " + LastSnapshot.SnapshotNum );
                return;
            }

            InsertSnapshot( snapshot );
        }


        public void ClearSnapshots()
        {
            for( int i = 0; i < snapshotBufferSize; ++i )
            {
                snapshots[i] = DummySnapshot;
            }
        }


        public Snapshot CurrentSnapshot
        {
            get
            {
                return snapshots[0];
            }           
        }


        public Snapshot LastSnapshot
        {
            get
            {
                return snapshots[1];
            }
        }


        public Snapshot[] Snapshots
        {
            get
            {
                return snapshots;
            }
        }


        public Snapshot DummySnapshot
        {
            get
            {
                return dummy;
            }
        }


        // Private:


        private const int               snapshotBufferSize = 32;

        private Snapshot[]              snapshots       = new Snapshot[snapshotBufferSize];
        private Snapshot                dummy           = new Snapshot();


        private void InsertSnapshot( Snapshot snapshot )
        {
            for( int i = ( snapshots.Length - 1 ); i > 0; --i )
            {
                snapshots[i] = snapshots[i - 1];
            }
            snapshots[0] = snapshot;
        }
    }
}

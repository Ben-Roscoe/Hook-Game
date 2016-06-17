using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class GameState : Actor
    {


        // Public:


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            RegisterRPC<StatsBase>( MulticastAddStats );
            RegisterRPC<StatsBase>( MulticastRemoveStats );
            RegisterRPC<StatsBase>( MulticastActivateStats );
            RegisterRPC<StatsBase>( MulticastDeactivateStats );

            ContainingWorld.OnGameStateCreated.Invoke( this );
        }


        public override void OnActorDestroy()
        {
            base.OnActorDestroy();

            foreach( var call in activateCalls )
            {
                ContainingWorld.NetworkSystem.UnbufferRPCCall( call.Value );
            }
            activateCalls.Clear();
            foreach( var call in addCalls )
            {
                ContainingWorld.NetworkSystem.UnbufferRPCCall( call.Value );
            }
            addCalls.Clear();
        }


        public StatsBase GetStatsForController( Controller controller )
        {
            // Is this an old player?
            StatsBase found = stats.Find( x => x.Controller == controller );
            if( found != null )
            {
                ActivateStat( found );
                return found;
            }

            // New player.
            StatsBase newStats = ( StatsBase )ContainingWorld.InstantiateReplicatedActor( statsPrefab, Vector3.zero, Quaternion.identity, ContainingWorld.NetworkSystem.AuthorityId, true, controller );
            AddStats( newStats );

            if( ContainingWorld.GameManager != null )
            {
                ContainingWorld.GameManager.InitialiseStats( controller, newStats );
            }

            ActivateStat( newStats );
            return newStats;
        }


        public void DeactivatePlayerStats( Controller controller )
        {
            StatsBase found = stats.Find( x => x.Controller == controller );
            if( found == null )
            {
                return;
            }

            DeactivateStat( found );
        }


        public StatsBase GetStats( Controller controller )
        {
            return stats.Find( x => x.Controller == controller );
        }


        public List<StatsBase> PlayerStats
        {
            get
            {
                return stats;
            }
        }


        public List<StatsBase> ActivePlayerStats
        {
            get
            {
                return ( from x in stats
                         where x.IsActive
                         select x ).ToList();
            }
        }


        public List<StatsBase> InActivePlayerStats
        {
            get
            {
                return ( from x in stats
                         where !x.IsActive
                         select x ).ToList();
            }
        }


        public NixinEvent<StatsBase> OnPlayerAdded
        {
            get
            {
                return onPlayerAdded;
            }
        }


        public NixinEvent<StatsBase> OnPlayerRemoved
        {
            get
            {
                return onPlayerRemoved;
            }
        }


        public NixinEvent<StatsBase> OnStatsActivated
        {
            get
            {
                return onStatsActivated;
            }
        }


        public NixinEvent<StatsBase> OnStatsDeactivated
        {
            get
            {
                return onStatsDeactivated;
            }
        }


        // Private:


        private List<StatsBase>                         stats       = new List<StatsBase>();
        private Dictionary<StatsBase, RPCCall>          addCalls    = new Dictionary<StatsBase, RPCCall>();
        private Dictionary<StatsBase, RPCCall>          activateCalls = new Dictionary<StatsBase, RPCCall>();

        [SerializeField]
        private StatsBase                               statsPrefab = null;

        private NixinEvent<StatsBase>                   onPlayerAdded       = new NixinEvent<StatsBase>();
        private NixinEvent<StatsBase>                   onPlayerRemoved     = new NixinEvent<StatsBase>();

        private NixinEvent<StatsBase>                   onStatsActivated     = new NixinEvent<StatsBase>();
        private NixinEvent<StatsBase>                   onStatsDeactivated   = new NixinEvent<StatsBase>();


        private void AddStats( StatsBase stat )
        {
            MulticastAddStats( stat );

            RPCCall lastCall;
            addCalls.TryGetValue( stat, out lastCall );

            addCalls[stat] = ContainingWorld.NetworkSystem.BufferRPCCall( ContainingWorld.RPC( MulticastAddStats, RPCType.Multicast, this, stat ), lastCall );
        }


        private void RemoveStats( StatsBase stat )
        {
            MulticastRemoveStats( stat );

            ContainingWorld.NetworkSystem.UnbufferRPCCall( addCalls[stat] );
            ContainingWorld.RPC( MulticastRemoveStats, RPCType.Multicast, this, stat );
        }


        private void ActivateStat( StatsBase stat )
        {
            stat.IsActive = true;
            MulticastActivateStats( stat );

            RPCCall call = null;
            activateCalls.TryGetValue( stat, out call );
            activateCalls[stat] = ContainingWorld.NetworkSystem.BufferRPCCall( ContainingWorld.RPC( MulticastActivateStats, RPCType.Multicast, this, stat ), call );
        }


        private void DeactivateStat( StatsBase stat )
        {
            stat.IsActive = false;
            MulticastDeactivateStats( stat );

            RPCCall found = null;
            if( activateCalls.TryGetValue( stat, out found ) )
            {
                ContainingWorld.NetworkSystem.UnbufferRPCCall( found );
                ContainingWorld.RPC( MulticastDeactivateStats, RPCType.Multicast, this, stat );
            }
        }


        private void MulticastAddStats( StatsBase stat )
        {
            stats.Add( stat );
            onPlayerAdded.Invoke( stat );
        }


        private void MulticastRemoveStats( StatsBase stat )
        {
            stats.Remove( stat );
            onPlayerRemoved.Invoke( stat );
        }


        private void MulticastActivateStats( StatsBase stat )
        {
            OnStatsActivated.Invoke( stat );
        }


        private void MulticastDeactivateStats( StatsBase stat )
        {
            onStatsDeactivated.Invoke( stat );
        }
    }

    [System.Serializable]
    public class GameStateWeakReference : WeakUnityReference<GameState>
    {
    }
}

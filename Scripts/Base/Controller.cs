using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class Controller : Actor
    {


        // Public:


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController == null ? this : responsibleController );

            if( IsAuthority )
            {
                Stats            = ContainingWorld.GameState.GetStatsForController( this );
            }

            RegisterRPC<ControllableActor>( MulticastPossess );
            RegisterRPC( MulticastUnpossess );
        }


        public override void OnActorDestroy()
        {
            base.OnActorDestroy();
            var gameState = ContainingWorld.GameState;
            if( gameState != null )
            {
                gameState.DeactivatePlayerStats( this );
            }
        }


        public virtual void OnPossessedControllableDeath( Actor killer, StatModifier modifier )
        {
        }


        public virtual void OnPossessedControllableKilledActor( Actor victim, StatModifier modifier )
        {
        }


        public virtual void Possess( ControllableActor controllableActor )
        {
            // Only possess if we have authority over both the player and the controllable.
            if( !IsAuthority || !controllableActor.IsAuthority )
            {
                return;
            }
            if( possessedActor != null )
            {
                Unpossess();
            }

            possessedActor = controllableActor;
            controllableActor.OnPossess( this );

            // No buffering. We can have two types of controllers; Player and AI. AI is server only and Player is between the server and
            // the owner.
            ContainingWorld.RPC( MulticastPossess, RPCType.Multicast, this, controllableActor );
        }


        public virtual void Unpossess()
        {
            // No need to buffer unpossess call, just unbuffer the posses call.
            ContainingWorld.RPC( MulticastUnpossess, RPCType.Multicast, this );

            if( possessedActor != null )
            {
                possessedActor.OnUnpossess();
            }
            possessedActor = null;
        }


        public override Stat GetStat( int nameHash, bool includeChildren )
        {
            var ret = stats == null || !includeChildren ? null : stats.GetStat( nameHash, false );
            return ret != null ? ret : base.GetStat( nameHash, includeChildren );
        }


        public ControllableActor PossessedActor
        {
            get
            {
                return possessedActor;
            }
        }


        public StatsBase Stats
        {
            get
            {
                return stats;
            }
            set
            {
                stats = value;
            }
        }


        public override StatsBase ResponsibleStats
        {
            get
            {
                return Stats;
            }
        }


        // Private:


        private ControllableActor   possessedActor              = null;
        private StatsBase           stats                       = null;


        // RPCs:


        private void MulticastPossess( ControllableActor controllableActor )
        {
            possessedActor = controllableActor;
            possessedActor.OnPossess( this );
        }


        private void MulticastUnpossess()
        {
            if( possessedActor == null )
            {
                NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Gameplay, "Could not unpossess on client actor. There was no possessed actor." );
                return;
            }
            possessedActor.OnUnpossess();
            possessedActor = null;
        }
    }
}

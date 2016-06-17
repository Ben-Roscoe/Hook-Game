using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    [IntGameVarDecl( 0, "TargetKills", LocalisationIds.TeamDeathmatch.TargetKillsGameVarName, 1, 200, 5 ),
     FloatGameVarDecl( 1, "DamageMultiplier", LocalisationIds.TeamDeathmatch.DamageMultiplierGameVarName,  0.1f, 1000.0f, 1.0f ),
     BoolGameVarDecl( 2, "BoolTest", LocalisationIds.MainMenu.CurrentPingTitle, false )]
    public class TeamDeathmatchManager : HookGameMatchManager
    {


        // Public:


        public override void InitialiseGameManager( List<GameVar> gameVars )
        {
            base.InitialiseGameManager( gameVars );

            if( ContainingWorld.useTestData )
            {
                targetKills         = 5;
                damageMultiplier    = 1.0f;
            }
            else
            {
                targetKills         = FindGameVarByName( "TargetKills" ).GetInt();
                damageMultiplier    = FindGameVarByName( "DamageMultiplier" ).GetFloat();
            }

            AddController( null, JoinType.Player );
        }


        public override void OnClientConnect( ClientState client )
        {
            base.OnClientConnect( client );
            AddController( client, JoinType.Player );
        }


        public override void StartPostMatch()
        {
            base.StartPostMatch();
            postMatchTimerHandle =  
                ContainingWorld.TimerSystem.SetTimerHandle( this, OnPostMatchTimerEnd, 0, PostMatchTime );
        }


        public override bool ShouldEndPreMatch()
        {
            return true;
        }


        public override bool ShouldEndInProgress()
        {
            var state = TeamDeathmatchState;
            if( state == null )
            {
                return false;
            }
            return state.GetTeam( TeamType.Blue ).Score >= targetKills || state.GetTeam( TeamType.Red ).Score >= targetKills;
        }


        public override bool ShouldEndPostMatch()
        {
            return base.ShouldEndPostMatch();
        }


        public override SpawnPointActor GetSpawnPoint( Controller controller )
        {
            var extension = TeamDeathmatchExtension;
            if( extension == null )
            {
                return base.GetSpawnPoint( controller );
            }

            var stats     = controller.Stats as TeamDeathmatchPlayerStats;
            if( stats == null )
            {
                return base.GetSpawnPoint( controller );
            }

            for( int i = 0; i < extension.SpawnPoints.Count; ++i )
            {
                if( extension.SpawnPoints[i].Team == stats.TeamType )
                {
                    return extension.SpawnPoints[i];
                }
            }

            return base.GetSpawnPoint( controller );
        }


        public override void OnActorKilled( Actor victim, Actor killer, StatModifier modifier )
        {
            base.OnActorKilled( victim, killer, modifier );


            var victimController = victim.ResponsibleController;
            if( victimController == null )
            {
                return;
            }

            var k = killer.ResponsibleController;
            if( k == null )
            {
                return;
            }
            AwardKillExperience( k, victimController );

            var victimStats = TeamDeathmatchState.GetStats( victimController ) as TeamDeathmatchPlayerStats;
            if( victimStats == null )
            {
                return;
            }

            if( killer == null )
            {
                return;
            }

            var attackerController = killer.ResponsibleController;
            if( attackerController == null )
            {
                return;
            }

            var attackerStats = TeamDeathmatchState.GetStats( attackerController ) as TeamDeathmatchPlayerStats;
            if( attackerStats == null )
            {
                return;
            }

            if( victimStats.TeamType == attackerStats.TeamType )
            {
                return;
            }

            TeamDeathmatchState.ScoreKill( attackerStats.TeamType );
        }


        public override bool CanDealDamage( Actor victim, Actor instigator, StatModifier modifier )
        {
            return base.CanDealDamage( victim, instigator, modifier ) && !TeamDeathmatchState.IsOnSameTeam( victim, instigator );
        }


        public override bool CouldDoAnyDamage( Actor victim, Actor instigator )
        {
            return base.CouldDoAnyDamage( victim, instigator ) && !TeamDeathmatchState.IsOnSameTeam( victim, instigator );
        }


        public override void InitialiseStats( Controller controller, StatsBase stats )
        {
            base.InitialiseStats( controller, stats );
            AllocateTeamNumber( controller );
        }


        public virtual bool CanJoinTeam( Controller controller, TeamType team )
        {
            int desired = TeamDeathmatchState.GetTeamCount( team );
            int other   = TeamDeathmatchState.GetTeamCount( TeamDeathmatchState.OtherTeam( team ) );

            if( desired > other )
            {
                return false;
            }

            return true;
        }


        public virtual void AllocateTeamNumber( Controller controller )
        {
            var tdStats = TeamDeathmatchState.GetStats( controller ) as TeamDeathmatchPlayerStats;
            if( tdStats == null )
            {
                return;
            }

            if( CanJoinTeam( controller, TeamType.Red ) )
            {
                tdStats.TeamType = TeamType.Red;
            }
            else
            {
                tdStats.TeamType = TeamType.Blue;
            }
        }


        public TeamDeathmatchGameState TeamDeathmatchState
        {
            get
            {
                return ContainingWorld.GameState as TeamDeathmatchGameState;
            }
        }


        public TeamDeathmatchExtension TeamDeathmatchExtension
        {
            get
            {
                return GameModeMapExtension as TeamDeathmatchExtension;
            }
        }


        // Private:


        private const float   PostMatchTime = 20.0f;

        private int         targetKills         = 0;
        private float       damageMultiplier    = 1.0f;

        private TimerHandle postMatchTimerHandle = null;


        private void OnPostMatchTimerEnd()
        {
            postMatchTimerHandle = null;
            EndPostMatch();
        }
    }
}

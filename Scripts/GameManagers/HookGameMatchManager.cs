using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using UnityEngine;

namespace Nixin
{
    public abstract class HookGameMatchManager : GameManager
    {


        // Public:


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
                UpdateComponent.UpdateRate = 0.0f;
            }
        }


        public override void InitialiseGameManager( List<GameVar> gameVars )
        {
            base.InitialiseGameManager( gameVars );

            StartPreMatch();
        }


        public override Controller AddController( ClientState client, JoinType type )
        {
            var controller          =  base.AddController( client, type );

            var state = ContainingWorld.GameState as HookGameMatchState;
            if( state != null && state.CurrentMatchState == MatchState.InProgress )
            {
                var characterOwner     = controller.GetComponent<HookCharacterOwnerComponent>();
                if( characterOwner != null )
                {
                    characterOwner.RespawnHookCharacter();
                }
            }

            return controller;
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );

            var state = ContainingWorld.GameState as HookGameMatchState;
            if( state == null )
            {
                return;
            }

            // See if we should change match state.
            if( state.CurrentMatchState == MatchState.PreMatch )
            {
                if( ShouldEndPreMatch() )
                {
                    EndPreMatch();
                }
            }
            else if( state.CurrentMatchState == MatchState.StartCountdown )
            {
                if( matchStartCountdownTimerHandle != null )
                {
                    state.SecondsUntilStart = ( byte )matchStartCountdownTimerHandle.TimeRemaining;
                }
                if( ShouldEndCountdown() )
                {
                    EndCountdown();
                }
            }
            else if( state.CurrentMatchState == MatchState.InProgress )
            {
                if( ShouldEndInProgress() )
                {
                    EndInProgress();
                }
            }
            else if( state.CurrentMatchState == MatchState.PostMatch )
            {
                if( ShouldEndPostMatch() )
                {
                    EndPostMatch();
                }
            }
        }


        public virtual void StartPreMatch()
        {
            var state = ContainingWorld.GameState as HookGameMatchState;
            if( state != null )
            {
                state.CurrentMatchState = MatchState.PreMatch;
            }
        }


        public virtual void EndPreMatch()
        {
            StartCountdown();
        }


        public virtual void StartCountdown()
        {
            var state = ContainingWorld.GameState as HookGameMatchState;
            if( state != null )
            {
                state.CurrentMatchState = MatchState.StartCountdown;
                matchStartCountdownTimerHandle = ContainingWorld.TimerSystem.SetTimerHandle( this, EndCountdown, 0, 1.0f );
            }
        }


        public virtual void EndCountdown()
        {
            ContainingWorld.TimerSystem.RemoveTimerHandle( matchStartCountdownTimerHandle );
            matchStartCountdownTimerHandle = null;

            StartInProgress();
        }


        public virtual void StartInProgress()
        {
            var state = ContainingWorld.GameState as HookGameMatchState;
            if( state != null )
            {
                state.CurrentMatchState = MatchState.InProgress;
            }

            for( int i = 0; i < Controllers.Count; ++i )
            {
                var characterOwner = Controllers[i].GetComponent<HookCharacterOwnerComponent>();
                if( characterOwner != null )
                {
                    characterOwner.RespawnHookCharacter();
                }
            }
        }


        public virtual void EndInProgress()
        {
            StartPostMatch();
        }


        public virtual void StartPostMatch()
        {
            if( !IsAuthority )
            {
                return;
            }

            var state = ContainingWorld.GameState as HookGameMatchState;
            if( state != null )
            {
                state.CurrentMatchState = MatchState.PostMatch;
            }

            // Disable all input.
            for( int i = 0; i < Controllers.Count; ++i )
            {
                var player = Controllers[i] as Player;
                if( player != null )
                {
                    player.DisableRemotePlayerInput();
                }
            }
        }


        public virtual void EndPostMatch()
        {
            if( ContainingWorld.NetworkSystem.IsServer )
            {
                ContainingWorld.ShutdownServer();
            }
            else
            {
                ContainingWorld.DisconnectFromServer();
            }

            var world = ( HookGameWorld )ContainingWorld;
            world.LoadMap( world.MainMenuGameMap );
        }


        public virtual void StartLoadingNextMap()
        {
            var state = ContainingWorld.GameState as HookGameMatchState;
            if( state != null )
            {
                state.CurrentMatchState = MatchState.LoadingNextMap;
            }
        }


        public virtual bool ShouldEndPreMatch()
        {
            return false;
        }


        public virtual bool ShouldEndCountdown()
        {
            return false;
        }


        public virtual bool ShouldEndInProgress()
        {
            return false;
        }


        public virtual bool ShouldEndPostMatch()
        {
            return false;
        }


        public virtual SpawnPointActor GetSpawnPoint( Controller controller )
        {
            return null;
        }


        public virtual void OnActorKilled( Actor victim, Actor killer, StatModifier modifier )
        {
            var controller = victim.ResponsibleController;
            if( controller != null )
            {
                controller.OnPossessedControllableDeath( killer, modifier );
            }

            if( killer != null && killer.ResponsibleController != null )
            {
                killer.ResponsibleController.OnPossessedControllableKilledActor( victim, modifier );
            }
        }


        public virtual void TryModifyStat( Stat stat, StatModifier modifier, bool baseModifier, Actor instigator )
        {
            if( stat.Owner != null && ManagerCanModifyStat( stat, modifier, instigator ) )
            {
                stat.Owner.ModifyStat( stat, modifier, baseModifier, instigator );
            }
        }


        public virtual bool CanRespawn( Controller controller )
        {
            return true;
        }


        public virtual double GetRespawnTime( Controller controller )
        {
            return 10.0f;
        }
        

        public virtual bool ManagerCanModifyStat( Stat stat, StatModifier modifier, Actor instigator )
        {
            // Damage.
            if( stat.StatName == StatDefs.currentHealthName && modifier.Value < 0.0f )
            {
                return CanDealDamage( stat.Owner, instigator, modifier );
            }
            return true;
        }


        public virtual bool CanDealDamage( Actor victim, Actor instigator, StatModifier modifier )
        {
            return true;
        }


        public virtual bool CouldDoAnyDamage( Actor victim, Actor instigator )
        {
            return true;
        }


        public virtual bool CanHeadshot
        {
            get
            {
                return true;
            }
        }


        public virtual bool CanPull
        {
            get
            {
                return true;
            }
        }


        public virtual bool CanGrapple
        {
            get
            {
                return true;
            }
        }


        public TimerHandle MatchStartCountdownTimerHandle
        {
            get
            {
                return matchStartCountdownTimerHandle;
            }
        }


        // Protected:


        protected virtual void AwardKillExperience( Controller killer, Controller victim )
        {
            var experienceStat = killer.GetStat( StatDefs.experienceName.Id, true );
            if( experienceStat == null )
            {
                return;
            }
            experienceStat.AddModifier( new StatModifier( GetKillExperience( killer, victim ), StatModifierTypeDefs.defaultStatModifierType, true, false, true ), false );
        }


        protected virtual float GetKillExperience( Controller killer, Controller victim )
        {
            return 1000.0f;
        }


        // Private:


        // Keep the statistics of every player who has joined this game.
        private Dictionary<long, HookGameMatchPlayerStats> playerStats = new Dictionary<long, HookGameMatchPlayerStats>();

        private TimerHandle matchStartCountdownTimerHandle             = null;
    }
}

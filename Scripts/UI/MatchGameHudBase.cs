using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nixin
{
    public class MatchGameHudBase : HudBase
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


        public override void SetUp( Player localPlayer )
        {
            base.SetUp( localPlayer );

            var matchPlayer = MatchPlayer;
            if( matchPlayer != null )
            {
                healthMagicUI.SetPlayer( MatchPlayer );
                respawnCountDownUI.Player = MatchPlayer;
            }

            var state = ContainingWorld.GameState as HookGameMatchState;
            if( state != null )
            {
                startMatchCountdownUI.State = state;
            }
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );

            var characterOwner = MatchPlayer == null ? null : MatchPlayer.GetComponent<HookCharacterOwnerComponent>();
            if( characterOwner != null )
            {
                // Hide and show respawn count down text.
                if( characterOwner.IsRespawning && !respawnCountDownUI.IsVisible )
                {
                    respawnCountDownUI.Show();
                }
                if( !characterOwner.IsRespawning && respawnCountDownUI.IsVisible )
                {
                    respawnCountDownUI.Hide();
                }
            }

            var state = ContainingWorld.GameState as HookGameMatchState;
            if( state != null )
            {
                startMatchCountdownUI.State = state;
                if( state.CurrentMatchState == MatchState.StartCountdown && !startMatchCountdownUI.IsVisible )
                {
                    startMatchCountdownUI.Show();
                }
                else if( state.CurrentMatchState != MatchState.StartCountdown && startMatchCountdownUI.IsVisible )
                {
                    startMatchCountdownUI.Hide();
                }
                if( state.CurrentMatchState == MatchState.PostMatch && !matchResultsUI.IsVisible && !matchResultsUI.IsTransitioning )
                {
                    matchResultsUI.Show();
                }
                else if( state.CurrentMatchState != MatchState.PostMatch && matchResultsUI.IsVisible )
                {
                    matchResultsUI.Hide();
                }
            }
        }


        public RespawnCountDownUI RespawnCountDownUI
        {
            get
            {
                return respawnCountDownUI;
            }
        }


        public HookGameMatchPlayer MatchPlayer
        {
            get
            {
                return LocalPlayer as HookGameMatchPlayer;
            }
        }


        // Private:


        [SerializeField]
        private HealthMagicUI           healthMagicUI           = null;

        [SerializeField]
        private RespawnCountDownUI      respawnCountDownUI      = null;

        [SerializeField]
        private StartMatchCountdownUI   startMatchCountdownUI   = null;

        [SerializeField, FormerlySerializedAs( "MatchResultsUI" )]
        private UIActor                 matchResultsUI          = null;
    }
}

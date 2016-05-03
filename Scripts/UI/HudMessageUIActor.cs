using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Nixin
{
    public class HudMessageUIActor : UIActor
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


        public void SetUpHudMessage( string msg, Color colour, HudMessageSpawnerComponent spawnerComponent, Player localPlayer )
        {
            text.text                   = msg;
            text.color                  = colour;

            this.spawnerComponent       = spawnerComponent;
            this.localPlayer            = localPlayer;

            currentSeconds              = 0.0f;
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );

            currentSeconds += deltaTime;


            if( localPlayer == null || spawnerComponent == null || localPlayer.CameraActor == null )
            {
                return;
            }
            transform.position = localPlayer.CameraActor.CameraComponent.WorldToScreenPoint( spawnerComponent.MessageOrigin.position ) + new Vector3( xMovement.Evaluate( currentSeconds / totalSeconds ) * totalXDisplacement,
                                                                                                                                                      yMovement.Evaluate( currentSeconds / totalSeconds ) * totalYDisplacement, 0.0f );
            text.color = new Color( text.color.r, text.color.g, text.color.b, alphaCurve.Evaluate( currentSeconds / totalSeconds ) );
            if( currentSeconds > totalSeconds )
            {
                ContainingWorld.DestroyActor( this );
            }
        }


        // Private:


        [SerializeField]
        private Text                            text                = null;

        [SerializeField]
        private AnimationCurve                  yMovement           = null;

        [SerializeField]
        private AnimationCurve                  xMovement           = null;

        [SerializeField]
        private AnimationCurve                  alphaCurve          = null;

        [SerializeField]
        private float                           totalYDisplacement  = 1.0f;

        [SerializeField]
        private float                           totalXDisplacement  = 1.0f;

        [SerializeField]
        private float                           totalSeconds        = 1.0f;

        private float                           currentSeconds      = 0.0f;

        private HudMessageSpawnerComponent      spawnerComponent    = null;
        private Player                          localPlayer         = null;
    }
}

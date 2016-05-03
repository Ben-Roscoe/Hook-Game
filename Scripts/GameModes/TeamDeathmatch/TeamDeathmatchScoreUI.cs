using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Nixin
{
    public class TeamDeathmatchScoreUI : UIActor
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

        public override void OnPostHierarchyInitialise()
        {
            base.OnPostHierarchyInitialise();
            SetState( null );
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );
            SetState( State );
        }


        public TeamDeathmatchGameState State
        {
            get
            {
                return ContainingWorld.GameState as TeamDeathmatchGameState;
            }
        }
        

        // Private:


        [SerializeField]
        private Text                        redScoreText = null;

        [SerializeField]
        private Text                        blueScoreText = null;

        // Keep track of the last score values so we don't have to convert
        // to strings redundantly.
        private Int16                       lastRedScore  = 0;
        private Int16                       lastBlueScore = 0;


        private void SetState( TeamDeathmatchGameState newState )
        {
            if( redScoreText == null || blueScoreText == null )
            {
                return;
            }

            var state = newState;
            if( state == null )
            {
                redScoreText.text   = "0";
                blueScoreText.text  = "0";
                return;
            }

            var red     = state.GetTeam( TeamType.Red );
            var blue    = state.GetTeam( TeamType.Blue );

            if( red.Score != lastRedScore )
            {
                redScoreText.text       = red.Score.ToString();
                lastRedScore            = red.Score;
            }
            if( blue.Score != lastBlueScore )
            {
                blueScoreText.text      = blue.Score.ToString();
                lastBlueScore           = blue.Score;
            }
        }
    }
}

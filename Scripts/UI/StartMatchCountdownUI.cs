using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Nixin
{
    public class StartMatchCountdownUI : UIActor
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


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );

            if( state == null || text == null )
            {
                return;
            }

            text.text = string.Format( ContainingWorld.LocalisationSystem.GetLocalString( 
                LocalisationIds.MatchBase.StartingMatchIn ), state.SecondsUntilStart.ToString() );
        }


        public HookGameMatchState State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }


        // Private:

        private HookGameMatchState      state = null;

        [SerializeField]
        private Text                    text  = null;
    }
}

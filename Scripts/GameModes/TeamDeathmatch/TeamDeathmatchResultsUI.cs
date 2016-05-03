using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nixin
{
    public class TeamDeathmatchResultsUI : UIActorGroup
    {


        // Public:


        public override void OnPostHierarchyInitialise()
        {
            base.OnPostHierarchyInitialise();

            Hide();
            SetShowHideEffect( new FadeInOutUIShowHideEffect( this, 2.0f ) );

            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
                UpdateComponent.UpdateRate      = 0.0f;
            }
        }


        public override void OnStartShow()
        {
            base.OnStartShow();

            // We use a fade in effect, so set fields when we start showing rather than
            // when showing is complete.

            var state = ContainingWorld.GameState as TeamDeathmatchGameState;
            Assert.IsTrue( state != null );

            redTeamScore.text = state.GetTeam( TeamType.Red ).Score.ToString();
            blueTeamScore.text = state.GetTeam( TeamType.Blue ).Score.ToString();

            var winner          = state.GetWinner();
            var winnerName      = ContainingWorld.LocalisationSystem.GetLocalString( winner.LocalisedNameId );
            var winnerMessage   = string.Format( ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.TeamDeathmatch.WinnerAnnouncement ), winnerName );

            winnerText.text = winnerMessage;
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "WinnerText" )]
        private Text winnerText     = null;

        [SerializeField, FormerlySerializedAs( "RedTeamScore" )]
        private Text redTeamScore   = null;

        [SerializeField, FormerlySerializedAs( "BlueTeamScore" )]
        private Text blueTeamScore  = null;
    }
}

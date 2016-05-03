using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nixin
{
    public class ConnectingInformationUI : UIActor
    {


        // Public:


        public override void SetLocalText()
        {
            base.SetLocalText();

            titleText.text          = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.ConnectingAlertTitle );
            informationText.text    = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.ConnectingAlertInformation );

            var cancelButtonText = cancelButton.GetComponentInAllChildren<Text>();
            if( cancelButtonText != null )
            {
                cancelButtonText.text = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.Cancel );
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "TitleText" )]
        private Text            titleText           = null;

        [SerializeField, FormerlySerializedAs( "InformationText" )]
        private Text            informationText     = null;

        [SerializeField, FormerlySerializedAs( "CancelButton" )]
        private Button          cancelButton        = null;
    }
}

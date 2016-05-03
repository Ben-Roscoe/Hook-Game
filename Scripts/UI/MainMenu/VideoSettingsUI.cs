using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nixin
{
    public class VideoSettingsUI : UIActor
    {


        // Public:


        public override void SetLocalText()
        {
            var backText = BackButton.GetComponentInAllChildren<Text>();
            if( backText != null )
            {
                backText.text = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.Back );
            }
        }


        public Button BackButton
        {
            get
            {
                return backButton;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "BackButton" )]
        private Button backButton       = null;
    }
}

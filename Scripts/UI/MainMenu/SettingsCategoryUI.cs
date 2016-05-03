using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nixin
{
    public class SettingsCategoryUI : UIActor
    {


        // Public:


        public override void SetLocalText()
        {
            base.SetLocalText();

            var gameSettingsText = GameSettingsButton.GetComponentInAllChildren<Text>();
            if( gameSettingsText != null )
            {
                gameSettingsText.text = ContainingWorld.LocalisationSystem.GetLocalString( 
                    LocalisationIds.MainMenu.GameSettings );
            }

            var videoSettingsText = VideoSettingsButton.GetComponentInAllChildren<Text>();
            if( videoSettingsText != null )
            {
                videoSettingsText.text = ContainingWorld.LocalisationSystem.GetLocalString( 
                    LocalisationIds.MainMenu.VideoSettings );
            }

            var audioSettingsText = AudioSettingsButton.GetComponentInAllChildren<Text>();
            if( audioSettingsText != null )
            {
                audioSettingsText.text = ContainingWorld.LocalisationSystem.GetLocalString( 
                    LocalisationIds.MainMenu.AudioSettings );
            }

            var backButtonText = BackButton.GetComponentInAllChildren<Text>();
            if( backButtonText != null )
            {
                backButtonText.text = ContainingWorld.LocalisationSystem.GetLocalString( 
                    LocalisationIds.MainMenu.Back );
            }
        }


        public Button GameSettingsButton
        {
            get
            {
                return gameSettingsButton;
            }
        }


        public Button VideoSettingsButton
        {
            get
            {
                return videoSettingsButton;
            }
        }


        public Button AudioSettingsButton
        {
            get
            {
                return audioSettingsButton;
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


        [SerializeField, FormerlySerializedAs( "GameSettingsButton" )]
        private Button              gameSettingsButton      = null;

        [SerializeField, FormerlySerializedAs( "VideoSettingsButton" )]
        private Button              videoSettingsButton     = null;

        [SerializeField, FormerlySerializedAs( "AudioSettingsButton" )]
        private Button              audioSettingsButton     = null;

        [SerializeField, FormerlySerializedAs( "BackButton" )]
        private Button              backButton              = null;
    }
}

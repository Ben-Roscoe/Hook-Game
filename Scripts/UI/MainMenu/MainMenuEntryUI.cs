using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nixin
{
    public class MainMenuEntryUI : UIActor
    {


        // Public:


        public override void SetLocalText()
        {
            base.SetLocalText();

            var hostButtonText = HostButton.GetComponentInAllChildren<Text>();
            if( hostButtonText != null )
            {
                hostButtonText.text = ContainingWorld.LocalisationSystem.GetLocalString( 
                    LocalisationIds.MainMenu.Host );
            }

            var gameListButtonText = GameListButton.GetComponentInAllChildren<Text>();
            if( gameListButtonText != null )
            {
                gameListButtonText.text = ContainingWorld.LocalisationSystem.GetLocalString( 
                    LocalisationIds.MainMenu.GameList );
            }

            var settingsButtonText = SettingsButton.GetComponentInAllChildren<Text>();
            if( settingsButtonText != null )
            {
                settingsButtonText.text = ContainingWorld.LocalisationSystem.GetLocalString( 
                    LocalisationIds.MainMenu.Settings );
            }

            var exitButtonText = ExitButton.GetComponentInAllChildren<Text>();
            if( exitButtonText != null )
            {
                exitButtonText.text = ContainingWorld.LocalisationSystem.GetLocalString( 
                    LocalisationIds.MainMenu.Exit );
            }
        }


        public Button HostButton
        {
            get
            {
                return hostButton;
            }
        }


        public Button GameListButton
        {
            get
            {
                return gameListButton;
            }
        }


        public Button SettingsButton
        {
            get
            {
                return settingsButton;
            }
        }


        public Button ExitButton
        {
            get
            {
                return exitButton;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "HostButton" )]
        private Button          hostButton              = null;

        [SerializeField, FormerlySerializedAs( "GameListButton" )]
        private Button          gameListButton          = null;

        [SerializeField, FormerlySerializedAs( "SettingsButton" )]
        private Button          settingsButton          = null;

        [SerializeField, FormerlySerializedAs( "ExitButton" )]
        private Button          exitButton              = null;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nixin
{
    public class HostGameUI : UIActor
    {


        // Public:


        public override void OnPostHierarchyInitialise()
        {
            base.OnPostHierarchyInitialise();

            HostButton.onClick.AddListener( OnHostButtonPressed );
        }


        public override void OnActorDestroy()
        {
            base.OnActorDestroy();

            HostButton.onClick.RemoveListener( OnHostButtonPressed );
        }


        public override void OnShow()
        {
            base.OnShow();
            gameNameFieldt.text = "";
        }


        public override void SetLocalText()
        {
            var backText = BackButton.GetComponentInAllChildren<Text>();
            if( backText != null )
            {
                backText.text = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.Back );
            }

            var hostText = HostButton.GetComponentInAllChildren<Text>();
            if( hostText != null )
            {
                hostText.text = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.Host );
            }

            gameNameText.text = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.GameNameTitle );
        }


        public Button BackButton
        {
            get
            {
                return backButtont;
            }
        }


        public Button HostButton
        {
            get
            {
                return hostButtont;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "GameNameTextt" )]
        private Text                gameNameText    = null;

        [SerializeField, FormerlySerializedAs( "GameNameFieldt" )]
        private InputField          gameNameFieldt    = null;

        [SerializeField, FormerlySerializedAs( "HostButtont" )]
        private Button              hostButtont       = null;

        [SerializeField, FormerlySerializedAs( "BackButtont" )]
        private Button              backButtont       = null;


        private void OnHostButtonPressed()
        {
            // Start a server.
            ContainingWorld.NetworkSystem.StartServer();

            MainMenuGameManager gameManager = ContainingWorld.GameManager as MainMenuGameManager;
            gameManager.CreateNetworkGame( gameNameFieldt.text );
        }
    }
}

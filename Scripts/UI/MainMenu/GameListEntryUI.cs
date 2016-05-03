using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nixin
{
    public class GameListEntryUI : UIActor
    {



        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

            layoutElementComponent = ConstructDefaultComponent<LayoutElement>( this, "LayoutElementComponent", layoutElementComponent );
            buttonComponent        = ConstructDefaultComponent<Button>( this, "ButtonComponent", buttonComponent );
        }


        public override void OnLocalisationChanged()
        {
            base.OnLocalisationChanged();
            UpdateLocalUI();
        }


        public NetworkGameEntryMetaData NetworkGameEntryMetaData
        {
            get
            {
                return networkGameEntryMetaData;
            }
            set
            {
                networkGameEntryMetaData = value;
                if( networkGameEntryMetaData != null )
                {
                    receivedGameMode = ContainingWorld.ResourceSystem.GetMatchGameModeChunk( networkGameEntryMetaData.GameModeName );
                    receivedMap      = ContainingWorld.ResourceSystem.GetMatchMapChunk( networkGameEntryMetaData.MapName );
                }
                else
                {
                    receivedGameMode = null;
                    receivedMap      = null;
                }
                UpdateUIControls();
            }
        }


        public Text GameNameText
        {
            get
            {
                return gameNameText;
            }
        }


        public Text GameModeNameText
        {
            get
            {
                return gameModeNameText;
            }
        }


        public Text MapNameText
        {
            get
            {
                return mapNameText;
            }
        }


        public Text CurrentPlayersText
        {
            get
            {
                return currentPlayersText;
            }
        }


        public Text CurrentPintText
        {
            get
            {
                return currentPingText;
            }
        }


        public Button ButtonComponent
        {
            get
            {
                return buttonComponent;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "LayoutElementComponent" ), HideInInspector]
        private LayoutElement   layoutElementComponent = null;

        [SerializeField, FormerlySerializedAs( "ButtonComponent" ), HideInInspector]
        private Button          buttonComponent        = null;

        [SerializeField, FormerlySerializedAs( "GameNameText" )]
        private Text            gameNameText        = null;

        [SerializeField, FormerlySerializedAs( "GameModeNameText" )]
        private Text            gameModeNameText    = null;

        [SerializeField, FormerlySerializedAs( "MapNameText" )]
        private Text            mapNameText         = null;

        [SerializeField, FormerlySerializedAs( "CurrentPlayersText" )]
        private Text            currentPlayersText  = null;

        [SerializeField, FormerlySerializedAs( "CurrentPingText" )]
        private Text            currentPingText     = null;

        private NetworkGameEntryMetaData     networkGameEntryMetaData   = null;
        private GameModeChunk                receivedGameMode           = null;
        private MapChunk                     receivedMap                = null;


        private void UpdateUIControls()
        {
            if( NetworkGameEntryMetaData == null )
            {
                gameNameText.text       = "";
                gameModeNameText.text   = "";
                mapNameText.text        = "";
                currentPlayersText.text = "";
                currentPingText.text    = "";
                return;
            }

            gameNameText.text           = NetworkGameEntryMetaData.Name;
            currentPlayersText.text     = NetworkGameEntryMetaData.PlayerCount.ToString();
            currentPingText.text        = NetworkGameEntryMetaData.Ping.ToString();
            UpdateLocalUI();
        }


        private void UpdateLocalUI()
        {
            gameModeNameText.text = receivedGameMode != null ? ContainingWorld.LocalisationSystem.GetLocalString( receivedGameMode.NameToken ) :
                                             ContainingWorld.LocalisationSystem.GetLocalString(  LocalisationIds.MainMenu.UnknownGameMode );

            mapNameText.text     = receivedMap != null ? ContainingWorld.LocalisationSystem.GetLocalString( receivedMap.NameToken ) :
                                             ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.UnknownMap );
        }
    }
}

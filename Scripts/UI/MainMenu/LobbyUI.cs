using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nixin
{
    public class LobbyUI : UIActor
    {


        // Public:


        public override void OnPostHierarchyInitialise()
        {
            base.OnPostHierarchyInitialise();

            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
                UpdateComponent.UpdateRate = 0.1f;
            }

            gameModeDropDown.options.Clear();
            gameModeDropDown.onValueChanged.AddListener( OnGameModeDropDownValueChanged );

            mapDropDown.options.Clear();
            mapDropDown.onValueChanged.AddListener( OnMapDropDownValueChanged );

            StartGameButton.onClick.AddListener( OnStartGameButtonPressed );
            DisconnectButton.onClick.AddListener( OnDisconnectButtonPressed );

            ContainingWorld.OnGameStateCreated.AddHandler( OnGameStateCreated );
        }



        public override void OnActorDestroy()
        {
            base.OnActorDestroy();

            // Remove event listeners.
            gameModeDropDown.onValueChanged.RemoveListener( OnGameModeDropDownValueChanged );
            mapDropDown.onValueChanged.RemoveListener( OnMapDropDownValueChanged );
            StartGameButton.onClick.RemoveListener( OnDisconnectButtonPressed );
            DisconnectButton.onClick.RemoveListener( OnDisconnectButtonPressed );

            ContainingWorld.OnGameStateCreated.RemoveHandler( OnGameStateCreated );
        }


        public override void SetInteractable( bool interactable )
        {
            base.SetInteractable( interactable );

            StartGameButton.interactable    = interactable;
            DisconnectButton.interactable   = interactable;
            mapDropDown.interactable        = interactable;
            gameModeDropDown.interactable   = interactable;
            for( int i = 0; i < gameVarUIs.Count; ++i )
            {
                gameVarUIs[i].SetInteractable( interactable );
            }
        }


        public override void SetLocalText()
        {
            base.SetLocalText();

            var disconnectText = disconnectButton.GetComponentInAllChildren<Text>();
            if( disconnectText != null )
            {
                disconnectText.text = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.Disconnect );
            }

            var startGameText   = startGameButton.GetComponentInAllChildren<Text>();
            if( startGameText != null )
            {
                startGameText.text = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.StartGame );
            }

            for( int i = 0; i < gameModeDropDown.options.Count; ++i )
            {
                gameModeDropDown.options[i].text = ContainingWorld.LocalisationSystem.GetLocalString( 
                    ContainingWorld.ResourceSystem.MatchGameModes[i].NameToken );
            }

            lobbyTitleText.text             = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.LobbyTitle );
            playersTitleText.text           = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.PlayersTitle );
            pingTitleText.text              = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.CurrentPingTitle );

            // Have to do this to refresh the shown value.
            gameModeDropDown.captionText = gameModeDropDown.captionText;

            BuildMapDropDown();
        }


        public override void OnShow()
        {
            base.OnShow();

            gameModeDropDown.options.Clear();
            for( int i = 0; i < ContainingWorld.ResourceSystem.MatchGameModes.Count; ++i )
            {
                gameModeDropDown.options.Add( new Dropdown.OptionData( ContainingWorld.LocalisationSystem.GetLocalString( 
                    ContainingWorld.ResourceSystem.MatchGameModes[i].NameToken ) ) );
            }

            if( gameModeDropDown.options.Count > 0 )
            {
                gameModeDropDown.value = 0;
            }
            else
            {
                gameModeDropDown.captionText.text = "";
            }

            // Have to do this to refresh the shown value.
            gameModeDropDown.captionText = gameModeDropDown.captionText;

            BuildMapDropDown();
            SetMapAndMode();
            BuildGameModeOptions();

            OnGameStateCreated( ContainingWorld.GameState );

            SetInteractable( true );
            if( !ContainingWorld.NetworkSystem.IsAuthoritative )
            {
                mapDropDown.interactable        = false;
                gameModeDropDown.interactable   = false;
                startGameButton.interactable    = false;
            }
        }


        public override void OnHide()
        {
            base.OnHide();

            var lobbyState = ContainingWorld.GameState;
            Assert.IsTrue( lobbyState != null );
            lobbyState.OnStatsActivated.RemoveHandler( OnPlayerAdded );
            lobbyState.OnStatsDeactivated.RemoveHandler( OnPlayerRemoved );
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );
            
            if( !ContainingWorld.NetworkSystem.IsAuthoritative )
            {
                // Check to see if we have to rebuild the game modes.
                MainMenuGameState gameState = ContainingWorld.GameState as MainMenuGameState;
                if( gameState != null )
                {
                    GameModeChunk newMode   = gameState.NetworkGameMetaData.GameMap.Mode;

                    if( oldMode != newMode )
                    {
                        oldMode       = newMode;
                        if( newMode != null )
                        {
                            BuildGameModeOptions();
                        }
                    }
                }
            }
        }


        public Button DisconnectButton
        {
            get
            {
                return disconnectButton;
            }
        }


        public Button StartGameButton
        {
            get
            {
                return startGameButton;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "PlayerListEntryUIPrefab" )]
        private PlayerListEntryUI   playerListEntryUIPrefab = null;

        [SerializeField, FormerlySerializedAs( "BoolGameVarUIPrefab" )]
        private BooleanGameVarUI    boolGameVarUIPrefab     = null;

        [SerializeField, FormerlySerializedAs( "IntGameVarUIPrefab" )]
        private IntGameVarUI        intGameVarUIPrefab      = null;

        [SerializeField, FormerlySerializedAs( "FloatGameVarUIPrefab" )]
        private FloatGameVarUI      floatGameVarUIPrefab    = null;

        [SerializeField, FormerlySerializedAs( "DisconnectButton" )]
        private Button              disconnectButton    = null;

        [SerializeField, FormerlySerializedAs( "StartGameButton" )]
        private Button              startGameButton     = null;

        [SerializeField, FormerlySerializedAs( "GameModeDropDown" )]
        private Dropdown            gameModeDropDown    = null;

        [SerializeField, FormerlySerializedAs( "MapDropDown" )]
        private Dropdown            mapDropDown         = null;

        [SerializeField, FormerlySerializedAs( "MapImage" )]
        private Image               mapImage            = null;

        [SerializeField, FormerlySerializedAs( "LobbyTitleText" )]
        private Text                lobbyTitleText      = null;

        [SerializeField, FormerlySerializedAs( "PlayersTitleText" )]
        private Text                playersTitleText = null;

        [SerializeField, FormerlySerializedAs( "PingTitleText" )]
        private Text                pingTitleText   = null;

        [SerializeField, FormerlySerializedAs( "PlayerListPanel" )]
        private RectTransform       playerListPanel          = null;

        [SerializeField, FormerlySerializedAs( "GameOptionsListPanel" )]
        private RectTransform       gameOptionsListPanel     = null;

        private List<PlayerListEntryUI>    playerEntryUIs       = new List<PlayerListEntryUI>();
        private List<GameVarUI>            gameVarUIs           = new List<GameVarUI>();
        private List<MapChunk>             availableMaps        = new List<MapChunk>();

        private GameModeChunk              oldMode              = null;


        private void OnStartGameButtonPressed()
        {
            var gameState = ContainingWorld.GameState as MainMenuGameState;
            if( !gameState.CanStartNetworkGame() )
            {
                return;
            }
            SetInteractable( false );
            ContainingWorld.LoadMap( gameState.NetworkGameMetaData.GameMap );
        }


        private void OnDisconnectButtonPressed()
        {
            ContainingWorld.NetworkSystem.Disconnect();
            ClearPlayerEntryList();
        }


        private void BuildMapDropDown()
        {
            availableMaps.Clear();
            mapDropDown.options.Clear();
            mapDropDown.captionText.text = "";

            if( gameModeDropDown.options.Count <= 0 )
            {
                mapDropDown.captionText.text = ContainingWorld.LocalisationSystem.GetLocalString( 
                    LocalisationIds.MainMenu.NoMapAvailable );
                return;
            }

            var gameMode = ContainingWorld.ResourceSystem.MatchGameModes[gameModeDropDown.value];
            if( gameMode == null )
            {
                mapDropDown.captionText.text = ContainingWorld.LocalisationSystem.GetLocalString( 
                    LocalisationIds.MainMenu.NoMapAvailable );
                return;
            }

            for( int i = 0; i < ContainingWorld.ResourceSystem.MatchMaps.Count; ++i )
            {
                if( ContainingWorld.ResourceSystem.MatchMaps[i].HasExtension( gameMode ) )
                {
                    availableMaps.Add( ContainingWorld.ResourceSystem.MatchMaps[i] );
                }
            }
            if( availableMaps.Count <= 0 )
            {
                mapDropDown.captionText.text = ContainingWorld.LocalisationSystem.GetLocalString( 
                    LocalisationIds.MainMenu.NoMapAvailable );
                return;
            }
            else
            {
                mapDropDown.value = 0;
            }
            for( int i = 0; i < availableMaps.Count; ++i )
            {
                mapDropDown.options.Add( new Dropdown.OptionData( ContainingWorld.LocalisationSystem.GetLocalString( 
                    availableMaps[i].NameToken ) ) );
            }

            // This will refresh the shown value.
            mapDropDown.captionText = mapDropDown.captionText;
        }


        private void BuildGameModeOptions()
        {
            ClearGameModeOptions();
            MainMenuGameState state = ContainingWorld.GameState as MainMenuGameState;
            GameModeChunk      mode = state.NetworkGameMetaData.GameMap.Mode;

            if( mode != null )
            { 
                // Build game var uis.
                for( int i = 0; i < mode.GameVarDelcs.Count; ++i )
                {
                    GameVarDeclAttribute         decl        = mode.GameVarDelcs[i];
                    GameVarUI                    gameVarUI   = null;

                    if( decl is BoolGameVarDeclAttribute )
                    {
                        gameVarUI = ( GameVarUI )ContainingWorld.InstantiateUIActor( boolGameVarUIPrefab, null );
                    }
                    else if( decl is IntGameVarDeclAttribute )
                    {
                        gameVarUI = ( GameVarUI )ContainingWorld.InstantiateUIActor( intGameVarUIPrefab, null );
                    }
                    else if( decl is FloatGameVarDeclAttribute )
                    {
                        gameVarUI = ( GameVarUI )ContainingWorld.InstantiateUIActor( floatGameVarUIPrefab, null );
                    }

                    gameVarUI.SetGameVarData( decl, state.NetworkGameMetaData.GameMap.Vars[i] );
                    gameVarUI.transform.localPosition = Vector3.zero;
                    gameVarUI.transform.localScale    = Vector3.one;
                    gameVarUI.transform.SetParent( gameOptionsListPanel, false );

                    gameVarUIs.Add( gameVarUI );
                }
            }
        }


        private void ClearGameModeOptions()
        {
            gameOptionsListPanel.DetachChildren();
            for( int i = 0; i < gameVarUIs.Count; ++i )
            {
                ContainingWorld.DestroyActor( gameVarUIs[i] );
            }
            gameVarUIs.Clear();
        }


        private void OnMapDropDownValueChanged( int i )
        {
            if( ContainingWorld.NetworkSystem.IsAuthoritative )
            {
                SetMapAndMode();
                MainMenuGameState state = ContainingWorld.GameState as MainMenuGameState;
            }
        }


        private void OnGameModeDropDownValueChanged( int i )
        {
            if( ContainingWorld.NetworkSystem.IsAuthoritative )
            {
                SetMapAndMode();
                BuildMapDropDown();
                BuildGameModeOptions();
            }
        }


        private void SetMapAndMode()
        {
            MainMenuGameState state = ContainingWorld.GameState as MainMenuGameState;
            if( gameModeDropDown.options.Count <= 0 )
            {
                state.NetworkGameMetaData.GameMap.Mode = null;
            }
            else
            {
                state.NetworkGameMetaData.GameMap.Mode = ContainingWorld.ResourceSystem.MatchGameModes[gameModeDropDown.value];
            }

            if( mapDropDown.options.Count <= 0 )
            {
                state.NetworkGameMetaData.GameMap.Map   = null;
                mapImage.sprite                 = null;
            }
            else
            {
                state.NetworkGameMetaData.GameMap.Map = availableMaps[mapDropDown.value];
                mapImage.sprite                        = availableMaps[mapDropDown.value].Icon;
            }
        }


        private void ClearPlayerEntryList()
        {
            playerListPanel.DetachChildren();
            for( int i = 0; i < playerEntryUIs.Count; ++i )
            {
                ContainingWorld.DestroyActor( playerEntryUIs[i] );
            }
            playerEntryUIs.Clear();
        }


        private void OnGameStateCreated( GameState state )
        {
            ClearPlayerEntryList();
            if( state == null )
            {
                return;
            }

            state.OnStatsActivated.AddHandler( OnPlayerAdded );
            state.OnStatsDeactivated.AddHandler( OnPlayerRemoved );

            // Add initial players.
            for( int i = 0; i < state.PlayerStats.Count; ++i )
            {
                if( state.PlayerStats[i].IsActive )
                {
                    OnPlayerAdded( state.PlayerStats[i] );
                }
            }

            MainMenuGameState mainMenuState = ( MainMenuGameState )state;
            oldMode = mainMenuState.NetworkGameMetaData.GameMap.Mode;
        }


        private void OnPlayerAdded( StatsBase stats )
        {
            var lobbyStats  = ( MainMenuPlayerStats )stats;
            var playerEntry = ( PlayerListEntryUI )ContainingWorld.InstantiateUIActor( playerListEntryUIPrefab, null );
            playerEntry.PlayerMetaData = lobbyStats.LobbyMetaData;

            // Attach the entry to the list.
            playerEntry.transform.localPosition = Vector3.zero;
            playerEntry.transform.localScale    = Vector3.one;
            playerEntry.transform.SetParent( playerListPanel, false );

            playerEntryUIs.Add( playerEntry );
        }


        private void OnPlayerRemoved( StatsBase stats )
        {
            var lobbyStats  = ( MainMenuPlayerStats )stats;
            var playerEntry = playerEntryUIs.Find( x => x.PlayerMetaData == lobbyStats.LobbyMetaData );
            if( playerEntry == null )
            {
                NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Gameplay, "Player removed but no entry was found in UI." );
                return;
            }

            playerEntryUIs.Remove( playerEntry );

            playerEntry.transform.SetParent( null, true );
            ContainingWorld.DestroyActor( playerEntry );
        }
    }
}

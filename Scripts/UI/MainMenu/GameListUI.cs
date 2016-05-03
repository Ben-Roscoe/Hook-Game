using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nixin
{
    public enum GameListSortBy
    {
        GameName        = 0,
        GameMode        = 1,
        Map             = 2,
        PlayerCount     = 3,
        Ping            = 4,
    }
    public enum GameListSortDirection
    {
        Ascending       = 0,
        Descending      = 1,
    }
    public class GameListUI : UIActor
    {


        // Public:



        public override void OnPostHierarchyInitialise()
        {
            base.OnPostHierarchyInitialise();

            var localManager = ContainingWorld.LocalGameManager as MainMenuLocalGameManager;
            localManager.OnGetRunningGameMetaDataComplete.AddHandler( OnGameEntriesArrived );


            sortByDropDown.options.Clear();
            sortDirectionDropDown.options.Clear();
            for( int i = 0; i < 5; ++i )
            {
                sortByDropDown.options.Add( new Dropdown.OptionData() );
            }
            for( int i = 0; i < 2; ++i )
            {
                sortDirectionDropDown.options.Add( new Dropdown.OptionData() );
            }

            sortByDropDown.onValueChanged.AddListener( OnSortDropDownValueChanged );
            sortDirectionDropDown.onValueChanged.AddListener( OnSortDropDownValueChanged );

            refreshButton.onClick.AddListener( OnRefreshButtonPressed );
            joinButton.onClick.AddListener( OnJoinButtonPressed );

            // Test list.
            var testList = new List<NetworkGameEntryMetaData>();
            for( int i = 0; i < 30; ++i )
            {
                var test = new NetworkGameEntryMetaData( "game" + i.ToString(), ( short )i, "TeamDeathmatch", "mapMeta" );
                test.Ping = i;
                test.PlayerCount = ( short )( 30 - i );
                testList.Add( test );
            }
            OnGameEntriesArrived( testList );
        }


        public override void OnShow()
        {
            base.OnShow();

            joinButton.interactable         = false;
            gameListEntryInfoUI.EntryMeta   = null;
            RefreshEntries();
        }


        public override void SetLocalText()
        {
            var backText = BackButton.GetComponentInAllChildren<Text>();
            if( backText != null )
            {
                backText.text = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.Back );
            }

            var refreshText = refreshButton.GetComponentInAllChildren<Text>();
            if( refreshText != null )
            {
                refreshText.text = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.Refresh );
            }

            var joinText = joinButton.GetComponentInAllChildren<Text>();
            if( joinText != null )
            {
                joinText.text = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.Join );
            }

            var localGameNameTitleText          = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.GameNameTitle ).ToUpper();
            var localGameModeNameTitleText      = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.GameModeNameTitle ).ToUpper();
            var localMapNameTitleText           = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.MapNameTitle ).ToUpper();
            var localCurrentPlayersTitleText    = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.CurrentPlayersTitle ).ToUpper();
            var localCurrentPingTitleText       = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.CurrentPingTitle ).ToUpper();

            gameNameTitleText.text          = localGameNameTitleText;
            gameModeTitleText.text          = localGameModeNameTitleText;
            mapNameTitleText.text           = localMapNameTitleText;
            currentPlayersTitleText.text    = localCurrentPlayersTitleText;
            currentPingTitleText.text       = localCurrentPingTitleText;

            sortByDropDown.options[0].text = localGameNameTitleText;
            sortByDropDown.options[1].text = localGameModeNameTitleText;
            sortByDropDown.options[2].text = localMapNameTitleText;
            sortByDropDown.options[3].text = localCurrentPlayersTitleText;
            sortByDropDown.options[4].text = localCurrentPingTitleText;

            sortDirectionDropDown.options[0].text  = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.AscendingSortDirection );
            sortDirectionDropDown.options[1].text  = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.DescendingSortDirection );

            // Refresh the captions.
            sortByDropDown.captionText          = sortByDropDown.captionText;
            sortDirectionDropDown.captionText   = sortDirectionDropDown.captionText;
        }


        public Button BackButton
        {
            get
            {
                return backButton;
            }
        }


        public Button JoinButton
        {
            get
            {
                return joinButton;
            }
        }


        public NixinEvent OnJoinedGame
        {
            get
            {
                return onJoinedGame;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "GameListEntryUIPrefab" )]
        private GameListEntryUI     gameListEntryUIPrefab   = null;

        [SerializeField, FormerlySerializedAs( "GameListEntryInfoUI" )]
        private GameListEntryInfoUI gameListEntryInfoUI     = null;

        [SerializeField, FormerlySerializedAs( "SortByDropDown" )]
        private Dropdown            sortByDropDown          = null;

        [SerializeField, FormerlySerializedAs( "SortDirectionDropDown" )]
        private Dropdown            sortDirectionDropDown   = null;

        [SerializeField, FormerlySerializedAs( "BackButton" )]
        private Button              backButton           = null;

        [SerializeField, FormerlySerializedAs( "RefreshButton" )]
        private Button              refreshButton        = null;

        [SerializeField, FormerlySerializedAs( "JoinButton" )]
        private Button              joinButton          = null;

        [SerializeField, FormerlySerializedAs( "ListScrollRect" )]
        private RectTransform       listPanel            = null;

        [SerializeField, FormerlySerializedAs( "GameNameTitleText" )]
        private Text                gameNameTitleText   = null;

        [SerializeField, FormerlySerializedAs( "GameModeTitleText" )]
        private Text                gameModeTitleText   = null;

        [SerializeField, FormerlySerializedAs( "MapNameTitleText" )]
        private Text                mapNameTitleText    = null;

        [SerializeField, FormerlySerializedAs( "CurrentPlayersTitleText" )]
        private Text                currentPlayersTitleText = null;

        [SerializeField, FormerlySerializedAs( "CurrentPingTitleText" )]
        private Text                currentPingTitleText    = null;

        private List<GameListEntryUI> gameListEntryUIs      = new List<GameListEntryUI>();
        private MessageBoxUI          connectingMessageBox  = null;

        private NixinEvent            onJoinedGame          = new NixinEvent();


        private void OnSortDropDownValueChanged( int i )
        {
            SortEntries();
        }


        private void OnRefreshButtonPressed()
        {
            RefreshEntries();
        }


        private void RefreshEntries()
        {
             var localManager = ContainingWorld.LocalGameManager as MainMenuLocalGameManager;
             localManager.GetRunningGameMetaData();
        }


        private void OnGameEntriesArrived( List<NetworkGameEntryMetaData> receivedEntries )
        {
            listPanel.DetachChildren();
            for( int i = 0; i < gameListEntryUIs.Count; ++i )
            {
                ContainingWorld.DestroyActor( gameListEntryUIs[i] );
            }
            gameListEntryUIs.Clear();

            for( int i = 0; i < receivedEntries.Count; ++i )
            {
                var entry = ( GameListEntryUI )ContainingWorld.InstantiateUIActor( gameListEntryUIPrefab, null );
                entry.NetworkGameEntryMetaData = receivedEntries[i];
                entry.ButtonComponent.onClick.AddListener( () => OnEntrySelected( entry ) );
                gameListEntryUIs.Add( entry );
            }
            SortEntries();
        }


        private void SortEntries()
        {
            listPanel.DetachChildren();


            var sortBy              = ( GameListSortBy )sortByDropDown.value;
            var sortDirection       = ( GameListSortDirection )sortDirectionDropDown.value;

            switch( sortBy )
            {
                case GameListSortBy.GameName:
                    {
                        gameListEntryUIs = gameListEntryUIs.OrderBy( x => x.GameNameText.text ).ToList();
                        break;
                    }
                case GameListSortBy.GameMode:
                    {
                        gameListEntryUIs = gameListEntryUIs.OrderBy( x => x.GameModeNameText.text  ).ToList();
                        break;
                    }
                case GameListSortBy.Map:
                    {
                        gameListEntryUIs = gameListEntryUIs.OrderBy( x => x.MapNameText.text ).ToList();
                        break;
                    }
                case GameListSortBy.PlayerCount:
                    {
                        gameListEntryUIs = gameListEntryUIs.OrderBy( x => x.NetworkGameEntryMetaData.PlayerCount ).ToList();
                        break;
                    }
                case GameListSortBy.Ping:
                default:
                    {
                        gameListEntryUIs = gameListEntryUIs.OrderBy( x => x.NetworkGameEntryMetaData.Ping ).ToList();
                        break;
                    }
            }

            if( sortDirection == GameListSortDirection.Descending )
            {
                gameListEntryUIs.Reverse();
            }

            AddEntriesToPanel();
        }


        private void AddEntriesToPanel()
        {
            listPanel.DetachChildren();
            for( int i = 0; i < gameListEntryUIs.Count; ++i )
            {
                gameListEntryUIs[i].transform.localPosition = Vector3.zero;
                gameListEntryUIs[i].transform.localScale    = Vector3.one;
                gameListEntryUIs[i].transform.SetParent( listPanel, false );
            }
        }


        private void OnEntrySelected( GameListEntryUI entry )
        {
            joinButton.interactable       = entry != null;
            gameListEntryInfoUI.EntryMeta = entry.NetworkGameEntryMetaData;
        }


        private void OnJoinButtonPressed()
        {
            if( gameListEntryUIs.Count > 0 )
            {
                var localManager        = ContainingWorld.LocalGameManager as MainMenuLocalGameManager;
                var request             = localManager.JoinLobby( gameListEntryUIs[0].NetworkGameEntryMetaData );
                connectingMessageBox    = ( ( HookGameWorld )ContainingWorld ).CreateMessageBoxUI( LocalisationIds.MainMenu.ConnectingAlertTitle, LocalisationIds.MainMenu.ConnectingAlertInformation, 
                                                                                                   LocalisationIds.MainMenu.Cancel, ContainingWorld.DefaultCanvas );

                request.OnCompleted.AddHandler( OnConnectionRequestCompleted );
            }
        }


        private void OnConnectionRequestCompleted( ConnectToServerRequest request )
        {
            ContainingWorld.DestroyActor( connectingMessageBox );
            onJoinedGame.Invoke();
        }
    }
}

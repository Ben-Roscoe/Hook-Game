using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nixin
{
    public class MainMenuUI : UIMenu
    {


        // Public:


        public override void OnPostHierarchyInitialise()
        {
            base.OnPostHierarchyInitialise();

            hostGameUI.Hide();
            gameListUI.Hide();
            settingsCategoryUI.Hide();
            gameSettingsUI.Hide();
            videoSettingsUI.Hide();
            audioSettingsUI.Hide();
            lobbyUI.Hide();

            mainMenuEntryUI.Show();

            // Entry <-> Host Game
            AddTransition( new UIButtonTransition( mainMenuEntryUI, hostGameUI, mainMenuEntryUI.HostButton ) );
            AddTransition( new UIButtonTransition( hostGameUI, mainMenuEntryUI, hostGameUI.BackButton ) );

            // Entry <-> Game List
            AddTransition( new UIButtonTransition( mainMenuEntryUI, gameListUI, mainMenuEntryUI.GameListButton ) );
            AddTransition( new UIButtonTransition( gameListUI, mainMenuEntryUI, gameListUI.BackButton ) );

            // Entry <-> Settings Category
            AddTransition( new UIButtonTransition( mainMenuEntryUI, settingsCategoryUI, mainMenuEntryUI.SettingsButton ) );
            AddTransition( new UIButtonTransition( settingsCategoryUI, mainMenuEntryUI, settingsCategoryUI.BackButton ) );

            // Settings Category <-> Game Settings
            AddTransition( new UIButtonTransition( settingsCategoryUI, gameSettingsUI, settingsCategoryUI.GameSettingsButton ) );
            AddTransition( new UIButtonTransition( gameSettingsUI, settingsCategoryUI, gameSettingsUI.BackButton ) );

            // Settings Category <-> Video Settings
            AddTransition( new UIButtonTransition( settingsCategoryUI, videoSettingsUI, settingsCategoryUI.VideoSettingsButton ) );
            AddTransition( new UIButtonTransition( videoSettingsUI, settingsCategoryUI, videoSettingsUI.BackButton ) );

            // Setting Category <-> Audio Settings
            AddTransition( new UIButtonTransition( settingsCategoryUI, audioSettingsUI, settingsCategoryUI.AudioSettingsButton ) );
            AddTransition( new UIButtonTransition( audioSettingsUI, settingsCategoryUI, audioSettingsUI.BackButton ) );

            // Host Game -> Lobby
            AddTransition( new UIButtonTransition( hostGameUI, lobbyUI, hostGameUI.HostButton ) );

            // Game List <-> Lobby
            AddTransition( new NixinEventUITransition( gameListUI, lobbyUI, gameListUI.OnJoinedGame ) );
            AddTransition( new UIButtonTransition( lobbyUI, gameListUI, lobbyUI.DisconnectButton ) );
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "MainMenuEntryUI" ) ]
        private MainMenuEntryUI             mainMenuEntryUI         = null;

        [SerializeField, FormerlySerializedAs( "HostGameUI" ) ]
        private HostGameUI                  hostGameUI              = null;

        [SerializeField, FormerlySerializedAs( "GameListUI" ) ]
        private GameListUI                  gameListUI              = null;

        [SerializeField, FormerlySerializedAs( "LobbyUI" )]
        private LobbyUI                     lobbyUI                 = null;


        [SerializeField, FormerlySerializedAs( "SettingsCategoryUI" ) ]
        private SettingsCategoryUI          settingsCategoryUI      = null;

        [SerializeField, FormerlySerializedAs( "GameSettingsUI" ) ]
        private GameSettingsUI              gameSettingsUI          = null;

        [SerializeField, FormerlySerializedAs( "VideoSettingsUI" ) ]
        private VideoSettingsUI             videoSettingsUI         = null;

        [SerializeField, FormerlySerializedAs( "AudioSettingsUI" ) ]
        private AudioSettingsUI             audioSettingsUI         = null;
    }
}

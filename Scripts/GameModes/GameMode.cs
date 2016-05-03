using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class GameMode
    {


        // Public:


        public GameMode( GameManager managerPrefab, LocalGameManager localManagerPrefab, GameState statePrefab, StatsBase statsPrefab, HudBase hudPrefab, GameModeMapExtension extensionPrefab )
        {
            this.managerPrefab      = managerPrefab;
            this.localManagerPrefab = localManagerPrefab;
            this.statePrefab        = statePrefab;
            this.statsPrefab        = statsPrefab;
            this.hudPrefab          = hudPrefab;
            this.extensionPrefab    = extensionPrefab;
        }


        public GameMode( GameModeChunk gameModeChunk, MapChunk mapChunk )
        {
            managerPrefab      = gameModeChunk.ManagerPrefab.GetRuntimeObject( gameModeChunk.Header.ResourceSystem );
            localManagerPrefab = gameModeChunk.LocalManagerPrefab.GetRuntimeObject( gameModeChunk.Header.ResourceSystem );
            statePrefab        = gameModeChunk.StatePrefab.GetRuntimeObject( gameModeChunk.Header.ResourceSystem );
            statsPrefab        = gameModeChunk.StatsPrefab.GetRuntimeObject( gameModeChunk.Header.ResourceSystem );
            hudPrefab          = gameModeChunk.HudPrefab.GetRuntimeObject( gameModeChunk.Header.ResourceSystem );

            var extensionChunk = mapChunk.GetExtension( gameModeChunk );

            // Might not need an extension.
            if( extensionChunk != null )
            {
                extensionPrefab = extensionChunk.ExtensionPrefab.GetRuntimeObject( gameModeChunk.Header.ResourceSystem );
            }
        }


        public GameManager ManagerPrefab
        {
            get
            {
                return managerPrefab;
            }
        }


        public LocalGameManager LocalManagerPrefab
        {
            get
            {
                return localManagerPrefab;
            }
        }


        public GameState StatePrefab
        {
            get
            {
                return statePrefab;
            }
        }


        public StatsBase StatsPrefab
        {
            get
            {
                return statsPrefab;
            }
        }


        public HudBase HudPrefab
        {
            get
            {
                return hudPrefab;
            }
        }


        public GameModeMapExtension ExtensionPrefab
        {
            get
            {
                return extensionPrefab;
            }
        }


        // Private:


        private GameManager             managerPrefab           = null;
        private LocalGameManager        localManagerPrefab      = null;
        private GameState               statePrefab             = null;
        private StatsBase               statsPrefab             = null;
        private HudBase                 hudPrefab               = null;

        private GameModeMapExtension    extensionPrefab         = null;
    }
}

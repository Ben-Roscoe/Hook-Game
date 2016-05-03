using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nixin
{
    public class GameListEntryInfoUI : UIActor
    {


        // Public:


        public override void SetLocalText()
        {
            base.SetLocalText();

            gameNameTitleText.text          = ContainingWorld.LocalisationSystem.GetLocalString( 
                LocalisationIds.MainMenu.GameNameTitle ).ToUpper();
            gameModeNameTitleText.text      = ContainingWorld.LocalisationSystem.GetLocalString( 
                LocalisationIds.MainMenu.GameModeNameTitle ).ToUpper();
            mapNameTitleText.text           = ContainingWorld.LocalisationSystem.GetLocalString( 
                LocalisationIds.MainMenu.MapNameTitle ).ToUpper();
            currentPlayersTitleText.text    = ContainingWorld.LocalisationSystem.GetLocalString( 
                LocalisationIds.MainMenu.CurrentPlayersTitle ).ToUpper();

            if( entryMeta == null )
            {
                gameModeNameText.text = "-";
                mapNameText.text      = "-";
                return;
            }

            gameModeNameText.text = mode == null ? ContainingWorld.LocalisationSystem.GetLocalString( 
                LocalisationIds.MainMenu.UnknownGameMode ) : ContainingWorld.LocalisationSystem.GetLocalString( mode.NameToken );
            mapNameText.text = map == null ? ContainingWorld.LocalisationSystem.GetLocalString( 
                LocalisationIds.MainMenu.UnknownMap ) : ContainingWorld.LocalisationSystem.GetLocalString( map.NameToken );
        }


        public NetworkGameEntryMetaData EntryMeta
        {
            get
            {
                return entryMeta;
            }
            set
            {
                entryMeta = value;
                if( entryMeta != null )
                {
                    map     = ContainingWorld.ResourceSystem.GetMatchMapChunk( entryMeta.MapName );
                    mode    = ContainingWorld.ResourceSystem.GetMatchGameModeChunk( entryMeta.GameModeName );
                }
                SetText();
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "MapImage" )]
        private Image           mapImage                = null;

        [SerializeField, FormerlySerializedAs( "GameNameTitleText" )]
        private Text            gameNameTitleText       = null;

        [SerializeField, FormerlySerializedAs( "GameNameText" )]
        private Text            gameNameText            = null;

        [SerializeField, FormerlySerializedAs( "GameModeTitleText" )]
        private Text            gameModeNameTitleText   = null;

        [SerializeField, FormerlySerializedAs( "GameModeNameText" )]
        private Text            gameModeNameText        = null;

        [SerializeField, FormerlySerializedAs( "MapNameTitleText" )]
        private Text            mapNameTitleText        = null;

        [SerializeField, FormerlySerializedAs( "MapNameText" )]
        private Text            mapNameText             = null;

        [SerializeField, FormerlySerializedAs( "CurrentPlayersTitleText" )]
        private Text            currentPlayersTitleText = null;

        [SerializeField, FormerlySerializedAs( "CurrentPlayersText" )]
        private Text            currentPlayersText      = null;

        private NetworkGameEntryMetaData    entryMeta   = null;
        private MapChunk                    map         = null;
        private GameModeChunk               mode        = null;


        private void SetText()
        {
            SetLocalText();

            if( entryMeta == null )
            {
                mapImage.sprite             = null;
                gameNameText.text           = "-";
                gameModeNameText.text       = "-";
                mapNameText.text            = "-";
                currentPlayersText.text     = "-";
                return;
            }

            mapImage.sprite             = entryMeta.MapIcon;
            gameNameText.text           = entryMeta.Name;
            currentPlayersText.text     = entryMeta.PlayerCount.ToString();
        }
    }
}

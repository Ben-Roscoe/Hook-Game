using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Nixin
{
    public class HookGameWorld : World
    {


        // Public:


        public MessageBoxUI CreateMessageBoxUI( string titleTextId, string bodyTextId, string okayButtonTextId, UICanvasActor canvas )
        {
            MessageBoxUI ui = ( MessageBoxUI )InstantiateUIActor( messageBoxUIPrefab, canvas );
            ui.SetLocalIds( titleTextId, bodyTextId, okayButtonTextId );
            return ui;
        }


        public override string AppId
        {
            get
            {
                return "Nixin-HookGame";
            }
        }


        public override string DataPath
        {
            get
            {
                return Application.dataPath + "/BuildData/Data/";
            }
        }


        public override string LocalisationPath
        {
            get
            {
                return Application.dataPath + "/BuildData/Localisation/";
            }
        }


        public SelectableOutlineOptions SelectableOutlineOptions
        {
            get
            {
                return selectableOutlineOptions;
            }
        }


        public GameMap MainMenuGameMap
        {
            get
            {
                return mainMenuGameMap;
            }
        }


        // Protected:


        protected override void WorldInitialise()
        {
            base.WorldInitialise();

            var mapMeta     = ResourceSystem.GetNonMatchMapChunk( "MainMenuMap_Chunk" );
            var gameMode    = ResourceSystem.GetNonMatchGameModeChunk( "MainMenuMode_Chunk" );
            mainMenuGameMap = new GameMap( mapMeta, gameMode );

            NetworkSystem.StartClient();
            if( !useTestData )
            {
                LoadMap( mainMenuGameMap );
            }
        }

#if false
        protected override void OnMapFinishLoad( MapTransition transition )
        {
            base.OnMapFinishLoad( transition );
            if( transition.Status == MapTransitionStatus.Complete )
            {
                if( mainMenuNext )
                {
                    var mapMeta     = ResourceSystem.GetNonMatchMapChunk( "MainMenuMap_Chunk" );
                    var gameMode    = ResourceSystem.GetNonMatchGameModeChunk( "MainMenuMode_Chunk" );

                    GameMap     testMap = new GameMap( mapMeta, gameMode );
                    LoadMap( testMap );
                }
                else
                {
                    var mapMeta     = ResourceSystem.GetMatchMapChunk( "RiverCrossingMap_Chunk" );
                    var gameMode    = ResourceSystem.GetMatchGameModeChunk( "TeamDeathmatch_Chunk" );

                    GameMap     testMap = new GameMap( mapMeta, gameMode );
                    LoadMap( testMap );
                }
                mainMenuNext = !mainMenuNext;
            }
        }
#endif


        // Private:
        bool mainMenuNext = false;

        // Generic message box for displaying information. Must be closed before other ui is interactable.
        [SerializeField, FormerlySerializedAs( "MessageBoxUIPrefab" )]
        private MessageBoxUI                messageBoxUIPrefab          = null;

        [SerializeField, FormerlySerializedAs( "SelectableOutlineOptions" )]
        private SelectableOutlineOptions    selectableOutlineOptions    = new SelectableOutlineOptions();

        private GameMap                     mainMenuGameMap = null;
    }


    [System.Serializable]
    public class SelectableOutlineOptions
    {


        // Public:


        public Color GetHoverColour( SelectableType type )
        {
            switch( type )
            {
                case SelectableType.Good:       return goodHoverColour;
                case SelectableType.Bad:        return badHoverColour;
                case SelectableType.Neutral:    return neutralHoverColour;
            }
            return neutralHoverColour;
        }


        public Color GetSelectColour( SelectableType type )
        {
            switch( type )
            {
                case SelectableType.Good:       return goodSelectColour;
                case SelectableType.Bad:        return badSelectColour;
                case SelectableType.Neutral:    return neutralSelectColour;
            }
            return neutralSelectColour;
        }


        public float NoneHoverLerpTime
        {
            get
            {
                return noneHoverLerpTime;
            }
            set
            {
                noneHoverLerpTime = value;
            }
        }


        public float HoverSelectLerpTime
        {
            get
            {
                return hoverSelectLerpTime;
            }
            set
            {
                hoverSelectLerpTime = value;
            }
        }


        public float SelectStayTime
        {
            get
            {
                return selectStayTime;
            }
            set
            {
                selectStayTime = value;
            }
        }


        public Color GoodHoverColour
        {
            get
            {
                return goodHoverColour;
            }
            set
            {
                goodHoverColour = value;
            }
        }


        public Color BadHoverColour
        {
            get
            {
                return badHoverColour;
            }
            set
            {
                badHoverColour = value;
            }
        }


        public Color NeutralHoverColour
        {
            get
            {
                return neutralHoverColour;
            }
            set
            {
                neutralHoverColour = value;
            }
        }


        public Color GoodSelectColour
        {
            get
            {
                return goodSelectColour;
            }
            set
            {
                goodSelectColour = value;
            }
        }


        public Color BadSelectColour
        {
            get
            {
                return badSelectColour;
            }
            set
            {
                badSelectColour = value;
            }
        }


        public Color NeutralSelectColour
        {
            get
            {
                return neutralSelectColour;
            }
            set
            {
                neutralSelectColour = value;
            }
        }


        public float OutlineWidth
        {
            get
            {
                return outlineWidth;
            }
            set
            {
                if( outlineWidth != value )
                {
                    OutlineWidthChanged.Invoke( value );
                }
                outlineWidth = value;
            }
        }


        public NixinEvent<float> OutlineWidthChanged
        {
            get
            {
                return outlineWidthChanged;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "NoneHoverLerpTime" )]
        private float noneHoverLerpTime = 1.0f;

        [SerializeField, FormerlySerializedAs( "HoverSelectLerpTime" )]
        private float hoverSelectLerpTime = 1.0f;

        [SerializeField, FormerlySerializedAs( "SelectStayTime" )]
        private float selectStayTime      = 1.0f;

        [SerializeField, FormerlySerializedAs( "GoodHoverColour" )]
        private Color goodHoverColour       = Color.green;

        [SerializeField, FormerlySerializedAs( "BadHoverColour" )]
        private Color badHoverColour        = Color.red;

        [SerializeField, FormerlySerializedAs( "NeutralHoverColour" )]
        private Color neutralHoverColour    = Color.yellow;

        [SerializeField, FormerlySerializedAs( "GoodSelectColour" )]
        private Color goodSelectColour      = Color.blue;

        [SerializeField, FormerlySerializedAs( "BadSelectColour" )]
        private Color badSelectColour       = Color.magenta;

        [SerializeField, FormerlySerializedAs( "NeutralSelectColour" )]
        private Color neutralSelectColour   = Color.white;

        [SerializeField, FormerlySerializedAs( "OutlineWidth" )]
        private float outlineWidth          = 0.2f;

        NixinEvent<float>           outlineWidthChanged = new NixinEvent<float>();
    }
}
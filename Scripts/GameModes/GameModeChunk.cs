using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nixin
{
    [CreateAssetMenu( fileName = "NewGameModeChunk", menuName = "Asset Bundle Chunks/Game Mode Chunk", order = 1 )]
    public class GameModeChunk : NixinAssetBundleChunk
    {


        // Public:


        public void FindGameVarDelcs()
        {
            if( managerClass.Type == null )
            {
                NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Resources, "Game manager type was null." );
                return;
            }
            gameVarDelcs = GameVarDeclAttribute.GetCustomAttributes( managerClass.Type,
                            typeof( GameVarDeclAttribute ) ).Cast<GameVarDeclAttribute>().OrderBy( x => x.Ordering ).ToList();
        }



        public string NameToken
        {
            get
            {
                return nameToken;
            }
        }


        public string DescriptionToken
        {
            get
            {
                return descriptionToken;
            }
        }


        public bool RequiresMapExtension
        {
            get
            {
                return requiresMapExntesion;
            }
        }


        public bool IsMatchGameMode
        {
            get
            {
                return isMatchGameMode;
            }
        }


        public List<GameVarDeclAttribute> GameVarDelcs
        {
            get
            {
                return gameVarDelcs;
            }
        }


        public GameManagerWeakReference ManagerPrefab
        {
            get
            {
                return managerPrefab;
            }
        }


        public LocalGameManagerWeakReference LocalManagerPrefab
        {
            get
            {
                return localManagerPrefab;
            }
        }


        public GameStateWeakReference StatePrefab
        {
            get
            {
                return statePrefab;
            }
        }


        public StatsBaseWeakReferenece StatsPrefab
        {
            get
            {
                return statsPrefab;
            }
        }


        public HudBaseWeakReference HudPrefab
        {
            get
            {
                return hudPrefab;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "NameToken" )]
        private string  nameToken            = null;

        [SerializeField, FormerlySerializedAs( "DescriptionToken" )]
        private string  descriptionToken     = null;

        [SerializeField, FormerlySerializedAs( "RequiresMapeExtenions" )]
        private bool    requiresMapExntesion = false;

        [SerializeField, FormerlySerializedAs( "IsMatchGameMode" )]
        private bool    isMatchGameMode      = true;

        [SerializeField, FormerlySerializedAs( "ManagerPrefab" )]
        private GameManagerWeakReference           managerPrefab = null;

        [SerializeField, FormerlySerializedAs( "LocalManagerPrefab" )]
        private LocalGameManagerWeakReference      localManagerPrefab = null;

        [SerializeField, FormerlySerializedAs( "StatePrefab" )]
        private GameStateWeakReference              statePrefab   = null;

        [SerializeField, FormerlySerializedAs( "StatsPrefab" )]
        private StatsBaseWeakReferenece              statsPrefab   = null;

        [SerializeField, FormerlySerializedAs( "HudPrefab" )]
        private HudBaseWeakReference                hudPrefab     = null;

        [SerializeField, FormerlySerializedAs( "ManagerClass" )]
        private SubClassOfGameManager               managerClass  = null;

        private List<GameVarDeclAttribute> gameVarDelcs = null;
    }
}

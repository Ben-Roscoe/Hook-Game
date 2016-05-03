using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nixin
{
    [CreateAssetMenu( fileName = "NewExtensionsChunk", 
        menuName = "Asset Bundle Chunks/Game Mode Extension Chunk", order = 0 )]
    public class GameModeMapExtensionChunk : NixinAssetBundleChunk
    {


        // Public:


        public GameModeChunk GameModeChunk
        {
            get
            {
                return gameModeChunk;
            }
        }


        public MapChunk MapChunk
        {
            get
            {
                return mapChunk;
            }
        }


        public GameModeMapExtensionWeakReference ExtensionPrefab
        {
            get
            {
                return extensionPrefab;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "GameModeChunk" )]
        private GameModeChunk       gameModeChunk = null;

        [SerializeField, FormerlySerializedAs( "MapChunk" )]
        private MapChunk            mapChunk      = null;

        [SerializeField, FormerlySerializedAs( "ExtensionPrefab" )]
        private GameModeMapExtensionWeakReference extensionPrefab = null;
    }
}

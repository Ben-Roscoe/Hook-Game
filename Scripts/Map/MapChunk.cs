using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

#if !NSHIPPING && UNITY_EDITOR
using UnityEditor;
#endif

namespace Nixin
{
    [CreateAssetMenu( fileName = "NewMapChunk", menuName = "Asset Bundle Chunks/Map Chunk", order = 0 )]
    public class MapChunk : NixinAssetBundleChunk
    {


        // Public:


        public bool HasExtension( GameModeChunk mode )
        {
            return GetExtension( mode ) != null;
        }


        public GameModeMapExtensionChunk GetExtension( GameModeChunk mode )
        {
            if( gameModeExtensions == null )
            {
                return null;
            }
            for( int i = 0; i < gameModeExtensions.Count; ++i )
            {
                if( gameModeExtensions[i] != null && gameModeExtensions[i].GameModeChunk == mode )
                {
                    return gameModeExtensions[i];
                }
            }
            return null;
        }


        public MapType MapType
        {
            get
            {
                return mapType;
            }
        }


        public string NameToken
        {
            get
            {
                return nameToken;
            }
        }


        public Sprite Icon
        {
            get
            {
                return icon;
            }
        }


        public List<GameModeMapExtensionChunk> GameModeExtensions
        {
            get
            {
                return gameModeExtensions;
            }
            set
            {
                gameModeExtensions = value;
            }
        }


        public AssetBundleName SceneBundleName
        {
            get
            {
                return sceneBundleName;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "MapType" )]
        private MapType         mapType     = MapType.Match;

        [SerializeField, FormerlySerializedAs( "NameToken" )]
        private string          nameToken   = null;

        [SerializeField, FormerlySerializedAs( "Icon" )]
        private Sprite          icon        = null;

        [SerializeField, FormerlySerializedAs( "SceneBundleName" )]
        private AssetBundleName                   sceneBundleName = new AssetBundleName();

        private List<GameModeMapExtensionChunk>  gameModeExtensions = new List<GameModeMapExtensionChunk>();
    }


    public enum MapType
    {
        Match = 0,
        NonMatch = 1,
        Transition = 2,
    }
}

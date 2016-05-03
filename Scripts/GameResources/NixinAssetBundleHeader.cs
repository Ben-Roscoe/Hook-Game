using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nixin
{
    [CreateAssetMenu( fileName = "DataBundleHeader", menuName = "Asset Bundle Headers/Data Bundle Header", order = 0 )]
    public class NixinAssetBundleHeader : UnityEngine.ScriptableObject
    {


        // Public:


        public void StartRuntime( ResourceSystem resourceSystem, AssetBundle assetBundle )
        {
            this.resourceSystem = resourceSystem;
            this.assetBundle    = assetBundle;
            for( int i = 0; i < Entries.Count; ++i )
            {
                Entries[i].Header = this;
            }
        }


        public void EndRuntime( AssetBundle assetBundle )
        {
            if( assetBundle != null )
            {
                assetBundle.Unload( true );
                assetBundle = null;
            }
        }


        public T GetLoadedObject<T>( string name ) where T : UnityEngine.Object
        {
            for( int i = 0; i < Entries.Count; ++i )
            {
                if( Entries[i].Status != NixinAssetBundleEntryStatus.Loaded )
                {
                    continue;
                }
                if( entries[i].Name == name )
                {
                    T t = entries[i].LoadedObject as T;
                    if( t != null )
                    {
                        return t;
                    }
                }
            }
            return null;
        }


        public T GetLoadedObject<T>() where T : UnityEngine.Object
        {
            for( int i = 0; i < Entries.Count; ++i )
            {
                if( Entries[i].Status != NixinAssetBundleEntryStatus.Loaded )
                {
                    continue;
                }
                T t = entries[i].LoadedObject as T;
                if( t != null )
                {
                    return t;
                }
            }
            return null;
        }


        public T GetLoadedActor<T>( string name ) where T : UnityEngine.Object
        {
            for( int i = 0; i < Entries.Count; ++i )
            {
                if( Entries[i].Status != NixinAssetBundleEntryStatus.Loaded
                    || entries[i].Name != name )
                {
                    continue;
                }

                GameObject gameObject = Entries[i].LoadedObject as GameObject;
                if( gameObject != null )
                {
                    var actor = gameObject.GetComponent<Actor>() as T;
                    if( actor != null )
                    {
                        return actor;
                    }
                }
            }
            return null;
        }


        public T GetLoadedActor<T>() where T : UnityEngine.Object
        {
            for( int i = 0; i < Entries.Count; ++i )
            {
                if( Entries[i].Status != NixinAssetBundleEntryStatus.Loaded )
                {
                    continue;
                }

                GameObject gameObject = Entries[i].LoadedObject as GameObject;
                if( gameObject != null )
                {
                    var actor = gameObject.GetComponent<Actor>() as T;
                    if( actor != null )
                    {
                        return actor;
                    }
                }
            }
            return null;
        }


        public NixinAssetBundleEntry GetEntry( string name )
        {
            for( int i = 0; i < Entries.Count; ++i )
            {
                if( entries[i].Name == name )
                {
                    return entries[i];
                }
            }
            return null;
        }


        public List<NixinAssetBundleEntry> GetChunks()
        {
            var chunks = new List<NixinAssetBundleEntry>();
            for( int i = 0; i < Entries.Count; ++i )
            {
                if( entries[i].IsChunk )
                {
                    chunks.Add( entries[i] );
                }
            }
            return chunks;
        }


        public NixinAssetBundleEntry GetLoadedEntry( UnityEngine.Object obj )
        {
            for( int i = 0; i < Entries.Count; ++i )
            {
                if( entries[i].LoadedObject != null && entries[i].LoadedObject == obj )
                {
                    return entries[i];
                }
            }
            return null;
        }


        public AssetBundle AssetBundle
        {
            get
            {
                return assetBundle;
            }
        }


        public ResourceSystem ResourceSystem
        {
            get
            {
                return resourceSystem;
            }
        }


        public string BundleName
        {
            get
            {
                return bundleName;
            }
#if UNITY_EDITOR
            set
            {
                bundleName = value;
            }
#endif
        }


        public List<NixinAssetBundleEntry> Entries
        {
            get
            {
                return entries;
            }
#if UNITY_EDITOR
            set
            {
                entries = value;
            }
#endif
        }


        public Hash128 Hash
        {
            get
            {
                return hash;
            }
#if UNITY_EDITOR
            set
            {
                hash = value;
            }
#endif
        }


        // Private:


        [SerializeField, HideInInspector]
        private string                      bundleName    = null;

        [SerializeField, HideInInspector]
        private List<NixinAssetBundleEntry> entries = new List<NixinAssetBundleEntry>();

        [SerializeField, HideInInspector]
        private Hash128                     hash;

        private AssetBundle                 assetBundle = null;
        private ResourceSystem              resourceSystem      = null;
    }


    [System.Serializable]
    public class NixinAssetBundleEntry
    {


        public static string GetFullEntryIdentifier( string bundleName, string name )
        {
            return string.Format( "{0}:{1}", bundleName, name );
        }


        public static string GetEntryBundleFromIdentifier( string identifier )
        {
            var index = identifier.IndexOf( AssetBundleNameSeparator );
            if( index <= 0 )
            {
                NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Resources,
                                            "GetEntryBundleFromIdentifier we given as invalid identifier." );
                return null;
            }
            return identifier.Substring( 0, index );
        }


        public static string GetEntryNameFromIdentifier( string identifier )
        {
            var index = identifier.IndexOf( AssetBundleNameSeparator );
            if( index == -1 || index >= identifier.Length - 1 )
            {
                NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Resources,
                                            "GetEntryNameFromIdentifier we given as invalid identifier." );
                return null;
            }
            return identifier.Substring( index + 1 );
        }


        // Public:


        public NixinAssetBundleEntry( string name, List<string> dependencies, bool isChunk, long runtimeSize )
        {
            this.name                       = name;
            this.stringDependencies         = dependencies;
            this.isChunk                    = isChunk;
            this.runtimeSize                = runtimeSize;
        }


        public void AddRef()
        {
            ++refCount;
        }


        public void RemoveRef()
        {
            --refCount;
        }


        public string Name
        {
            get
            {
                return name;
            }
        }


        public UnityEngine.Object LoadedObject
        {
            get
            {
                return loadedObject;
            }
            set
            {
                loadedObject = value;
            }
        }


        public NixinAssetBundleEntryStatus Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
            }
        }


        public List<string> StringDependencies
        {
            get
            {
                return stringDependencies;
            }
        }


        public List<NixinAssetBundleEntry> Dependencies
        {
            get
            {
                return depedencies;
            }
            set
            {
                depedencies = value;
            }
        }


        public NixinAssetBundleHeader Header
        {
            get
            {
                return header;
            }
            set
            {
                header = value;
            }
        }


        public bool IsChunk
        {
            get
            {
                return isChunk;
            }
#if UNITY_EDITOR
            set
            {
                isChunk = value;
            }
#endif
        }


        public long RuntimeSize
        {
            get
            {
                return runtimeSize;
            }
        }


        public int RefCount
        {
            get
            {
                return refCount;
            }
        }


        // Private:


        // Dependencies are stored as BundlePath:AssetName.
        private const char AssetBundleNameSeparator = ':';


        [SerializeField, HideInInspector]
        private string          name;

        [SerializeField, HideInInspector]
        private List<string>    stringDependencies = new List<string>();

        [SerializeField, HideInInspector]
        private List<string>    stringPreloadDependencies = new List<string>();

        [SerializeField, HideInInspector]
        private bool            isChunk = false;

        [SerializeField, HideInInspector]
        private long            runtimeSize = 0;

        [NonSerialized]
        private NixinAssetBundleHeader       header         = null;

        [NonSerialized]
        private UnityEngine.Object           loadedObject   = null;

        [NonSerialized]
        private List<NixinAssetBundleEntry>  depedencies    = new List<NixinAssetBundleEntry>();

        [NonSerialized]
        private List<NixinAssetBundleEntry>  preloadDependencies = new List<NixinAssetBundleEntry>();

        private NixinAssetBundleEntryStatus  status         = NixinAssetBundleEntryStatus.Unloaded;
        private int                          refCount       = 0;
        
    }


    public enum NixinAssetBundleEntryStatus
    {
        Unloaded,
        Loaded,
        Loading,
    }
}

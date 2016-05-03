using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class ResourceSystem : NixinSystem
    {


        // Public:


        public ResourceSystem( World containingWorld, string dataFolder ) : base( containingWorld )
        {
            this.dataFolder             = dataFolder;

            LoadDataFiles( dataFolder );
            for( int i = 0; i < matchMapChunks.Count; ++i )
            {
                matchMapChunks[i].GameModeExtensions = GetExtensionChunksForMap( matchMapChunks[i] );
            }
        }


        public NixinAssetBundleEntry ResolveDepedencyString( string dependencyString )
        {
            var bundleName = NixinAssetBundleEntry.GetEntryBundleFromIdentifier( dependencyString );
            var entryName  = NixinAssetBundleEntry.GetEntryNameFromIdentifier( dependencyString );

            var header     = GetBundleHeader( bundleName );
            if( header == null )
            {
                NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Resources,
                       string.Format( "Could not resolve depedency {0}. Bundle not found.", dependencyString ) );
                return null;
            }

            var entry       = header.GetEntry( entryName );
            if( entry == null )
            {
                NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Resources,
                       string.Format( "Could not resolve depedency {0}. Entry not found.", dependencyString ) );
                return null;
            }

            return entry;
        }


        public Actor GetLoadedActorResourceWithName( string name )
        {
            foreach( var bundle in bundleHeaders )
            {
                var gameObject = bundle.GetLoadedObject<GameObject>( name );
                if( gameObject == null )
                {
                    continue;
                }

                var actor = gameObject.GetComponent<Actor>();
                if( actor == null )
                {
                    continue;
                }

                return actor;
            }
            return null;
        }


        public AssetBundle GetSceneAssetBundle( string name )
        {
            for( int i = 0; i < sceneAssetBundles.Count; ++i )
            {
                if( sceneAssetBundles[i].name == name )
                {
                    return sceneAssetBundles[i];
                }
            }
            return null;
        }


        public NixinAssetBundleHeader GetBundleHeader( string name )
        {
            name = name.ToLower();
            for( int i = 0; i < bundleHeaders.Count; ++i )
            {
                if( bundleHeaders[i].BundleName == name )
                {
                    return bundleHeaders[i];
                }
            }
            return null;
        }


        public NixinAssetBundleChunk GetBundleChunk( string name )
        {
            name = name.ToLower();
            return allChunks.Find( x => x.name == name );
        }


        public MapChunk GetMapChunk( string name )
        {
            var found = GetMatchMapChunk( name );
            if( found != null )
            {
                return found;
            }

            found = GetNonMatchMapChunk( name );
            if( found != null )
            {
                return found;
            }

            found = GetTransitionMapChunk( name );
            if( found != null )
            {
                return found;
            }
            return null;
        }


        public MapChunk GetMatchMapChunk( string name )
        {
            for( int i = 0; i < matchMapChunks.Count; ++i )
            {
                if( matchMapChunks[i].name == name )
                {
                    return matchMapChunks[i];
                }
            }
            return null;
        }


        public MapChunk GetNonMatchMapChunk( string name )
        {
            for( int i = 0; i < nonMatchMapChunks.Count; ++i )
            {
                if( nonMatchMapChunks[i].name == name )
                {
                    return nonMatchMapChunks[i];
                }
            }
            return null;
        }


        public MapChunk GetTransitionMapChunk( string name )
        {
            for( int i = 0; i < transitionMapChunks.Count; ++i )
            {
                if( transitionMapChunks[i].name == name )
                {
                    return transitionMapChunks[i];
                }
            }
            return null;
        }


        public GameModeChunk GetGameModeChunk( string name )
        {
            var found = GetMatchGameModeChunk( name );
            if( found == null )
            {
                found = GetNonMatchGameModeChunk( name );
            }
            return found;
        }


        public GameModeChunk GetMatchGameModeChunk( string name )
        {
            for( int i = 0; i < matchGameModeChunks.Count; ++i )
            {
                if( matchGameModeChunks[i].name == name )
                {
                    return matchGameModeChunks[i];
                }
            }
            return null;
        }


        public GameModeChunk GetNonMatchGameModeChunk( string name )
        {
            for( int i = 0; i < nonMatchGameModeChunks.Count; ++i )
            {
                if( nonMatchGameModeChunks[i].name == name )
                {
                    return nonMatchGameModeChunks[i];
                }
            }
            return null;
        }


        public List<MapChunk> MatchMaps
        {
            get
            {
                return matchMapChunks;
            }
        }


        public List<MapChunk> NonMatchMaps
        {
            get
            {
                return nonMatchMapChunks;
            }
        }


        public List<MapChunk> TransitionMaps
        {
            get
            {
                return transitionMapChunks;
            }
        }


        public List<GameModeChunk> MatchGameModes
        {
            get
            {
                return matchGameModeChunks;
            }
        }


        public List<GameModeChunk> NonMatchGameModes
        {
            get
            {
                return nonMatchGameModeChunks;
            }
        }


        // Private:


        public const string NixinAssetBundleHeaderPostfix = "_Header";


        private List<MapChunk>                      matchMapChunks           = new List<MapChunk>();
        private List<MapChunk>                      nonMatchMapChunks        = new List<MapChunk>();
        private List<MapChunk>                      transitionMapChunks      = new List<MapChunk>();
        private List<GameModeChunk>                 matchGameModeChunks      = new List<GameModeChunk>();
        private List<GameModeChunk>                 nonMatchGameModeChunks   = new List<GameModeChunk>();
        private List<GameModeMapExtensionChunk>     extensionChunks          = new List<GameModeMapExtensionChunk>();
        private List<NixinAssetBundleChunk>         allChunks                = new List<NixinAssetBundleChunk>();

        private List<AssetBundle>                   sceneAssetBundles        = new List<AssetBundle>();

        private List<NixinAssetBundleHeader>        bundleHeaders            = new List<NixinAssetBundleHeader>();

        private string                              dataFolder      = "";


        private void LoadDataFiles( string rootfolder )
        {
            var files = Directory.GetFiles( rootfolder, "*.", SearchOption.AllDirectories );
            for( int i = 0; i < files.Length; ++i )
            {
                // Load asset bundle info.
                AssetBundle  assetBundle = AssetBundle.LoadFromFile( files[i] );
                if( assetBundle == null )
                {
                    NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Resources,
                        string.Format( "Failed to load asset bundle at path: {0}", files[i] ) );
                    continue;
                }

                assetBundle.name = Path.GetFileNameWithoutExtension( files[i] );

                // Load header.
                var          header      = assetBundle.LoadAsset<NixinAssetBundleHeader>( assetBundle.name
                    + NixinAssetBundleHeaderPostfix );
                if( header == null )
                {
                    // No header, this must be a scene asset bundle.
                    sceneAssetBundles.Add( assetBundle );
                    continue;
                }
                header.StartRuntime( this, assetBundle );
                bundleHeaders.Add( header );
            }

            ResolveAllEntryDependencyStrings();
            for( int i = 0; i < bundleHeaders.Count; ++i )
            {
                var chunks = bundleHeaders[i].GetChunks();
                if( chunks.Count <= 0 )
                {
                    // No chunks to load.
                    continue;
                }

                // We want to load the chunk entries without their dependencies. We'll do this synchronously.
                ResourceBatchLoader loader = new ResourceBatchLoader( this, false, chunks.ToArray() );
                loader.BeginLoad( false );

                for( int c = 0; c < chunks.Count; ++c )
                {
                    var gameModeChunk = chunks[c].LoadedObject as GameModeChunk;
                    if( gameModeChunk != null )
                    {
                        if( gameModeChunk.IsMatchGameMode )
                        {
                            matchGameModeChunks.Add( gameModeChunk );
                        }
                        else
                        {
                            nonMatchGameModeChunks.Add( gameModeChunk );
                        }
                        gameModeChunk.FindGameVarDelcs();
                        gameModeChunk.Header = bundleHeaders[i];
                        gameModeChunk.AssetName = gameModeChunk.name;
                        allChunks.Add( gameModeChunk );
                        continue;
                    }

                    var mapChunk = chunks[c].LoadedObject as MapChunk;
                    if( mapChunk != null )
                    {
                        if( mapChunk.MapType == MapType.Match )
                        {
                            matchMapChunks.Add( mapChunk );
                        }
                        else if( mapChunk.MapType == MapType.NonMatch )
                        {
                            nonMatchMapChunks.Add( mapChunk );
                        }
                        else
                        {
                            transitionMapChunks.Add( mapChunk );
                        }
                        mapChunk.Header = bundleHeaders[i];
                        mapChunk.AssetName = mapChunk.name;
                        allChunks.Add( mapChunk );
                        continue;
                    }

                    var extensionChunk = chunks[c].LoadedObject as GameModeMapExtensionChunk;
                    if( extensionChunk != null )
                    {
                        extensionChunks.Add( extensionChunk );
                        extensionChunk.AssetName = extensionChunk.name;
                        allChunks.Add( extensionChunk );
                        continue;
                    }

                    NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Resources,
                        string.Format( "Unrecognised chunk found in bundle {0}.", bundleHeaders[i].BundleName ) );
                }
            }
        }


        private void ResolveAllEntryDependencyStrings()
        {
            for( int i = 0; i < bundleHeaders.Count; ++i )
            {
                for( int e = 0; e < bundleHeaders[i].Entries.Count; ++e )
                {
                    ResolveEntryDependencyStrings( bundleHeaders[i].Entries[e] );
                }
            }
        }


        private void ResolveEntryDependencyStrings( NixinAssetBundleEntry entry )
        {
            var dependencies = new List<NixinAssetBundleEntry>( entry.StringDependencies.Count );
            for( int i = 0; i < entry.StringDependencies.Count; ++i )
            {
                dependencies.Add( ResolveDepedencyString( entry.StringDependencies[i] ) );
            }
            entry.Dependencies = dependencies;
        }


        private List<GameModeMapExtensionChunk> GetExtensionChunksForMap( MapChunk mapChunk )
        {
            var ret = new List<GameModeMapExtensionChunk>();
            for( int i = 0; i < extensionChunks.Count; ++i )
            {
                if( extensionChunks[i].MapChunk == mapChunk )
                {
                    ret.Add( extensionChunks[i] );
                }
            }
            return ret;
        }
    }
}

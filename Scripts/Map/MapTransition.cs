using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nixin
{
    public delegate void MapTransitionProgressCallback( MapTransition transition, float progress );
      
    public enum MapTransitionStatus
    {
        NotStarted,
        LoadingNextBundles,
        LoadingNextScene,
        ClearingLastScene,
        UnloadingLastBundles,
        ActivatingNextScene,
        Failed,
        Complete,
    }


    public class MapTransition
    {


        // Public:


        public MapTransition( GameMap lastGameMap, GameMap nextGameMap, GameMapBatchLoader lastBatch, 
            GameMapBatchLoader nextBatch )
        {
            Assert.IsTrue( nextBatch != null );

            this.lastGameMap            = lastGameMap;
            this.nextGameMap            = nextGameMap;

            this.lastBatch      = lastBatch;
            this.nextBatch      = nextBatch;

            status              = MapTransitionStatus.NotStarted;
        }


        public void MakeTransition()
        {
            Assert.IsTrue( nextGameMap != null );

            LoadNext();
        }


        public void AddProgressCallback( MapTransitionProgressCallback callback )
        {
            if( !callbacks.Contains( callback ) )
            {
                callbacks.Add( callback );
            }
        }


        public void RemoveProgressCallback( MapTransitionProgressCallback callback )
        {
            callbacks.Remove( callback );
        }


        public GameMap CurrentGameMap
        {
            get
            {
                return lastGameMap;
            }
        }


        public GameMap NextGameMap
        {
            get
            {
                return nextGameMap;
            }
        }


        public GameMapBatchLoader LastBatch
        {
            get
            {
                return lastBatch;
            }
        }


        public GameMapBatchLoader NextBatch
        {
            get
            {
                return nextBatch;
            }
        }


        public MapTransitionStatus Status
        {
            get
            {
                return status;
            }
        }


        // Private:


        // Can't tell how long the scene load will take compared to bundle loading,
        // so just estimate.
        private const float                         bundleLoadWeighting = 0.99f;

        private const float                         sceneDoneProgress   = 0.9f;

        private List<MapTransitionProgressCallback>         callbacks   = new List<MapTransitionProgressCallback>();

        private GameMap                     lastGameMap                 = null;
        private GameMap                     nextGameMap                 = null;

        private GameMapBatchLoader          lastBatch                   = null;
        private GameMapBatchLoader          nextBatch                   = null;

        private MapTransitionStatus         status                      = MapTransitionStatus.NotStarted;

        private AsyncOperation              sceneLoadOperation          = null;


        private void LoadNext()
        {
            status = MapTransitionStatus.LoadingNextBundles;

            nextBatch.AddProgressCallback( NextLoaderProgressCallback );
            nextBatch.BeginLoad();
        }


        private void UnloadLast()
        {
            status = MapTransitionStatus.UnloadingLastBundles;
            ReportProgress( 0.0f );

            if( lastGameMap != null )
            {
                lastBatch.Unload();
            }
        }


        private void NextLoaderProgressCallback( ResourceBatchLoader loader, float progress )
        {
            if( loader.Status == ResourceBatchLoaderStatus.Failed )
            {
                status = MapTransitionStatus.Failed;
                ReportProgress( 0.0f );

                return;
            }
            else if( loader.Status == ResourceBatchLoaderStatus.Finished )
            {
                status              = MapTransitionStatus.LoadingNextScene;
                NextBatch.ResourceSystem.ContainingWorld.StartCoroutine( LoadNextScene() );
            }
            ReportProgress( progress * bundleLoadWeighting );
        }


        private IEnumerator LoadNextScene()
        {
            // Wait one frame for the current update to complete. Updates are not allowed during scene load.
            yield return null;
            sceneLoadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync( 
                Path.GetFileNameWithoutExtension( nextBatch.SceneAssetBundle.GetAllScenePaths()[0] ) );

            sceneLoadOperation.allowSceneActivation = false;
            while( !sceneLoadOperation.isDone )
            {
                float progress = bundleLoadWeighting + ( sceneLoadOperation.progress * ( 1.0f - bundleLoadWeighting ) );

                if( !sceneLoadOperation.allowSceneActivation && sceneLoadOperation.progress >= sceneDoneProgress )
                {

                    // Report that we're unloading old data.
                    UnloadLast();
                    ReportProgress( progress );

                    // Report that the last scene needs to be cleared.
                    status = MapTransitionStatus.ClearingLastScene;
                    ReportProgress( 1.0f );

                    // Report that we're now activating the next scene.
                    sceneLoadOperation.allowSceneActivation = true;
                    status = MapTransitionStatus.ActivatingNextScene;
                    yield return null;
                    continue;
                }
                ReportProgress( progress );
                yield return null;
            }
            
            status = MapTransitionStatus.Complete;
            ReportProgress( 1.0f );
        }


        private void ReportProgress( float progress )
        {
            callbacks.ForEach( x => x.Invoke( this, progress ) );

            if( status == MapTransitionStatus.Complete || status == MapTransitionStatus.Failed )
            {
                callbacks.Clear();
            }
        }
    }


    public class MapTransitionException : Exception
    {
        public MapTransitionException() : base()
        {
        }


        public MapTransitionException( string msg ) : base( msg )
        {
        }


        public MapTransitionException( string msg, Exception inner ) : base( msg, inner )
        {
        }
    }
}

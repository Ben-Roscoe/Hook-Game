using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Nixin
{
    public delegate void ResourceBatchLoaderProgressCallback( ResourceBatchLoader loader, float progress );

    public enum ResourceBatchLoaderStatus
    {
        Unloaded,
        Loading,
        Finished,
        Failed,
    }

    public class ResourceBatchLoader
    {


        // Public:


        public ResourceBatchLoader( ResourceSystem resourceSystem, bool loadDependencies, params NixinAssetBundleEntry[] inJobs )
        {
            this.resourceSystem     = resourceSystem;
            this.loadDependencies   = loadDependencies;

            givenJobs           = new List<NixinAssetBundleEntry>();
            if( inJobs.Length <= 0 )
            {
                NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Resources, "No jobs given to batch loader." );
            }

            // Add only valid jobs.
            jobs       = new HashSet<NixinAssetBundleEntry>();
            AddJobs( inJobs );
        }


        public void BeginLoad( bool isAsync = true )
        {
            this.isAsync = isAsync;

            // Nothing to load.
            if( jobs == null || completedJobs >= jobs.Count )
            {
                Complete();
                return;
            }

            status = ResourceBatchLoaderStatus.Loading;

            // Add a ref foreach bundle we need. This will prevent them from being unloaded.
            foreach( var job in jobs )
            {
                job.AddRef();
            }
            resourceSystem.ContainingWorld.StartCoroutine( DoJobs() );
        }


        public void Unload()
        {
            status = ResourceBatchLoaderStatus.Unloaded;

            // Allow the jobs to be unloaded.
            foreach( var job in jobs )
            {
                job.RemoveRef();
                if( job.RefCount == 0 )
                {
                    job.Status       = NixinAssetBundleEntryStatus.Unloaded;
                    job.LoadedObject = null;
                }
            }
            completedJobs = 0;
            Resources.UnloadUnusedAssets();
        }


        public void AddProgressCallback( ResourceBatchLoaderProgressCallback callback )
        {
            if( status == ResourceBatchLoaderStatus.Failed || status == ResourceBatchLoaderStatus.Loading )
            {
                return;
            }

            if( !callbacks.Contains( callback ) )
            {
                callbacks.Add( callback );
            }
        }


        public void RemoveProgressCallback( ResourceBatchLoaderProgressCallback callback )
        {
            callbacks.Remove( callback );
        }


        public ResourceBatchLoaderStatus Status
        {
            get
            {
                return status;
            }
        }


        public ResourceSystem ResourceSystem
        {
            get
            {
                return resourceSystem;
            }
        }


        // Private:

            
        private HashSet<NixinAssetBundleEntry>     jobs                        = null;
        private List<NixinAssetBundleEntry>     givenJobs                   = null;

        private NixinAssetBundleEntry           currentJob                  = null;
        private float                           currentJobProgress          = 0.0f;

        private ResourceBatchLoaderStatus       status                      = ResourceBatchLoaderStatus.Unloaded;

        private List<ResourceBatchLoaderProgressCallback>                   callbacks = new List<ResourceBatchLoaderProgressCallback>();

        private ResourceSystem                  resourceSystem              = null;

        private int                             completedJobs               = 0;
        private bool                            isAsync                     = true;
        private bool                            loadDependencies            = true;


        private IEnumerator DoJobs()
        {
            foreach( var job in jobs )
            {
                if( job.Status == NixinAssetBundleEntryStatus.Loaded
                    || job.Status == NixinAssetBundleEntryStatus.Loading )
                {
                    ++completedJobs;
                    continue;
                }

                currentJob = job;
                currentJob.Status = NixinAssetBundleEntryStatus.Loading;
                currentJobProgress = 0.0f;

                if( isAsync )
                {
                    var request = currentJob.Header.AssetBundle.LoadAssetAsync( currentJob.Name );
                    while( !request.isDone )
                    {
                        currentJobProgress = request.progress;
                        ReportProgress();
                        yield return null;
                    }
                    yield return request;

                    job.LoadedObject = request.asset;
                }
                else
                {
                    job.LoadedObject = currentJob.Header.AssetBundle.LoadAsset( currentJob.Name );
                }

                currentJob.Status = NixinAssetBundleEntryStatus.Loaded;
                currentJobProgress = 1.0f;
                ++completedJobs;
            }
            Complete();
        }


        private void Complete()
        {
            status = ResourceBatchLoaderStatus.Finished;
            ReportProgress();
        }


        private void Fail()
        {
            status = ResourceBatchLoaderStatus.Failed;
            ReportProgress();
        }


        private void ReportProgress()
        {
            float progress = CalculateCurrentProgress();
            callbacks.ForEach( x => x.Invoke( this, progress ) );

            if( status == ResourceBatchLoaderStatus.Finished || status == ResourceBatchLoaderStatus.Failed )
            {
                callbacks.Clear();
            }
        }


        private float CalculateCurrentProgress()
        {
            if( status == ResourceBatchLoaderStatus.Unloaded || status == ResourceBatchLoaderStatus.Failed )
            {
                return 0.0f;
            }
            if( status == ResourceBatchLoaderStatus.Finished )
            {
                return 1.0f;
            }

            long loaded = 0;
            long total  = 0;
            foreach( var job in jobs )
            {
                if( job.Status == NixinAssetBundleEntryStatus.Loading )
                {
                    loaded += ( long )( job.RuntimeSize * currentJobProgress );
                }
                else if( job.Status == NixinAssetBundleEntryStatus.Loaded )
                {
                    loaded += job.RuntimeSize;
                }
                total += job.RuntimeSize;
            }

            return ( float )loaded / ( float )total;
        }


        private void AddJobs( NixinAssetBundleEntry[] inJobs )
        {
            foreach( var job in inJobs )
            {
                if( job != null )
                {
                    // Keep track of the jobs that were given to us, since the jobs list also contains dependencies.
                    givenJobs.Add( job );
                    if( loadDependencies && job.Dependencies != null )
                    {
                        for( int i = 0; i < job.Dependencies.Count; ++i )
                        {
                            jobs.Add( job.Dependencies[i] );
                        }
                    }
                    else
                    {
                        jobs.Add( job );
                    }
                }
            }
        }
    }
}

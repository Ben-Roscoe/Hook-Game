using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    
    public abstract class NixinAssetBundleChunk : UnityEngine.ScriptableObject
    {


        // Public:


        public List<NixinAssetBundleEntry> GetChunkEntries()
        {
            var thisEntry = GetThisEntry();
            return thisEntry == null ? null : thisEntry.Dependencies;
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


        public string AssetName
        {
            get
            {
                return assetName;
            }
            set
            {
                assetName = value;
            }
        }


        // Protected:


        protected NixinAssetBundleEntry GetThisEntry()
        {
            var thisEntry = Header.GetLoadedEntry( this );
            if( thisEntry == null )
            {
                NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Resources,
                    "Could not get this chunk entry in GetChunkEntries because it has not been loaded." );
                return null;
            }
            return thisEntry;
        }


        // Private:


        private NixinAssetBundleHeader          header              = null;

        // So we can get the name from a different thread.
        private string                          assetName           = null;
    }
}

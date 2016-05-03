using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class WeakUnityReferenceSerializable
    {


        // Protected:


        [SerializeField, HideInInspector]
        protected string      guid            = "";

        [SerializeField, HideInInspector]
        protected string      assetName       = "";

        [SerializeField, HideInInspector]
        protected string      bundleName      = "";

        [SerializeField, HideInInspector]
        protected string      requiredTypeFullAssemblyName = "";
    }


    [System.Serializable]
    public class WeakUnityReference<T> : WeakUnityReferenceSerializable where T : UnityEngine.Object
    {


        // Public:


        public WeakUnityReference()
        {
            requiredTypeFullAssemblyName = typeof( T ).AssemblyQualifiedName;
        }


        public T GetRuntimeObject( ResourceSystem resourceSystem )
        {
            var entry  = GetEntry( resourceSystem );

            if( entry == null )
            {
                return null;
            }

            // Unity object?
            var t      = entry.LoadedObject as T;
            if( t != null )
            {
                return t;
            }

            var gameObject = entry.LoadedObject as GameObject;
            if( gameObject == null )
            {
                return null;
            }

            var actor      = gameObject.GetComponent<T>();
            return actor;
        }


        public NixinAssetBundleEntry GetEntry( ResourceSystem resourceSystem )
        {
            if( cachedEntry == null )
            {
                var bundle  = resourceSystem.GetBundleHeader( bundleName );
                if( bundle != null )
                {
                    cachedEntry = bundle.GetEntry( assetName );
                }
            }
            return cachedEntry;
        }


        public bool IsNull
        {
            get
            {
                return string.IsNullOrEmpty( guid );
            }
        }


        // Private:


        private NixinAssetBundleEntry cachedEntry = null;
    }
}

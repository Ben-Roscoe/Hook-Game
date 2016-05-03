using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    [System.Serializable]
    public class SubClassOfSerializable
    {

        // Protected:


        protected Type        selectedType     = null;

        [SerializeField, HideInInspector]
        protected string      selectedFullAssemblyType = "";

        [SerializeField, HideInInspector]
        protected string      parentFullAssemblyType = "";
    }


    [System.Serializable]
    public class SubClassOf<T> : SubClassOfSerializable, ISerializationCallbackReceiver
    {


        // Public:


        public void OnBeforeSerialize()
        {
            if( selectedType == null )
            {
                selectedFullAssemblyType = "";
            }
            else
            {
                selectedFullAssemblyType = selectedType.AssemblyQualifiedName;
            }
            parentFullAssemblyType   = typeof( T ).AssemblyQualifiedName;
        }


        public void OnAfterDeserialize()
        {
            if( string.IsNullOrEmpty( selectedFullAssemblyType ) )
            {
                return;
            }
            selectedType = Type.GetType( selectedFullAssemblyType );
        }


        public Type Type
        {
            get
            {
                if( !string.IsNullOrEmpty( selectedFullAssemblyType ) && selectedType == null )
                {
                    selectedType = Type.GetType( selectedFullAssemblyType );
                }
                return selectedType;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nixin
{
    [System.Serializable]
    public class AssetBundleName
    {


        // Public:


        public string Name
        {
            get
            {
                return name;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "Name" )]
        private string name;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class QuerySystem : NixinSystem
    {


        // Public:


        public QuerySystem( World containingWorld ) : base( containingWorld )
        {
        }


        public T[] GetActorsWithColliderInSphere<T>( Vector3 origin, float radius ) where T : Actor
        {
            var found   = new HashSet<T>();
            var results = Physics.OverlapSphere( origin, radius );
            for( int i = 0; i < results.Length; ++i )
            {
                var actor = results[i].gameObject.GetComponent<T>();
                if( actor == null )
                {
                    continue;
                }
                found.Add( actor );
            }
            
            return found.ToArray<T>();
        }
    }
}

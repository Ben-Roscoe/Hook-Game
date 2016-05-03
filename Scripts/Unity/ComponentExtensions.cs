using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public static class ComponentExtensions
    {


        // Public:


        public static T GetComponentInAllChildren<T>( this Component component ) where T : Component
        {
            // Get the first component found in a child, event an inactive one...
            for( int i = 0; i < component.gameObject.transform.childCount; ++i )
            {
                var child = component.gameObject.transform.GetChild( i );
                if( child == null )
                {
                    continue;
                }

                var found = child.GetComponent<T>();
                if( found != null )
                {
                    return found;
                }
            }
            return null;
        }


        public static void ForeachComponentsInParentAndChildrenRecursive<T>( this Component component, Action<T> action ) 
            where T : Component
        {
            var parent = component.GetComponent<T>();
            if( parent != null )
            {
                action.Invoke( parent );
            }
            component.ForeachComponentsInChildrenRecursive<T>( action );
        }


        public static void ForeachComponentsInChildrenRecursive<T>( this Component component, Action<T> action )
            where T : Component
        {
            int count = component.gameObject.transform.childCount;
            for( int i = 0; i < count; ++i )
            {
                var child = component.gameObject.transform.GetChild( i );
                var x = child.GetComponent<T>();
                if( x != null )
                {
                    action.Invoke( x );
                }

                child.ForeachComponentsInParentAndChildrenRecursive( action );
            }
        }


        public static void GetComponentsInParentAndChildrenRecursive<T>( this Component component, List<T> found ) where T : Component
        {
            var parent = component.GetComponent<T>();
            if( parent != null )
            {
                found.Add( parent );
            }

            for( int i = 0; i < component.gameObject.transform.childCount; ++i )
            {
                var child = component.gameObject.transform.GetChild( i );
                if( child == null )
                {
                    continue;
                }

                var x = child.GetComponent<T>();
                if( x != null )
                {
                    found.Add( x );
                }

                child.GetComponentsInParentAndChildrenRecursive( found );
            }
        }
    }
}

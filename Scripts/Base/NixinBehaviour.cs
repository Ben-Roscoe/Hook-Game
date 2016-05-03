using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Lidgren.Network;
using System.Reflection;

namespace Nixin
{
    public abstract class NixinBehaviour : MonoBehaviour
    {


        // Public:


        public const UInt32      worldId         = 0;
        public const UInt32      localId         = 0;


        public static Component ConstructDefaultComponent( Actor owner, string name, Component componentReference,
            Type type )
        {
            var found = owner.createdComponents.Find( ( x ) => { return x == componentReference && 
                x.GetType() == type; } );
            if( found != null )
            {
                if( found is NixinComponent )
                {
                    ( ( NixinComponent )( object )found ).EditorComponentConstructor( owner, name );
                }
                owner.currentCreatedComponents.Add( found );
                return found;
            }

            Component instanceComponent         = owner.gameObject.AddComponent( type );
            owner.currentCreatedComponents.Add( instanceComponent );
            owner.createdComponents.Add( instanceComponent );

            if( instanceComponent is NixinComponent )
            {
                ( ( NixinComponent )( object )instanceComponent ).EditorComponentConstructor( owner, name );
            }
            return instanceComponent;
        }


        public static T ConstructDefaultComponent<T>( Actor owner, string name, Component componentReference ) where T : Component
        {
            var found = owner.createdComponents.Find( ( x ) => { return x == componentReference && x is T; } );
            if( found != null )
            {
                if( found is NixinComponent )
                {
                    ( ( NixinComponent )( object )found ).EditorComponentConstructor( owner, name );
                }
                owner.currentCreatedComponents.Add( found );
                return ( T )found;
            }

            T instanceComponent         = owner.gameObject.AddComponent<T>();
            owner.currentCreatedComponents.Add( instanceComponent );
            owner.createdComponents.Add( instanceComponent );

            if( instanceComponent is NixinComponent )
            {
                ( ( NixinComponent )( object )instanceComponent ).EditorComponentConstructor( owner, name );
            }
            return instanceComponent;
        }


        public static T OverrideDefaultComponent<T>( Actor owner, string name, Component componentReference, Component componentToOverride ) where T : Component
        {
            var found = owner.createdComponents.Find( ( x ) => { return x == componentReference && x is T; } );
            if( found != null )
            {
                if( found is NixinComponent )
                {
                    ( ( NixinComponent )( object )found ).EditorComponentConstructor( owner, name );
                }
                owner.currentCreatedComponents.Add( found );
                return ( T )found;
            }

            // Trying to override with the same type.
            if( componentToOverride.GetType() == typeof( T ) )
            {
                return ( T )componentToOverride;
            }

            if( componentToOverride != null )
            {
                owner.createdComponents.RemoveAll( ( x ) => { return x == componentToOverride; } );
                DestroyImmediate( componentToOverride );   
            }
            return ConstructDefaultComponent<T>( owner, name, componentReference );
        }


        public static T ConstructDefaultStaticComponent<T>( Actor owner, string nameIfConstructing, Component componentReference ) where T : Component
        {
            var found = owner.createdComponents.Find( ( x ) => { return x == componentReference && x is T; } );
            if( found != null )
            {
                if( owner.currentCreatedComponents.Find( ( x ) => { return x == componentReference && x is T; } ) == null )
                {
                    if( found is NixinComponent )
                    {
                        ( ( NixinComponent )( object )found ).EditorComponentConstructor( owner, nameIfConstructing );
                    }
                    owner.currentCreatedComponents.Add( found );
                }
                return ( T )found;
            }

            T newComponent = owner.GetComponent<T>();
            if( newComponent != null )
            {
                return newComponent;
            }
            return ConstructDefaultComponent<T>( owner, nameIfConstructing, componentReference );
        }


        public static T DontConstruct<T>( Actor owner, T component ) where T : Component
        {
            if( component != null )
            {
                owner.createdComponents.RemoveAll( ( x ) => { return x == component; } );
                DestroyImmediate( component );
            }
            return null;
        }


        public virtual void EditorConstruct()
        {
        }


        public RPCMethod FindRPCMethodById( byte id )
        {
            if( rpcMethods.Count <= id  || id < 0 )
            {
                return null;
            }
            return rpcMethods[id];
        }


        public RPCMethod FindRPCMethodByMethodInfo( MethodInfo info )
        {
            return rpcMethods.Find( x => x.MethodInfo == info );
        }


        public void RegisterRPC( RPCMethodDelegate method )
        {
            AddRPCMethod( method.Method, method.Target );
        }


        public void RegisterRPC<A>( RPCMethodDelegate<A> method )
        {
            AddRPCMethod( method.Method, method.Target );
        }


        public void RegisterRPC<A, B>( RPCMethodDelegate<A, B> method )
        {
            AddRPCMethod( method.Method, method.Target );
        }


        public void RegisterRPC<A, B, C>( RPCMethodDelegate<A, B, C> method )
        {
            AddRPCMethod( method.Method, method.Target );
        }


        public void RegisterRPC<A, B, C, D>( RPCMethodDelegate<A, B, C, D> method )
        {
            AddRPCMethod( method.Method, method.Target );
        }


        public void RegisterRPC<A, B, C, D, E>( RPCMethodDelegate<A, B, C, D, E> method )
        {
            AddRPCMethod( method.Method, method.Target );
        }


        public void RegisterRPC<A, B, C, D, E, F>( RPCMethodDelegate<A, B, C, D, E, F> method )
        {
            AddRPCMethod( method.Method, method.Target );
        }


        // Protected:


        //protected virtual void BaseOnDestroy()
        //{
        //    foreach( var nixinEvent in ownedEvents )
        //    {
        //        nixinEvent.RemoveAll();
        //    }
        //    ownedEvents.Clear();

        //    foreach( var nixinEvent in subscribedEvents )
        //    {
        //        if( nixinEvent.Key == null )
        //        {
        //            continue;
        //        }
        //        nixinEvent.Key.RemoveHandlerReference( nixinEvent.Value );
        //    }
        //    subscribedEvents.Clear();
        //}


        // Private:

        // List should be unsorted and immutable after the NixinBehaviour's Initialise method.
        private List<RPCMethod>                             rpcMethods              = new List<RPCMethod>();


        private void AddRPCMethod( MethodInfo info, object owner )
        {
            rpcMethods.Add( new RPCMethod( info, owner, ( byte )rpcMethods.Count ) );
        }
    }
}
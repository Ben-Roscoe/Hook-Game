using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace Nixin
{
    public interface INixinEvent
    {
        void RemoveHandlerReference( object handlerReference );
        void RemoveAll();
    }


    public class NixinEvent : INixinEvent
    {

        // Public:


        public void Invoke()
        {
            var copyHandlers = new List<UnityAction>( handlers );
            foreach( var handler in copyHandlers )
            {
                handler.Invoke();
            }
        }


        public void AddHandler( UnityAction handler )
        {
            handlers.Add( handler );
        }


        public void RemoveHandler( UnityAction handler )
        {
            handlers.Remove( handler );
        }


        public void RemoveHandlerReference( object handlerReference )
        {
            var handler = handlerReference as UnityAction;
            if( handler != null )
            {
                handlers.Remove( handler );
            }
        }


        public void RemoveAll()
        {
            handlers.Clear();
        }


        // Private:


        private List<UnityAction> handlers = new List<UnityAction>();
    }


    public class NixinEvent<A> : INixinEvent
    {

        // Public:


        public void Invoke( A a )
        {
            var copyHandlers = new List<UnityAction<A>>( handlers );
            foreach( var handler in copyHandlers )
            {
                handler.Invoke( a );
            }
        }


        public void AddHandler( UnityAction<A> handler )
        {
            handlers.Add( handler );
        }


        public void RemoveHandler( UnityAction<A> handler )
        {
            handlers.Remove( handler );
        }


        public void RemoveHandlerReference( object handlerReference )
        {
            var handler = handlerReference as UnityAction<A>;
            if( handler != null )
            {
                handlers.Remove( handler );
            }
        }


        public void RemoveAll()
        {
            handlers.Clear();
        }


        // Private:


        private List<UnityAction<A>> handlers = new List<UnityAction<A>>();
    }


    public class NixinEvent<A, B> : INixinEvent
    {

        // Public:


        public void Invoke( A a, B b )
        {
            var copyHandlers = new List<UnityAction<A, B>>( handlers );
            foreach( var handler in copyHandlers )
            {
                handler.Invoke( a, b );
            }
        }


        public void AddHandler( UnityAction<A, B> handler )
        {
            handlers.Add( handler );
        }


        public void RemoveHandler( UnityAction<A, B> handler )
        {
            handlers.Remove( handler );
        }


        public void RemoveHandlerReference( object handlerReference )
        {
            var handler = handlerReference as UnityAction<A, B>;
            if( handler != null )
            {
                handlers.Remove( handler );
            }
        }


        public void RemoveAll()
        {
            handlers.Clear();
        }


        // Private:


        private List<UnityAction<A, B>>     handlers    = new List<UnityAction<A, B>>();
    }


    public class NixinEvent<A, B, C> : INixinEvent
    {

        // Public:


        public void Invoke( A a, B b, C c )
        {
            var copyHandlers = new List<UnityAction<A, B, C>>( handlers );
            foreach( var handler in copyHandlers )
            {
                handler.Invoke( a, b, c );
            }
        }


        public void AddHandler( UnityAction<A, B, C> handler )
        {
            handlers.Add( handler );
        }


        public void RemoveHandler( UnityAction<A, B, C> handler )
        {
            handlers.Remove( handler );
        }


        public void RemoveHandlerReference( object handlerReference )
        {
            var handler = handlerReference as UnityAction<A, B, C>;
            if( handler != null )
            {
                handlers.Remove( handler );
            }
        }


        public void RemoveAll()
        {
            handlers.Clear();
        }


        // Private:


        private List<UnityAction<A, B, C>>     handlers    = new List<UnityAction<A, B, C>>();
    }


    public class NixinEvent<A, B, C, D> : INixinEvent
    {

        // Public:


        public void Invoke( A a, B b, C c, D d )
        {
            var copyHandlers = new List<UnityAction<A, B, C, D>>( handlers );
            foreach( var handler in copyHandlers )
            {
                handler.Invoke( a, b, c, d );
            }
        }


        public void AddHandler( UnityAction<A, B, C, D> handler )
        {
            handlers.Add( handler );
        }


        public void RemoveHandler( UnityAction<A, B, C, D> handler )
        {
            handlers.Remove( handler );
        }


        public void RemoveHandlerReference( object handlerReference )
        {
            var handler = handlerReference as UnityAction<A, B, C, D>;
            if( handler != null )
            {
                handlers.Remove( handler );
            }
        }


        public void RemoveAll()
        {
            handlers.Clear();
        }


        // Private:


        private List<UnityAction<A, B, C, D>>     handlers    = new List<UnityAction<A, B, C, D>>();
    }
}

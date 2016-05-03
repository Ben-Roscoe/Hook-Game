using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nixin
{
    public delegate void Action<A, B, C, D, E>( A a, B b, C c, D d, E e );
    public class ConsoleCommand
    {

        public static ConsoleCommand Create( string name, Action method, object owner )
        {
            return new ConsoleCommand( name, method.Method, owner );
        }


        public static ConsoleCommand Create<A>( string name, Action<A> method, object owner )
        {
            return new ConsoleCommand( name, method.Method, owner );
        }


        public static ConsoleCommand Create<A, B>( string name, Action<A, B> method, object owner )
        {
            return new ConsoleCommand( name, method.Method, owner );
        }


        public static ConsoleCommand Create<A, B, C>( string name, Action<A, B, C> method, object owner )
        {
            return new ConsoleCommand( name, method.Method, owner );
        }


        public static ConsoleCommand Create<A, B, C, D>( string name, Action<A, B, C, D> method, object owner )
        {
            return new ConsoleCommand( name, method.Method, owner );
        }


        public static ConsoleCommand Create<A, B, C, D, E>( string name, Action<A, B, C, D, E> method, object owner )
        {
            return new ConsoleCommand( name, method.Method, owner );
        }


        public static ConsoleCommand Create<A>( string name, Func<A> method, object owner )
        {
            return new ConsoleCommand( name, method.Method, owner );
        }


        public static ConsoleCommand Create<A, B>( string name, Func<A, B> method, object owner )
        {
            return new ConsoleCommand( name, method.Method, owner );
        }


        public static ConsoleCommand Create<A, B, C>( string name, Func<A, B, C> method, object owner )
        {
            return new ConsoleCommand( name, method.Method, owner );
        }


        public static ConsoleCommand Create<A, B, C, D>( string name, Func<A, B, C, D> method, object owner )
        {
            return new ConsoleCommand( name, method.Method, owner );
        }


        // Public:


        public ConsoleCommand( string name, MethodInfo methodInfo, object owner )
        {
            this.name           = name;
            this.methodInfo     = methodInfo;
            this.owner          = owner;

            returnTypeName      = methodInfo.ReturnType.Name;

            var parameterInfos  = methodInfo.GetParameters();
            parameters          = new ConsoleCommandArg[parameterInfos.Length];
            for( int i = 0; i < parameters.Length; ++i )
            {
                var parameter       = new ConsoleCommandArg();
                parameter.Name      = parameterInfos[i].Name;
                parameter.TypeName  = parameterInfos[i].ParameterType.Name;
                parameters[i]       = parameter;
            }
        }


        public MethodInfo MethodInfo
        {
            get
            {
                return methodInfo;
            }
        }


        public object Owner
        {
            get
            {
                return owner;
            }
        }


        public string Name
        {
            get
            {
                return name;
            }
        }


        public string ReturnTypeName
        {
            get
            {
                return returnTypeName;
            }
        }
        

        // Private:


        private string                              name            = null;
        private object                              owner           = null;
        private MethodInfo                          methodInfo      = null;
        private ConsoleCommandArg[]                 parameters      = null;
        private string                              returnTypeName  = null;
    }
}

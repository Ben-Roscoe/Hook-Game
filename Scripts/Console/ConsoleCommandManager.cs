using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine.Assertions;

namespace Nixin
{
    public enum InvokeConsoleCommandErrorType
    {
        None,
        CommandNotFound,
        InvalidArgs,
        InvalidArgCount,
    }

    public class ConsoleCommandManager
    {


        // Public:


        public ConsoleCommandManager( World containingWorld )
        {
            this.containingWorld = containingWorld;
            AddCommandsInAssembly();
        }


        public void AddCommand( ConsoleCommand command )
        {
            Assert.IsTrue( !commands.ContainsKey( command.Name ), "Duplicate console command name: " + command.Name );
            commands.Add( command.Name, command );
        }


        public void RemoveCommand( ConsoleCommand command )
        {
            commands.Remove( command.Name );
        }


        public InvokeConsoleCommandErrorType InvokeCommand( ConsoleCommandCall call )
        {
            object outObject = null;
            return InvokeCommand( call, out outObject );
        }


        public InvokeConsoleCommandErrorType InvokeCommand( ConsoleCommandCall call, out object returnObject )
        {
            // First parameter is always the world.
            object[] finalArgs = new object[call.ArgInstances.Count + 1];
            finalArgs[0]       = containingWorld;
            for( int i = 1; i < finalArgs.Length; ++i )
            {
                if( !call.ArgInstances[i - 1].IsCommand )
                {
                    finalArgs[i] = call.ArgInstances[i - 1].Data;
                    continue;
                }

                object argReturnObject = null;
                InvokeCommand( ( ConsoleCommandCall )call.ArgInstances[i - 1].Data, out argReturnObject );
                finalArgs[i] = argReturnObject;
            }

            ConsoleCommand outCommand = null;
            if( !commands.TryGetValue( call.CommandName, out outCommand ) )
            {
                returnObject = null;
                return InvokeConsoleCommandErrorType.CommandNotFound;
            }

            try
            {
                returnObject = outCommand.MethodInfo.Invoke( outCommand.Owner, finalArgs );
                return InvokeConsoleCommandErrorType.None;
            }
            catch( TargetParameterCountException e )
            {
                returnObject = null;
                return InvokeConsoleCommandErrorType.InvalidArgCount;
            }
            catch( ArgumentException e )
            {
                returnObject = null;
                return InvokeConsoleCommandErrorType.InvalidArgs;
            }
        }


        // Private:


        private Dictionary<string, ConsoleCommand>           commands           = new Dictionary<string, ConsoleCommand>();
        private World                                        containingWorld    = null;


        private void AddCommandsInAssembly()
        {

            // Add all the console commands in the assembly that dereive from ConsoleCommandDefs. These must be static and must accept a world as their first
            // parameter.
            var assembly = Assembly.GetAssembly( typeof( ConsoleCommandManager ) );
            var classMethods  = assembly.GetTypes().Where( x => x.IsSubclassOf( typeof( ConsoleCommandDefs ) ) ).Select( y => y.GetMethods().Where( z => z.GetCustomAttributes( typeof( ConsoleCommandDefAttribute ), true ).Any() ).ToList() ).ToList();
            for( int i = 0; i < classMethods.Count; ++i )
            {
                for( int j = 0; j < classMethods[i].Count; ++j )
                {
                    // Get the console command def attribute.
                    var attributes = classMethods[i][j].GetCustomAttributes( typeof( ConsoleCommandDefAttribute ), true );
                    var attribute  = ( ConsoleCommandDefAttribute )( attributes.Length > 0 ? attributes[0] : null );
                    if( attribute == null )
                    {
                        continue;
                    }
                    
                    // Make sure the world is the first parameter.
                    var parameters          = classMethods[i][j].GetParameters();
                    var isSubclassOfWorld   = parameters[0].ParameterType.IsSubclassOf( typeof( World ) );
                    var isWorld             = parameters[0].ParameterType == typeof( World );
                    if( parameters.Length <= 0 || ( !isSubclassOfWorld && !isWorld ) )
                    {
                        NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Gameplay, 
                                                    string.Format( "Console command method {0} must have a world type as it's first parameter. Skipping.", classMethods[i][j].Name ) );
                        continue;
                    }
                    AddCommand( new ConsoleCommand( attribute.CommandName, classMethods[i][j], null ) );
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class ConsoleInterpreter
    {


        // Public:


        public ConsoleCommandCall ParseCommand( string input )
        {
            input               = input.Trim();
            var firstWhiteSpace = input.IndexOf( ' ', 0 );
            var parameterStart  = input.IndexOf( '(', 0 );
            var commandNameEnd  = -1;
            if( firstWhiteSpace == -1 || parameterStart == -1 )
            {
                commandNameEnd = Math.Max( firstWhiteSpace, parameterStart );
            }
            else
            {
                commandNameEnd = Math.Min( firstWhiteSpace, parameterStart );
            }

            if( commandNameEnd <= 0 )
            {
                return null;
            }
            var commandName     = input.Substring( 0, commandNameEnd );

            // Find the last bracket in the input.
            int parameterEnd;
            for( parameterEnd = input.Length - 1; parameterEnd > parameterStart; --parameterEnd )
            {
                if( input[parameterEnd] == ')' )
                {
                    break;
                }
            }

            List<ConsoleCommandArgInstance> parameters = new List<ConsoleCommandArgInstance>();
            if( parameterEnd > parameterStart + 1 )
            {
                var parameterList   = input.Substring( parameterStart + 1, ( parameterEnd - 1 ) - parameterStart );
                parameters          = ParseParameters( parameterList );
            }

            ConsoleCommandCall  call = new ConsoleCommandCall( commandName, parameters );
            return call;
        }


        // Private:



        private List<ConsoleCommandArgInstance> ParseParameters( string input )
        {
            var parameterStrings    = input.Split( ',' );
            if( parameterStrings.Length == 1 && parameterStrings[0].Trim() == "" )
            {
                return new List<ConsoleCommandArgInstance>();
            }

            var parameters          = new List<ConsoleCommandArgInstance>();
            for( int i = 0; i < parameterStrings.Length; ++i )
            {
                parameterStrings[i]                     = parameterStrings[i].Trim();
                ConsoleCommandArgInstance       arg     = new ConsoleCommandArgInstance();

                if( parameterStrings[i].Length <= 0 )
                {
                    continue;
                }

                // String.
                if( parameterStrings[i][0] == '\"' && parameterStrings[i][parameterStrings[i].Length - 1] == '\"' )
                {
                    arg.IsCommand   = false;
                    arg.Data        = parameterStrings[i].Substring( 1, parameterStrings[i].Length - 2 );
                    parameters.Add( arg );
                    continue;
                }

                // Integer.
                int outInt = 0;
                if( int.TryParse( parameterStrings[i], out outInt ) )
                {
                    arg.IsCommand = false;
                    arg.Data    = outInt;
                    parameters.Add( arg );
                    continue;
                }

                // Float.
                float outFloat = 0.0f;
                if( float.TryParse( parameterStrings[i], out outFloat ) )
                {
                    arg.IsCommand = false;
                    arg.Data    = outFloat;
                    parameters.Add( arg );
                    continue;
                }

                // Bool.
                bool outBool = false;
                if( bool.TryParse( parameterStrings[i], out outBool ) )
                {
                    arg.IsCommand = false;
                    arg.Data = outBool;
                    parameters.Add( arg );
                    continue;
                }

                // Command.
                if( parameterStrings[i].Contains( '(' ) && parameterStrings[i].Contains( ')' ) )
                {
                    arg.IsCommand = true;
                    arg.Data    = ParseCommand( parameterStrings[i] );
                    parameters.Add( arg );
                    continue;
                }

                // ERROR.
            }
            return parameters;
        }
    }
}

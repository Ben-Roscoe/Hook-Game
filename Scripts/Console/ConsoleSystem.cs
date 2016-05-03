using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine.Assertions;

namespace Nixin
{
    public class ConsoleSystem : NixinSystem
    {


        // Public:


        public ConsoleSystem( World containingWorld ) : base( containingWorld )
        {
            commandManager = new ConsoleCommandManager( containingWorld );
            for( int i = 0; i < InputBuffer.Length; ++i )
            {
                InputBuffer[i] = "";
            }
        }


        public void SendTextInput( string input )
        {
            BroadcastNewLine( input, false );
        }


        public void SendCommandInput( string input )
        {
            InsertInput( input );
            ++inputCount;
            BroadcastNewLine( input, true );
            
            var call = interpreter.ParseCommand( input );
            if( call == null )
            {
                BroadcastNewLine( unableToParseCommandError, false );
                return;
            }

            var error = CommandManager.InvokeCommand( call );
            if( error == InvokeConsoleCommandErrorType.CommandNotFound )
            {
                BroadcastNewLine( commandNotFoundError, false );
            }
            else if( error == InvokeConsoleCommandErrorType.InvalidArgCount )
            {
                BroadcastNewLine( invalidParameterCountError, false );
            }
            else if( error == InvokeConsoleCommandErrorType.InvalidArgs )
            {
                BroadcastNewLine( invalidParametersError, false );
            }
        }


        public NixinEvent<ConsoleSystem, string, bool> OnNewLineInserted
        {
            get
            {
                return onNewLineInserted;
            }
        }


        public ConsoleCommandManager CommandManager
        {
            get
            {
                return commandManager;
            }
        }


        public string[] InputBuffer
        {
            get
            {
                return inputBuffer;
            }
        }


        public int InputCount
        {
            get
            {
                return inputCount;
            }
        }


        // Private:


        private const int                           inputBufferSize                 = 20;
        private const string                        unableToParseCommandError       = "Unable to parse command.";
        private const string                        commandNotFoundError            = "Command not found.";
        private const string                        invalidParameterCountError      = "Invalid parameter count.";
        private const string                        invalidParametersError          = "Invalid parameters.";

        private ConsoleInterpreter                  interpreter                     = new ConsoleInterpreter();
        private ConsoleCommandManager               commandManager                  = null;
        private string[]                            inputBuffer                     = new string[inputBufferSize];
        private int                                 inputCount                      = 0;

        private NixinEvent<ConsoleSystem, string, bool>   onNewLineInserted               = new NixinEvent<ConsoleSystem, string, bool>();


        private void InsertInput( string input )
        {
            // Shift the input buffer and insert the new input.
            for( int i = inputBuffer.Length - 2; i > 0; --i )
            {
                inputBuffer[i] = inputBuffer[i - 1];
            }
            inputBuffer[0] = input;
        }


        private void BroadcastNewLine( string str, bool isCommand )
        {
            onNewLineInserted.Invoke( this, str, isCommand );
        }
    }
}

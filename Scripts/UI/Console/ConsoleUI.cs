using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nixin
{
    public class ConsoleUI : UIActor
    {


        // Public:


        public override void OnPostHierarchyInitialise()
        {
            base.OnPostHierarchyInitialise();

            displayText.supportRichText = true;
            displayText.text            = "";
            
            enterButton.onClick.AddListener( OnEnterButtonPressed );
            ContainingWorld.ConsoleSystem.OnNewLineInserted.AddHandler( OnConsoleNewLineInserted );

            input.OnSubmit.AddHandler( OnTextInputSubmit );
            input.InputComponent.BindAction( "ConsoleMoveUpBuffer", InputState.Down, OnConsoleMoveUpBufferActivated, this );
            input.InputComponent.BindAction( "ConsoleMoveDownBuffer", InputState.Down, OnCosoleMoveDownBufferActivated, this );

            Hide();
            SetShowHideEffect( new ConsoleUIShowHideEffect( this, 1200.0f ) );

            // Allow the show hide effect to update.
            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
                UpdateComponent.UpdateRate      = 0.0f;
            }
        }


        public override void EnableDontDestroyOnLoad()
        {
            base.EnableDontDestroyOnLoad();
            input.EnableDontDestroyOnLoad();
        }


        public override void DisableDontDestroyOnLoad()
        {
            base.DisableDontDestroyOnLoad();
            input.DisableDontDestroyOnLoad();
        }


        public override void OnActorDestroy()
        {
            base.OnActorDestroy();
            ContainingWorld.ConsoleSystem.OnNewLineInserted.RemoveHandler( OnConsoleNewLineInserted );
            input.OnSubmit.RemoveHandler( OnTextInputSubmit );
            input.InputComponent.RemoveAllWithOwner( this );
        }


        public override void OnShow()
        {
            base.OnShow();

            // Make sure the text is focused when we show the console.
            if( !ContainingWorld.EventSystem.alreadySelecting && ContainingWorld.EventSystem.currentSelectedGameObject != input.gameObject )
            {
                ContainingWorld.EventSystem.SetSelectedGameObject( input.gameObject );
            }

            // Move the caret to the end of the text.
            input.InputFieldComponent.ActivateInputField();
            input.MoveTextToEnd( false );
        }


        public override void OnHide()
        {
            base.OnHide();
            input.InputFieldComponent.text = "";
        }


        // Private:


        private const string            commandPrefix       = "> ";

        [SerializeField, FormerlySerializedAs( "DisplayText" )]
        private Text                    displayText = null;

        [SerializeField, FormerlySerializedAs( "Input" )]
        private TextInput               input       = null;

        [SerializeField, FormerlySerializedAs( "EnterButton" )]
        private Button                  enterButton = null;

        private int                     bufferPosition = -1;


        private void OnConsoleMoveUpBufferActivated()
        {
            MoveInBuffer( 1 );
        }


        private void OnCosoleMoveDownBufferActivated()
        {
            MoveInBuffer( -1 );
        }


        private void MoveInBuffer( int delta )
        {
            if( ContainingWorld.ConsoleSystem.InputCount > 0 )
            {
                bufferPosition += delta;
                bufferPosition = Mathf.Clamp( bufferPosition, 0, Mathf.Min( ContainingWorld.ConsoleSystem.InputBuffer.Length - 1, ContainingWorld.ConsoleSystem.InputCount - 1 ) );
                input.InputFieldComponent.text = ContainingWorld.ConsoleSystem.InputBuffer[bufferPosition];
            }
        }


        private void OnTextInputSubmit( TextInput textInput )
        {
            SendInputToConsole();
        }


        private void OnEnterButtonPressed()
        {
            SendInputToConsole();
        }


        private void SendInputToConsole()
        {
            bufferPosition = -1;
            if( string.IsNullOrEmpty( input.InputFieldComponent.text ) )
            {
                return;
            }
            ContainingWorld.ConsoleSystem.SendCommandInput( input.InputFieldComponent.text );
            input.InputFieldComponent.text = "";
        }


        private void OnConsoleNewLineInserted( ConsoleSystem consoleSystem, string line, bool isCommand )
        {
            var finalLine = line + "\n";
            if( isCommand )
            {
                finalLine = commandPrefix + finalLine;
            }
            displayText.text += finalLine;
        }
    }
}

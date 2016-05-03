using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nixin
{
    public class TextInput : UIActor
    {


        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

            inputFieldComponent     = ConstructDefaultStaticComponent<InputField>( this, "InputFieldComponent", inputFieldComponent );
        }


        public void MoveTextToEnd( bool shift )
        {
            StartCoroutine( MoveTextToEndNextFrame( shift ) );
        }


        public InputField InputFieldComponent
        {
            get
            {
                return inputFieldComponent;
            }
        }


        public NixinEvent<TextInput> OnSubmit
        {
            get
            {
                return onSubmit;
            }
        }


        // Protected:


        protected override void SetupInputComponent( InputComponent inputComponent )
        {
            base.SetupInputComponent( inputComponent );

            inputComponent.BindAction( "SubmitInput", InputState.Down, OnSubmitButtonPressed, this );
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "InputFieldComponent" ), HideInInspector ]
        private InputField                  inputFieldComponent = null;

        private NixinEvent<TextInput>       onSubmit            = new NixinEvent<TextInput>();


        private void OnSubmitButtonPressed()
        {
            OnSubmit.Invoke( this );
            InputFieldComponent.ActivateInputField();
        }


        private IEnumerator MoveTextToEndNextFrame( bool shift )
        {
            yield return null;
            InputFieldComponent.MoveTextEnd( shift );
        }
    }
}

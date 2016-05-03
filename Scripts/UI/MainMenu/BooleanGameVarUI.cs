using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nixin
{
    public class BooleanGameVarUI : GameVarUI
    {


        // Public:


        public override void OnPostHierarchyInitialise()
        {
            base.OnPostHierarchyInitialise();

            if( ContainingWorld.NetworkSystem.IsAuthoritative )
            {
                optionToggle.onValueChanged.AddListener( OnOptionToggleChangeValue );
            }
        }


        public override void SetInteractable( bool interactable )
        {
            base.SetInteractable( interactable );

            optionToggle.interactable = interactable;
        }


        public Toggle OptionToggle
        {
            get
            {
                return optionToggle;
            }
        }


        // Protected:


        protected override void ResetToDefault()
        {
            base.ResetToDefault();

            if( GameVar != null && GameVarDecl != null )
            {
                var type = GameVarDecl as BoolGameVarDeclAttribute;
                Assert.IsTrue( type != null );
                GameVar.SetBool( type.DefaultValue );
                optionToggle.isOn = GameVar.GetBool();
            }
        }


        protected override void UpdateGameVarValue()
        {
            base.UpdateGameVarValue();
            optionToggle.isOn = GameVar.GetBool();
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "OptionToggle" )]
        private Toggle         optionToggle     = null;


        private void OnOptionToggleChangeValue( bool v )
        {
            GameVar.SetBool( v );
        }



    }
}

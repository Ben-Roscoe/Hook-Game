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
    public class FloatGameVarUI : GameVarUI
    {


        // Public:


        public override void OnPostHierarchyInitialise()
        {
            base.OnPostHierarchyInitialise();

            if( ContainingWorld.NetworkSystem.IsAuthoritative )
            {
                optionSlider.onValueChanged.AddListener( OnOptionSliderValueChanged );
            }
        }


        public override void SetInteractable( bool interactable )
        {
            base.SetInteractable( interactable );

            optionSlider.interactable = interactable;
        }


        public Slider OptionSlider
        {
            get
            {
                return optionSlider;
            }
        }


        // Protected:


        protected override void ResetToDefault()
        {
            base.ResetToDefault();

            if( GameVar != null && GameVarDecl != null )
            {
                var type = GameVarDecl as FloatGameVarDeclAttribute;
                Assert.IsTrue( type != null );
                GameVar.SetFloat( type.DefaultValue );
                optionSlider.value = GameVar.GetFloat();
            }
        }


        protected override void OnGameVarDeclChanged()
        {
            base.OnGameVarDeclChanged();

            if( GameVarDecl != null )
            {
                var type = GameVarDecl as FloatGameVarDeclAttribute;
                Assert.IsTrue( type != null );
                optionSlider.minValue = type.Min;
                optionSlider.maxValue = type.Max;
            }
        }


        protected override void UpdateGameVarValue()
        {
            base.UpdateGameVarValue();
            optionSlider.value      = GameVar.GetFloat();
            currentValueText.text   = GameVar.GetFloat().ToString( "#.#" );
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "OptionSlider" )]
        private Slider          optionSlider        = null;

        [SerializeField, FormerlySerializedAs( "CurrentValueText" )]
        private Text            currentValueText    = null;


        private void OnOptionSliderValueChanged( float v )
        {
            GameVar.SetFloat( v );
        }
    }
}

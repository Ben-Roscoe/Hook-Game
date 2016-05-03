using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class StatModifier
    {


        // Public:


        public StatModifier( float value, StatModifierType modifierType, bool isPermanent, bool isMultiplier, bool shouldShowOnScreen )
        {
            this.value                  = value;
            this.modifierType           = modifierType;
            this.isPermanent            = isPermanent;
            this.isMultiplier           = isMultiplier;
            this.shouldShowOnScreen     = shouldShowOnScreen;
        }


        public float GetModifiedValue( float baseValue )
        {
            return IsMultiplier ? baseValue * value : value;
        }


        public bool IsPermanent
        {
            get
            {
                return isPermanent;
            }
        }


        public bool IsMultiplier
        {
            get
            {
                return isMultiplier;
            }
        }


        public float Value
        {
            get
            {
                return value;
            }
        }


        public bool ShouldShowOnScreen
        {
            get
            {
                return shouldShowOnScreen;
            }
        }


        public StatModifierType ModifierType
        {
            get
            {
                return modifierType;
            }
        }


        // Private:


        private float               value;
        private StatModifierType    modifierType;

        private bool                isPermanent;
        private bool                isMultiplier;
        private bool                shouldShowOnScreen;
    }
}

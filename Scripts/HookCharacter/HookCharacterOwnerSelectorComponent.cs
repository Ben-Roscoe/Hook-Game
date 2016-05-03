using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class HookCharacterOwnerSelectorComponent : SelectorComponent
    {


        // Public:


        public override void OnRegistered( Actor owner, byte id )
        {
            base.OnRegistered( owner, id );

            if( Owner == null )
            {
                ownerComponent = null;
                return;
            }

            ownerComponent = Owner.GetNixinComponent<HookCharacterOwnerComponent>();
            if( ownerComponent == null )
            {
                ownerComponent = Owner.GetComponent<HookCharacterOwnerComponent>();
            }
        }


        public override bool CanSelect
        {
            get
            {
                return ( ownerComponent == null || ownerComponent.HookCharacter == null ) ? false : ownerComponent.HookCharacter.IsMovementFree;
            }
        }


        // Private:


        private HookCharacterOwnerComponent     ownerComponent = null;
    }
}

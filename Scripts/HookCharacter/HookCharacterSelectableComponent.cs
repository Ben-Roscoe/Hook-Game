using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class HookCharacterSelectableComponent : SelectableComponent
    {


        // Public:


        public override void OnSelected( SelectorComponent selector, SelectableType type )
        {
            base.OnSelected( selector, type );

            // Server only.
            if( !Owner.IsAuthority )
            {
                return;
            }

            // Need to be a hook character ourselves.
            if( ( Owner as HookCharacterControllableActor ) == null )
            {
                return;
            }

            // Must be selected by an owner of a hook character.
            var hookCharacterOwnerComponent = ( selector == null || selector.Owner == null ) ? null : selector.Owner.GetNixinComponent<HookCharacterOwnerComponent>();
            if( hookCharacterOwnerComponent == null )
            {
                return;
            }

            // Move the hook character to us. 
            var hookCharacter = hookCharacterOwnerComponent.HookCharacter;
            if( hookCharacter == null )
            {
                return;
            }
            hookCharacter.NavigateToActor( Owner );
        }



        public override bool CanBeSelected
        {
            get
            {
                var hookCharacter = Owner as HookCharacterControllableActor;
                if( hookCharacter == null )
                {
                    return false;
                }
                return !hookCharacter.IsDying;
            }
        }


        public override bool CanBeHovered
        {
            get
            {
                return CanBeSelected;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class SelectorComponent : NixinComponent
    {


        // Public:


        public override void OnRegistered( Actor owner, byte id )
        {
            base.OnRegistered( owner, id );

            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
                UpdateComponent.UpdateRate = 0.0f;
            }
        }


        public void HoverSelect( SelectableComponent selectable )
        {
            if( hoveredComponent != null && hoveredComponent != selectable )
            {
                hoveredComponent.OnHoverDeselect( this );
            }
            hoveredComponent = selectable;
            if( hoveredComponent != null )
            {
                hoveredComponent.OnHoverSelect( this );
            }
        }


        public void Select( SelectableComponent selectable )
        {
            if( selectedComponent != null && selectedComponent != selectable )
            {
                selectedComponent.OnDeselect( this );
            }

            if( CanSelect )
            {
                selectedComponent = selectable;
                if( selectedComponent != null )
                {
                    selectedComponent.OnSelect( this );
                }
            }
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );


        }


        public virtual bool CanSelect
        {
            get
            {
                return true;
            }
        }


        public SelectableComponent HoveredComponent
        {
            get
            {
                return hoveredComponent;
            }
        }


        public SelectableComponent SelectedComponent
        {
            get
            {
                return selectedComponent;
            }
        }


        // Private:


        private SelectableComponent     hoveredComponent    = null;
        private SelectableComponent     selectedComponent   = null;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Nixin
{
    public class SelectableComponent : NixinComponent
    {


        // Public:


        public override void OnRegistered( Actor owner, byte id )
        {
            base.OnRegistered( owner, id );

            Owner.RegisterRPC<SelectorComponent>( ServerOnSelected );
            Owner.RegisterRPC<SelectorComponent>( ServerOnDeselect );

            hookGameWorld = Owner.ContainingWorld as HookGameWorld;
            Assert.IsTrue( hookGameWorld != null, "Owner must be in a hook game world." );

            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
                UpdateComponent.UpdateRate      = 0.0f;
            }

            if( useOutlineComponent )
            {
                outlineComponent = new SelectableOutlineComponent( this );
            }
        }


        public override void OnUnregistered()
        {
            base.OnUnregistered();

            if( useOutlineComponent )
            {
                outlineComponent.Uninitialise();
            }
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );
            if( useOutlineComponent )
            {
                outlineComponent.Update( deltaTime );
            }
        }


        public void OnHoverSelect( SelectorComponent selector )
        {
            if( CanBeHovered )
            {
                selectableType  = GetSelectableType( selector );
                if( useOutlineComponent )
                {
                    outlineComponent.OnHoverSelect( selectableType );
                }
                isHovered       = true;
            }
        }


        public void OnHoverDeselect( SelectorComponent selector )
        {
            if( useOutlineComponent )
            {
                outlineComponent.OnHoverDeselect( selectableType );
            }
            isHovered = false;
        }


        public void OnSelect( SelectorComponent selector )
        {
            if( !isSelected && selector.Owner != null && CanBeSelected )
            {
                selectableType = GetSelectableType( selector );
                if( useOutlineComponent )
                {
                    outlineComponent.OnSelect( selectableType );
                }
                isSelected = true;

                if( Owner.IsAuthority && selector.Owner.IsAuthority )
                {
                    ServerOnSelected( selector );
                }
                else
                {
                    ContainingWorld.RPC( ServerOnSelected, RPCType.Server, Owner, selector );
                }
            }
        }


        public void OnDeselect( SelectorComponent selector )
        {
            if( isSelected && selector.Owner != null )
            {
                if( useOutlineComponent )
                {
                    outlineComponent.OnHoverDeselect( selectableType );
                }
                isSelected = false;

                if( Owner.IsAuthority && selector.Owner.IsAuthority )
                {
                    ServerOnDeselect( selector );
                }
                else
                {
                    ContainingWorld.RPC( ServerOnDeselect, RPCType.Server, Owner, selector );
                }
            }
        }


        public virtual void OnSelected( SelectorComponent selector, SelectableType type )
        {
        }


        public virtual void OnDeselected( SelectorComponent selector, SelectableType type )
        {
        }


        public HookGameWorld HookGameWorld
        {
            get
            {
                return hookGameWorld;
            }
        }


        public SelectableType SelectableType
        {
            get
            {
                return selectableType;
            }
        }


        public bool IsHovered
        {
            get
            {
                return isHovered;
            }
        }


        public virtual bool CanBeSelected
        {
            get
            {
                return true;
            }
        }


        public virtual bool CanBeHovered
        {
            get
            {
                return true;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "UseOutlineComponent" )]
        private bool                        useOutlineComponent = true;

        private bool                        isHovered           = false;
        private bool                        isSelected          = false;
        private SelectableType              selectableType      = SelectableType.Neutral;
        private SelectableOutlineComponent  outlineComponent;
                                          
        private HookGameWorld             hookGameWorld         = null;


        private SelectableType GetSelectableType( SelectorComponent selector )
        {
            var gameState       = ContainingWorld.GameState as HookGameMatchState;
            if( gameState == null )
            {
                NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Gameplay, "Selectable does not work with this game state." );
                return SelectableType.Neutral;
            }

            return gameState.GetSelectableType( this, selector );
        }


        private void ServerOnSelected( SelectorComponent selector )
        {
            OnSelected( selector, GetSelectableType( selector ) );
        }


        private void ServerOnDeselect( SelectorComponent selector )
        {
            OnDeselected( selector, GetSelectableType( selector ) );
        }
    }


    public struct SelectableOutlineComponent
    {


        // Public:


        public SelectableOutlineComponent( SelectableComponent selectableComponent )
        {
            this.selectableComponent    = selectableComponent;
            totalLerpTime               = 0.0f;
            currentLerpTime             = 0.0f;

            currentColour               = Color.clear;
            fromColour                  = Color.clear;
            toColour                    = Color.clear;

            state                       = SelectableState.None;
            currentWidth                = selectableComponent.HookGameWorld.SelectableOutlineOptions.OutlineWidth;

            SetOutlineColours();
            SetOutlineWidths();

            selectableComponent.HookGameWorld.SelectableOutlineOptions.OutlineWidthChanged.AddHandler( OnOutlineWidthChanged );
        }


        public void Uninitialise()
        {
            selectableComponent.HookGameWorld.SelectableOutlineOptions.OutlineWidthChanged.RemoveHandler( OnOutlineWidthChanged );
        }


        public void Update( float deltaTime )
        {
            if( currentLerpTime < totalLerpTime )
            {
                // Lerp between colours.
                currentLerpTime             += deltaTime;
                var t                       = currentLerpTime / totalLerpTime;
                currentColour               = Color.Lerp( fromColour, toColour, t );
                SetOutlineColours();
            }
            else if( state == SelectableState.Selecting )
            {
                // Wait at the selected colour for a while.
                state                   = SelectableState.SelectingWaiting;
                fromColour              = currentColour;
                toColour                = fromColour;
                totalLerpTime           = selectableComponent.HookGameWorld.SelectableOutlineOptions.SelectStayTime;
                currentLerpTime         = 0.0f;
            }
            else if( state == SelectableState.SelectingWaiting )
            {
                // Lerp back to none/hovered.
                fromColour = currentColour;
                currentLerpTime = 0.0f;
                if( selectableComponent.IsHovered )
                {
                    state               = SelectableState.Hovering;
                    toColour            = selectableComponent.HookGameWorld.SelectableOutlineOptions.GetHoverColour( selectableComponent.SelectableType );
                    totalLerpTime       = selectableComponent.HookGameWorld.SelectableOutlineOptions.HoverSelectLerpTime;
                }
                else
                {
                    state               = SelectableState.None;
                    toColour            = new Color( fromColour.r, fromColour.g, fromColour.b, 0.0f );
                    totalLerpTime       = selectableComponent.HookGameWorld.SelectableOutlineOptions.NoneHoverLerpTime;
                }
            }
        }


        public void OnSelect( SelectableType type )
        {
            if( state != SelectableState.Selecting )
            {
                state               = SelectableState.Selecting;
                fromColour          = currentColour;
                toColour            = selectableComponent.HookGameWorld.SelectableOutlineOptions.GetSelectColour( type );
                totalLerpTime       = selectableComponent.HookGameWorld.SelectableOutlineOptions.HoverSelectLerpTime;
                currentLerpTime     = 0.0f;
            }
        }


        public void OnHoverSelect( SelectableType type )
        {
            if( state == SelectableState.None )
            {
                state               = SelectableState.Hovering;
                toColour            = selectableComponent.HookGameWorld.SelectableOutlineOptions.GetHoverColour( type );

                if( currentColour.a <= 0.005 )
                {
                    fromColour = new Color( toColour.r, toColour.g, toColour.b, 0.0f );
                }
                else
                {
                    fromColour = currentColour;
                }
                totalLerpTime = selectableComponent.HookGameWorld.SelectableOutlineOptions.NoneHoverLerpTime;
                currentLerpTime = 0.0f;
            }
        }


        public void OnHoverDeselect( SelectableType type )
        {
            if( state == SelectableState.Hovering )
            {
                state           = SelectableState.None;
                fromColour      = currentColour;
                toColour        = new Color( fromColour.r, fromColour.g, fromColour.b, 0.0f );
                totalLerpTime   = selectableComponent.HookGameWorld.SelectableOutlineOptions.NoneHoverLerpTime;//Mathf.Clamp( currentLerpTime, 0.0f, 1.0f );
                currentLerpTime = 0.0f;
            }
        }


        public void OnDeselect( SelectableType type )
        {
        }


        // Private:


        private const string        OutlineColourMaterialPropertyName = "_OutlineColor";
        private const string        OutlineLengthMaterialPropertyName = "_Outline";

        private float               totalLerpTime;
        private float               currentLerpTime;

        private Color               currentColour;
        private Color               fromColour;
        private Color               toColour;  

        private SelectableComponent       selectableComponent;
        private SelectableState           state;

        private float               currentWidth;


        private void OnOutlineWidthChanged( float width )
        {
            this.currentWidth = width;
            SetOutlineWidths();
        }


        private void SetOutlineColours()
        {
            selectableComponent.ForeachComponentsInParentAndChildrenRecursive<Renderer>( SetOutlineColour );
        }


        private void SetOutlineColour( Renderer renderer )
        {
            var material = renderer.material;
            if( material == null )
            {
                return;
            }
            material.SetColor( OutlineColourMaterialPropertyName, currentColour );
        }


        private void SetOutlineWidths()
        {
            selectableComponent.ForeachComponentsInParentAndChildrenRecursive<Renderer>( SetOutlineWidth );
        }


        private void SetOutlineWidth( Renderer renderer )
        {
            var material = renderer.material;
            if( material == null )
            {
                return;
            }
            material.SetFloat( OutlineLengthMaterialPropertyName, currentWidth );
        }
    }

    public enum SelectableState
    {
        None,
        Hovering,
        Selecting,
        SelectingWaiting,
    }

    public enum SelectableType
    {
        Good,
        Bad,
        Neutral,
    }
}

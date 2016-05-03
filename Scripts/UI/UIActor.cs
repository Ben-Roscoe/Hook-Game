using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Nixin
{
    public class UIActor : Actor
    {


        // Public:


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, 
            Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );
            rectTransformComponent = GetComponent<RectTransform>();
            inputComponent = new InputComponent( ContainingWorld );
            isVisible = gameObject.activeSelf;
            SetShowHideEffect( new DefaultUIShowHideEffect( this ) );
        }


        public override void OnPostHierarchyInitialise()
        {
            base.OnPostHierarchyInitialise();
            SetupInputComponent( inputComponent );
        }


        public override void OnActorDestroy()
        {
            base.OnActorDestroy();
            inputComponent.Uninitialise();
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );
            if( showHideEffect != null )
            {
                showHideEffect.Update( deltaTime );
            }
        }


        public override void OnLocalisationChanged()
        {
            base.OnLocalisationChanged();
            SetLocalText();
        }


        public virtual void SetInteractable( bool interactable )
        {

        }


        public virtual void SetLocalText()
        {
        }


        public virtual void OnStartShow()
        {
        }


        public virtual void OnStartHide()
        {
        }


        public virtual void OnShow()
        {
            isVisible = true;
        }


        public virtual void OnHide()
        {
            isVisible = false;
        }


        public void SetShowHideEffect( UIShowHideEffect newEffect )
        {
            if( showHideEffect != newEffect )
            {
                if( showHideEffect != null )
                {
                    showHideEffect.Stop( newEffect );
                }
                showHideEffect = newEffect;
                if( newEffect != null )
                {
                    newEffect.Start( IsVisible );
                }
            }
        }


        public void UpdateInput( float deltaTime )
        {
            inputComponent.UpdateInput( deltaTime );
        }


        public void Show()
        {
            if( !IsVisible && showHideEffect != null )
            {
                showHideEffect.Show();
            }
        }


        public void Hide()
        {
            if( IsVisible && showHideEffect != null )
            {
                showHideEffect.Hide();
            }
        }


        public bool IsVisible
        {
            get
            {
                return isVisible;
            }
        }


        public InputComponent InputComponent
        {
            get
            {
                return inputComponent;
            }
        }


        public RectTransform RectTransformComponent
        {
            get
            {
                return rectTransformComponent;
            }
        }


        public bool IsTransitioning
        {
            get
            {
                return showHideEffect == null ? false : showHideEffect.IsTransitioning;
            }
        }


        // Protected:


        protected virtual void SetupInputComponent( InputComponent inputComponent )
        {
        }


        // Private:


        private InputComponent      inputComponent      = null;
        private RectTransform       rectTransformComponent = null;

        private UIShowHideEffect    showHideEffect      = null;
        private bool                isVisible           = true;

        private CanvasGroup         canvasGroup         = null;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public abstract class UIShowHideEffect
    {


        // Public:


        public UIShowHideEffect( UIActor uiActor )
        {
            this.uiActor        = uiActor;
        }


        public virtual void Start( bool isShowing )
        {
        }


        public virtual void Stop( UIShowHideEffect newEffect )
        {
        }


        public virtual void Update( float deltaTime )
        {
        }


        public virtual void Show()
        {
            if( !uiActor.IsVisible )
            {
                UIActor.OnStartShow();
                BeginTransition();
            }
        }


        public virtual void Hide()
        {
            if( uiActor.IsVisible )
            {
                UIActor.OnStartHide();
                BeginTransition();
            }
        }


        public UIActor UIActor
        {
            get
            {
                return uiActor;
            }
        }


        public bool IsTransitioning
        {
            get
            {
                return isTransitioning;
            }
        }


        // Protected:


        protected void BeginTransition()
        {
            isTransitioning = true;
        }

        protected void EndTransition( bool showing )
        {
            isTransitioning = false;
            if( showing )
            {
                uiActor.OnShow();
            }
            else
            {
                uiActor.OnHide();
            }
        }


        // Private:


        private UIActor                             uiActor    = null;

        private bool                                isTransitioning = false;
    }
}

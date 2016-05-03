using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nixin
{
    public class FadeInOutUIShowHideEffect : UIShowHideEffect
    {


        // Public:


        public FadeInOutUIShowHideEffect( UIActorGroup uiActor, float fadeTime ) : base( uiActor )
        {
            uiActorGroup    = uiActor;
            this.fadeTime = fadeTime;
        }


        public override void Start( bool isShowing )
        {
            base.Start( isShowing );
            if( isShowing )
            {
                UIActor.gameObject.SetActive( true );
                uiActorGroup.CanvasGroupComponent.alpha = 1.0f;
            }
            else
            {
                UIActor.gameObject.SetActive( false );
                uiActorGroup.CanvasGroupComponent.alpha = 0.0f;
            }
        }


        public override void Show()
        {
            base.Show();

            UIActor.gameObject.SetActive( true );
            currentFadeTime = 0.0f;
            fromAlpha       = 0.0f;
            toAlpha         = 1.0f;
        }


        public override void Hide()
        {
            base.Hide();

            currentFadeTime = 0.0f;
            fromAlpha       = 1.0f;
            toAlpha         = 0.0f;
        }


        public override void Update( float deltaTime )
        {
            base.Update( deltaTime );

            if( IsTransitioning )
            {
                currentFadeTime += deltaTime;
                var t = currentFadeTime / fadeTime;
                uiActorGroup.CanvasGroupComponent.alpha = Mathf.Lerp( fromAlpha, toAlpha, t );
                if( t > 1.0f )
                {
                    if( UIActor.IsVisible )
                    {
                        EndTransition( false );
                    }
                    else
                    {
                        UIActor.gameObject.SetActive( true );
                        EndTransition( true );
                    }
                }
            }
        }


        // Private:


        private UIActorGroup            uiActorGroup    = null;
        private float                   fadeTime        = 1.0f;
        private float                   currentFadeTime = 0.0f;

        private float                   fromAlpha       = 0.0f;
        private float                   toAlpha         = 0.0f;
    }
}

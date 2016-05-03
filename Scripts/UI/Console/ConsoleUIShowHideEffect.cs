using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class ConsoleUIShowHideEffect : UIShowHideEffect
    {


        // Public:


        public ConsoleUIShowHideEffect( UIActor uiActor, float scrollSpeed ) : base( uiActor )
        {
            this.scrollSpeed        = scrollSpeed;
        }


        public override void Start( bool isShowing )
        {
            base.Start( isShowing );

            if( isShowing )
            {
                UIActor.gameObject.SetActive( true );
            }
            else
            {
                UIActor.gameObject.SetActive( false );
            }
        }


        public override void Show()
        {
            base.Show();
            UIActor.gameObject.SetActive( true );
        }


        public override void Update( float deltaTime )
        {
            base.Update( deltaTime );

            yDestination = !UIActor.IsVisible ? 0.0f : -UIActor.RectTransformComponent.rect.height;
            if( IsTransitioning )
            {
                var position = UIActor.RectTransformComponent.position;
                position.y += ( scrollSpeed * deltaTime ) * ( UIActor.IsVisible ? -1.0f : 1.0f );

                if( !UIActor.IsVisible && position.y > yDestination )
                {
                    position.y = yDestination;
                    EndTransition( true );
                }
                else if( UIActor.IsVisible && position.y < yDestination )
                {
                    position.y = yDestination;
                    EndTransition( false );
                    UIActor.gameObject.SetActive( false );
                }
                UIActor.RectTransformComponent.position = position;
            }
            else
            {
                var position = UIActor.RectTransformComponent.position;
                position.y = UIActor.IsVisible ? 0.0f : -UIActor.RectTransformComponent.rect.height;
                UIActor.RectTransformComponent.position = position;
            }
        }


        public float ScrollSpeed
        {
            get
            {
                return scrollSpeed;
            }
            set
            {
                scrollSpeed = value;
            }
        }


        // Private:


        private float           yDestination    = 0.0f;
        private float           scrollSpeed     = 1.0f;
    }
}

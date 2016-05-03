using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Nixin
{
    [RequireComponent( typeof( Canvas ) )]
    public class UICanvasActor : UIActor
    {


        // Public:

          
        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            canvasComponent  = GetComponent<Canvas>();
        }


        public Canvas CanvasComponent
        {
            get
            {
                return canvasComponent;
            }
        }


        // Private:


        private Canvas              canvasComponent     = null;



#if false
        private void GetChildWidgetsRecursive( Transform widget, bool isTopCanvas )
        {
            for( int i = 0; i < widget.childCount; ++i )
            {
                Transform child         = transform.GetChild( i );
                UIActor childUIActor    = child.GetComponent<UIActor>();
                if( childUIActor != null )
                {
                    if( isTopCanvas )
                    {
                        childUIActor.ContainingCanvas = this;
                    }
                    widgets.Add( childUIActor );
                }
                if( childUIActor is UICanvasActor )
                {
                    isTopCanvas = false;
                }
                GetChildWidgetsRecursive( child, isTopCanvas );
            }
        }
#endif
    }
}
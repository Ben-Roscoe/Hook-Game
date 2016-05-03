using UnityEngine;
using System.Collections;
using System;

namespace Nixin
{
    [RequireComponent( typeof( Camera ) )]
    public class CameraActor : Actor
    {


        // Public:


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );
            cameraComponent = GetComponent<Camera>();
        }


        public Camera CameraComponent
        {
            get
            {
                return cameraComponent;
            }
        }


        // Private:


        private Camera              cameraComponent = null;
    }
}

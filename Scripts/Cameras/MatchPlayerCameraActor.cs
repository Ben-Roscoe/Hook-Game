using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class MatchPlayerCameraActor : CameraActor
    {



        // Public:


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
                UpdateComponent.UpdateRate = 0.0f;
            }
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );

            if( isLerpingToPosition )
            {
                float t       = ( Time.time - lerpStartTime ) / ( lerpToTime );
                if( t >= 1.0f )
                {
                    transform.position  = lerpPosition;
                    isLerpingToPosition = false;
                }
                else
                {
                    transform.position      = Vector3.Lerp( transform.position, lerpPosition, t );
                }
            }
        }


        public void Scroll( Vector2 direction, float deltaTime )
        {
            Vector3 deltaPosition = Vector3.zero;

            deltaPosition.x = scrollSpeed * deltaTime * direction.x;
            deltaPosition.z = scrollSpeed * deltaTime * direction.y;

            // If we're lerping, add delta to our target instead so we don't
            // mess up the lerp.
            if( !isLerpingToPosition )
            {
                transform.position += deltaPosition;
            }
            else
            {
                lerpPosition += deltaPosition;
            }
        }


        public void LerpToPosition( Vector3 position )
        {
            var cameraPoint         = new Vector3( position.x, transform.position.y, position.z );
            var ray                 = new Ray( cameraPoint, transform.forward );
            var plane               = new Plane( Vector3.up, position );

            float hitDistance       = 0.0f;
            if( !plane.Raycast( ray, out hitDistance ) )
            {
                return;
            }

            Vector3 hitPoint          = cameraPoint + ( transform.forward * hitDistance );
            Vector3 projection        = Vector3.ProjectOnPlane( cameraPoint - hitPoint, plane.normal );

            lerpPosition            = cameraPoint + projection;

            lerpStartTime           = Time.time;
            isLerpingToPosition     = true;
        }


        public int DistanceFromEdgeToScroll
        {
            get
            {
                return distanceFromEdgeToScroll;
            }
        }


        // Private:


        [SerializeField]
        private int             distanceFromEdgeToScroll    = 5;

        [SerializeField]
        private float           scrollSpeed                 = 1.0f;

        [SerializeField]
        private float           lerpToTime                  = 1.0f;

        private bool            isLerpingToPosition         = false;
        private float           lerpStartTime               = 0.0f;
        private Vector3         lerpPosition                = Vector3.zero;
    }
}

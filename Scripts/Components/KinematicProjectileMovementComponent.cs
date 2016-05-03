using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    class KinematicProjectileMovementComponent : MovementComponent
    {


        // Public


        public override void OnOwningActorFixedUpdate()
        {
            base.OnOwningActorFixedUpdate();
            transform.Translate( velocity, Space.World );
        }


        public bool FaceVelocity
        {
            get
            {
                return faceVelocity;
            }
            set
            {
                faceVelocity = value;
            }
        }


        public Vector3 Velocity
        {
            get
            {
                return velocity;
            }
            set
            {
                velocity = value;
                if( faceVelocity && velocity != Vector3.zero )
                {
                    transform.forward = velocity.normalized;
                }
            }
        }


        // Private:


        [SerializeField]
        private bool        faceVelocity;

        private Vector3     velocity        = Vector3.zero;
    }
}

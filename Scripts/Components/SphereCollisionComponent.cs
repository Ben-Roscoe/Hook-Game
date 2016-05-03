using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class SphereCollisionComponent : CollisionComponent
    {


        // Public:


        public override void EditorComponentConstructor( Actor actor, string name )
        {
            base.EditorComponentConstructor( actor, name );

            sphereCollider        = ConstructDefaultComponent<SphereCollider>( actor, name + ": " + "SphereCollider", sphereCollider );
        }


        public SphereCollider SphereCollider
        {
            get
            {
                return sphereCollider;
            }
            set
            {
                sphereCollider = value;
            }
        }


        public override Collider GetCollider()
        {
            return sphereCollider;
        }


        // Private:


        [SerializeField, HideInInspector]
        new private SphereCollider sphereCollider;
    }
}

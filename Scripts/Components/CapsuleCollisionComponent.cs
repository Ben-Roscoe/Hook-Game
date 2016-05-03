using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class CapsuleCollisionComponent : CollisionComponent
    {


        // Public:


        public override void EditorComponentConstructor( Actor actor, string name )
        {
            base.EditorComponentConstructor( actor, name );

            capsuleCollider = ConstructDefaultComponent<CapsuleCollider>( actor, name + ":" + "CapsuleCollider", capsuleCollider );
        }


        public override Collider GetCollider()
        {
            return capsuleCollider;
        }


        // Private:


        [SerializeField, HideInInspector]
        private CapsuleCollider capsuleCollider;
    }
}

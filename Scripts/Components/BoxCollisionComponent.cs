using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class BoxCollisionComponent : CollisionComponent
    {


        // Public:


        public override void EditorComponentConstructor( Actor actor, string name )
        {
 	        base.EditorComponentConstructor(actor, name);

            boxCollider     = ConstructDefaultComponent<BoxCollider>( actor, name + ":" + "BoxCollider", boxCollider );
        }


        public override Collider GetCollider()
        {
            return boxCollider;
        }


        // Private:


        [SerializeField, HideInInspector]
        private BoxCollider         boxCollider;
    }
}

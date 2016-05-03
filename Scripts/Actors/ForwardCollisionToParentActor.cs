using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nixin
{
    public class ForwardCollisionToParentActor : Actor
    {


        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

            if( collisionComponentType != null && collisionComponentType.Type != null )
            {
                collisionComponent = ( CollisionComponent )ConstructDefaultComponent( this, "CollisionComponent", 
                    collisionComponent, collisionComponentType.Type );
                if( collisionComponent != null )
                {
                    collisionComponent.ForwardToParent = true;
                }
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "CollisionComponentType" )]
        private SubClassOfCollisionComponent        collisionComponentType;

        [SerializeField, FormerlySerializedAs( "CollisionComponent" ), HideInInspector]
        private CollisionComponent                  collisionComponent = null;
    }
}

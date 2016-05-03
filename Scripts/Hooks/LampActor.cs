using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    class LampActor : Actor, IGrapplable
    {


        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

            staticMeshRendererComponent = ConstructDefaultStaticComponent<StaticMeshRendererComponent>( this, "StaticMeshRendererComponent", staticMeshRendererComponent );
            boxCollisionComponent       = ConstructDefaultComponent<BoxCollisionComponent>( this, "BoxCollisionComponent", boxCollisionComponent );
            navMeshObstacleComponent        = ConstructDefaultComponent<NavMeshObstacle>( this, "NavMeshObstacleComponent", navMeshObstacleComponent );
        }


        public virtual bool StartGrapple( HookActor hook )
        {
            return true;
        }


        public virtual void EndGrapple( HookActor hook )
        {

        }


        // Private:


        [SerializeField, HideInInspector]
        StaticMeshRendererComponent             staticMeshRendererComponent;

        [SerializeField, HideInInspector]
        BoxCollisionComponent                   boxCollisionComponent;

        [SerializeField, HideInInspector]
        NavMeshObstacle                         navMeshObstacleComponent;
    }
}

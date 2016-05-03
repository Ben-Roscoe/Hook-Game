using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nixin
{
    public class FountainActor : Actor
    {


        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

            collisionComponent = ConstructDefaultComponent<CapsuleCollisionComponent>( this, "CollisionComponent",
                collisionComponent );
            selectableComponent     = ConstructDefaultComponent<SelectableComponent>( this, "SelectableComponent",
                selectableComponent );
        }


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, 
            Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            healRadiusAbilityInstance = healRadiusAbilityMeta.CreateInstance( this, this, 
                AbilityDefs.fountainHealRadius, healRadius, null, transform.position );
        }


        public override void OnActorDestroy()
        {
            base.OnActorDestroy();

            healRadiusAbilityMeta.DestroyInstance( healRadiusAbilityInstance );
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "SelectableComponent" ), HideInInspector]
        private SelectableComponent                 selectableComponent   = null;

        [SerializeField, FormerlySerializedAs( "HealRadiusOvertimeAbilityMeta" )]
        private HealRadiusOvertimeAbilityMeta       healRadiusAbilityMeta = null;

        [SerializeField, FormerlySerializedAs( "CollisionComponent" ), HideInInspector]
        private CapsuleCollisionComponent           collisionComponent = null;

        [SerializeField, FormerlySerializedAs( "HealRadius" )]
        private float                               healRadius  = 50.0f;

        private HealRadiusOvertimeAbilityInstance   healRadiusAbilityInstance = null;


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var oldColour = Gizmos.color;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere( transform.position, healRadius );

            Gizmos.color = oldColour;
        }
#endif
    }
}

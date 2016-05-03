using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nixin
{
    [CreateAssetMenu( fileName = "HealRadiusOvertimeAbility", menuName = "Abilities/Heal Radius Overtime", order = 0 )]
    public class HealRadiusOvertimeAbilityMeta : AreaOfEffectAbilityMeta
    {


        // Public:


        public HealRadiusOvertimeAbilityInstance CreateInstance( Actor owner, Actor instigator, NixinName id, 
            float areaOfEffectMultiplier, Actor target, Vector3 location )
        {
            return new HealRadiusOvertimeAbilityInstance( owner, instigator, id, target, location, 
                areaOfEffectMultiplier * BaseAreaOfEffect, healAbility, this );
        }


        public override AbilityTargetType TargetType
        {
            get
            {
                return AbilityTargetType.Location;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "HealAbility" )]
        private HealTimeIntervalAbilityMeta healAbility = null;
    }


    public class HealRadiusOvertimeAbilityInstance : AreaOfEffectAbilityInstance
    {


        // Public:


        public HealRadiusOvertimeAbilityInstance( Actor owner, Actor instigator, NixinName id, Actor target, 
            Vector3 location, float radius, HealTimeIntervalAbilityMeta healAbility, AbilityMeta meta ) 
                    : base( owner, instigator, id, target, location, radius, meta )
        {
            this.healAbilityMeta = healAbility;

            UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
            UpdateComponent.UpdateRate      = 0.0f;

            sphere          = new GameObject();
            sphere.layer    = LayerDefs.QueryAbilityTargetablePos;
            sphereCollision = sphere.AddComponent<SphereCollisionComponent>();
            sphereCollision.ForwardToParent = false;

            var collider        = sphere.AddComponent<SphereCollider>();
            collider.isTrigger  = true;
            collider.radius     = Radius;

            sphereCollision.SphereCollider           = collider;

            sphere.transform.position = owner.transform.position;

            sphereCollision.OnTriggerEnterEvent.AddHandler( OnObjectEnterSphere );
            sphereCollision.OnTriggerExitEvent.AddHandler( OnObjectLeaveSphere );
        }


        public override void Uninitialise()
        {
            base.Uninitialise();

            sphereCollision.OnTriggerEnterEvent.RemoveHandler( OnObjectEnterSphere );
            sphereCollision.OnTriggerExitEvent.RemoveHandler( OnObjectLeaveSphere );
            GameObject.Destroy( sphere );
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );
        }


        // Private:


        private HealTimeIntervalAbilityMeta healAbilityMeta = null;

        private GameObject                  sphere              = null;
        private SphereCollisionComponent    sphereCollision     = null;


        private void OnObjectEnterSphere( CollisionComponent self, Collider other )
        {
            var actor = other.GetComponent<Actor>();
            if( actor == null )
            {
                return;
            }

            var health = actor.GetStat( StatDefs.currentHealthName.Id, true );
            if( health == null )
            {
                return;
            }

            var abilityComponent = actor.GetNixinComponent<AbilityTargetComponent>();
            if( abilityComponent == null )
            {
                return;
            }

            abilityComponent.AddUnique( healAbilityMeta.CreateInstance( actor, Instigator, AbilityDefs.fountainRegen, 
                actor, Vector3.zero, StatModifierTypeDefs.defaultStatModifierType ) );
        }


        private void OnObjectLeaveSphere( CollisionComponent self, Collider other )
        {
            var actor = other.GetComponent<Actor>();
            if( actor == null )
            {
                return;
            }

            var abilityComponent = actor.GetNixinComponent<AbilityTargetComponent>();
            if( abilityComponent == null )
            {
                return;
            }
            abilityComponent.RemoveAbility( AbilityDefs.fountainRegen );
        }
    }
}

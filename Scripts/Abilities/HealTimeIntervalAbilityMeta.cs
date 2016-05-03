using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Nixin
{
    [CreateAssetMenu( fileName = "HealTimeIntervalAbility", menuName = "Abilities/Heal Time Interval", order = 1 )]
    public class HealTimeIntervalAbilityMeta : TargetAbilityMeta
    {


        // Public:


        public HealTimeIntervalAbilityInstance CreateInstance( Actor owner, Actor instigator, NixinName id,
            Actor target, Vector3 location, StatModifierType modifierType )
        {
            return new HealTimeIntervalAbilityInstance( owner, instigator, id, target, location, healInterval,
                healPerInterval, modifierType, this );
        }


        public override float BaseAreaOfEffect
        {
            get
            {
                return -1.0f;
            }
        }


        public override AbilityTargetType TargetType
        {
            get
            {
                return AbilityTargetType.Actor;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "HealInterval" )]
        private float healInterval    = 1.0f;

        [SerializeField, FormerlySerializedAs( "HealPerInterval" )]
        private float healPerInterval = 1.0f;

    }


    public class HealTimeIntervalAbilityInstance : TargetAbilityInstance
    {
        


        // Public:


        public HealTimeIntervalAbilityInstance( Actor owner, Actor instigator, NixinName id, Actor target,
            Vector3 location, float healInterval, float healPerInterval, StatModifierType modifierType,
            AbilityMeta meta ) : base( owner, instigator, id, target, location, meta )
        {
            Assert.IsTrue( Target != null, "Invalid target." );

            this.healInterval    = healInterval;
            this.healPerInterval = healPerInterval;
            this.modifierType    = modifierType;

            healTimerHandle      = ContainingWorld.TimerSystem.SetTimerHandle( this, OnHealInterval, -1, healInterval );
        }


        public override void Uninitialise()
        {
            ContainingWorld.TimerSystem.RemoveTimerHandle( healTimerHandle );
            base.Uninitialise();
        }


        // Private:


        private float               healInterval    = 1.0f;
        private float               healPerInterval = 1.0f;
        private StatModifierType    modifierType    = null;
        private TimerHandle         healTimerHandle = null;


        private void OnHealInterval()
        {
            var manager  = ContainingWorld.GameManager as HookGameMatchManager;
            if( manager == null )
            {
                return;
            }

            var health = Target.GetStat( StatDefs.currentHealthName.Id, true );
            if( health == null )
            {
                return;
            }

            var modifier = new StatModifier( healPerInterval, modifierType, true, false, true );
            manager.TryModifyStat( health, modifier, false, Instigator );
        }
    }
}

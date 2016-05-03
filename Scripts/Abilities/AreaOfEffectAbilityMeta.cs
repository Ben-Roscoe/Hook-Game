using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nixin
{
    public abstract class AreaOfEffectAbilityMeta : TargetAbilityMeta
    {


        // Public:


        public override float BaseAreaOfEffect
        {
            get
            {
                return baseAreaOfEffect;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "BaseAreaOfEffect" )]
        private float baseAreaOfEffect;
    }


    public class AreaOfEffectAbilityInstance : TargetAbilityInstance
    {


        // Public:


        public AreaOfEffectAbilityInstance( Actor owner, Actor instigator, NixinName id, Actor target, Vector3 location,
            float radius, AbilityMeta meta ) : base( owner, instigator, id, target, location, meta )
        {
            this.radius = radius;
        }


        public float Radius
        {
            get
            {
                return radius;
            }
        }


        // Private:


        private float radius = 0.0f;
    }
}

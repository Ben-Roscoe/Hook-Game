using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public abstract class TargetAbilityMeta : AbilityMeta
    {


        // Public:


        public override AbilityType Type
        {
            get
            {
                return AbilityType.Target;
            }
        }
    }


    public abstract class TargetAbilityInstance : AbilityInstance
    {


        // Public:


        public TargetAbilityInstance( Actor owner, Actor instigator, NixinName id, Actor target, Vector3 location,
            AbilityMeta meta ) : base( owner, instigator, id, meta )
        {
            this.target      = target;
            this.location    = location;
        }


        public Actor Target
        {
            get
            {
                return target;
            }
        }


        public Vector3 Location
        {
            get
            {
                return location;
            }
        }


        // Private:


        private Actor       target        = null;
        private Vector3     location      = Vector3.zero;
    }
}

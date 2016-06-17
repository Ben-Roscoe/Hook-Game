using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class DamageRadiusComponent : NixinComponent
    {


        // Public:


        public override void OnRegistered( Actor owner, byte id )
        {
            base.OnRegistered( owner, id );

            if( Owner != null && Owner.IsAuthority && healTimerHandle == null )
            {
                healTimerHandle = Owner.ContainingWorld.TimerSystem.SetTimerHandle( this, HealRadius, -1, interval );
            }

            healModifier = new StatModifier( -20.0f, new HookDamageType(), true, false, true );
        }


        // Private:


        [SerializeField]
        private float           healRadius  = 1.0f;

        [SerializeField]
        private float           amount      = 1.0f;

        [SerializeField]
        private float           interval    = 1.0f;

        private TimerHandle     healTimerHandle     = null;
        private StatModifier    healModifier        = null;


        private void HealRadius()
        {
            if( Owner == null || !Owner.IsAuthority || Owner.ContainingWorld == null )
            {
                return;
            }

            var matchManagaer = Owner.ContainingWorld.GameManager as HookGameMatchManager;
            if( matchManagaer == null )
            {
                return;
            }

            for( int i = 0; i < Owner.ContainingWorld.Actors.Count; ++i )
            {
                var damagable = Owner.ContainingWorld.Actors[i];
                if( damagable == null || Vector3.Distance( damagable.transform.position, transform.position ) > healRadius )
                {
                    continue;
                }

                var currentHealthStat = damagable.GetStat( StatDefs.currentHealthName.Id, false );
                if( currentHealthStat == null )
                {
                    continue;
                }

                matchManagaer.TryModifyStat( currentHealthStat, healModifier, false, Owner );
            }
        }


#if !NSHIPPING
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

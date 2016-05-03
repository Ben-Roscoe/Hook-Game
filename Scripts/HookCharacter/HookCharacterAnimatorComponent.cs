using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using UnityEngine;

namespace Nixin
{
    class HookCharacterAnimatorComponent : AnimatorComponent, IHookCharacterControllableActorAnimator
    {


        // Public:


        public override void OnRegistered( Actor owner, byte id )
        {
            base.OnRegistered( owner, id );

            hookCharacter               = Owner as HookCharacterControllableActor;
            if( hookCharacter != null )
            {
                lastCharacterPosition = hookCharacter.transform.position;
            }

            runSpeed = new AnimatorParameter( "WalkSpeed", UnityAnitmatorComponent );
            throwTrigger = new AnimatorParameter( "ThrowHookTrigger", UnityAnitmatorComponent );
            deathTrigger = new AnimatorParameter( "DeathTrigger", UnityAnitmatorComponent );
            meleeTrigger = new AnimatorParameter( "MeleeTrigger", UnityAnitmatorComponent );

            if( Owner != null )
            {
                lastCharacterPosition = Owner.transform.position;
            }
            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
            }
        }


        public override void OnUnregistered()
        {
            base.OnUnregistered();

            hookCharacter = null;
        }


        public override void WriteSnapshot( NetBuffer buffer )
        {
            base.WriteSnapshot( buffer );

            buffer.Write( estimatedVelocityLength );
        }


        public override void ReadSnapshot( NetBuffer buffer, bool isFuture )
        {
            base.ReadSnapshot( buffer, isFuture );

            estimatedVelocityLength = buffer.ReadFloat( estimatedVelocityLength, isFuture );
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );

            if( UnityAnitmatorComponent == null || hookCharacter == null )
            {
                return;
            }

            if( Owner != null && Owner.IsAuthority )
            {
                estimatedVelocityLength = Vector3.Distance( hookCharacter.transform.position, lastCharacterPosition ) / deltaTime;
                lastCharacterPosition = hookCharacter.transform.position;
            }

            // Don't play running animation if the character can't run.
            if( hookCharacter.IsMovementFree )
            {
                runSpeed.SetFloat( estimatedVelocityLength );
            }
            else
            {
                runSpeed.SetFloat( 0.0f );
            }
            
            if( !hookCharacter.IsAuthority )
            {
                hasDied.Load( hookCharacter.IsDying );
                hasThrown.Load( hookCharacter.IsFiring );
                hasStartedMelee.Load( hookCharacter.IsPerformingMelee );

                if( hasDied.Fire() )
                {
                    deathTrigger.SetTrigger();
                }
                if( hasThrown.Fire() )
                {
                    throwTrigger.SetTrigger();
                }
                if( hasStartedMelee.Fire() )
                {
                    meleeTrigger.SetTrigger();
                }
            }
        }


        public void OnDeath( PostDeathCallback postDeathCallback )
        {
            this.postDeathCallback = postDeathCallback;

            deathTrigger.SetTrigger();

            // Set running speed to 0.
            runSpeed.SetFloat( 0.0f );
        }


        public void OnStartThrowHook( ThrowHookCallback throwHookCallback )
        {
            if( Owner == null || !Owner.IsAuthority )
            {
                return;
            }
            this.throwHookCallback = throwHookCallback;

            throwTrigger.SetTrigger();
        }


        public void OnStartMelee( HitMeleeCallback meleeHitCallback )
        {
            if( Owner == null || !Owner.IsAuthority )
            {
                return;
            }
            this.meleeHitCallback = meleeHitCallback;

            meleeTrigger.SetTrigger();
        }


        public void HookCharacter_ThrowHook_AnimEvent()
        {
            if( throwHookCallback != null )
            {
                throwHookCallback();
            }
        }


        public void HookCharacter_PostDeath_AnimEvent()
        {
            if( postDeathCallback != null )
            {
                postDeathCallback();
            }
        }


        public void HookCharacter_Melee_AnimEvent()
        {
            if( meleeHitCallback != null )
            {
                meleeHitCallback();
            }
        }


        // Private:


        private HookCharacterControllableActor          hookCharacter     = null;

        private PostDeathCallback                       postDeathCallback = null;
        private ThrowHookCallback                       throwHookCallback = null;
        private HitMeleeCallback                        meleeHitCallback  = null;

        private AnimatorParameter                       runSpeed          = null;
        private AnimatorParameter                       throwTrigger      = null;
        private AnimatorParameter                       deathTrigger      = null;
        private AnimatorParameter                       meleeTrigger      = null;

        private Trigger                                 hasDied           = new Trigger();
        private Trigger                                 hasThrown         = new Trigger();
        private Trigger                                 hasStartedMelee   = new Trigger();

        private Vector3                                 lastCharacterPosition   = Vector3.zero;
        private float                                   estimatedVelocityLength = 0.0f;
    }
}

using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Nixin
{
    public class HookCharacterControllableActor : ControllableActor, IPullable, IFogClearer
    {


        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

          //  movementComponent           = OverrideDefaultComponent<MovementComponent>( this, "MovementComponent", movementComponent, movementComponent );
            collisionComponent          = ConstructDefaultComponent<CapsuleCollisionComponent>( this, 
                "HookCapsuleCollisionComponent", collisionComponent );
            navigationAgentComponent    = ConstructDefaultComponent<NavigationAgentComponent>( this, 
                "NavigationAgentComponent", navigationAgentComponent );
            hudMessageSpawnComponent    = ConstructDefaultComponent<HudMessageSpawnerComponent>( this, 
                "HudMessageSpawnerComponent", hudMessageSpawnComponent );
            selectableComponent         = ConstructDefaultComponent<HookCharacterSelectableComponent>( this, 
                "SelectableComponent", selectableComponent );
            rigidbodyComponent          = ConstructDefaultComponent<Rigidbody>( this, 
                "RigidbodyComponent", rigidbodyComponent );
            abilityTargetComponent      = ConstructDefaultComponent<AbilityTargetComponent>( this, 
                "AbilityTargetComponent", abilityTargetComponent );
        }


        public override void OnPossess( Controller possesser )
        {
            base.OnPossess( possesser );

            if( !IsAuthority )
            {
                return;
            }

            var matchPlayer = possesser as HookGameMatchPlayer;
            if( matchPlayer == null || matchPlayer.Stats == null )
            {
                return;
            }

            var matchPlayerStats = matchPlayer.Stats as HookGameMatchPlayerStats;
            if( matchPlayerStats == null )
            {
                return;
            }

            if( possesser != null )
            {
                var maxHealth           = possesser.Stats.GetStat( StatDefs.healthName.Id, true );
                CurrentHealth.StatMax   = maxHealth;
                currentHealth.BaseValue = maxHealth.ModifiedValue;
            }
        }


        public override void OnUnpossess()
        {
            base.OnUnpossess();

            CurrentHealth.StatMax   = null;
        }


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            currentHealth = AddStat( StatDefs.currentHealthName );

            // Register RPCs.
            RegisterRPC<HookActor>( MulticastSetHook );
            RegisterRPC( MulticastSetHookUserAndHandle );
            RegisterRPC<float, bool, Actor>( MulticastHealthModified );

            if( IsAuthority )
            {
                SetHookType( hookPrefab );
            }

            // Get the animator if there is one.
            animator        = GetComponent<IHookCharacterControllableActorAnimator>();

            // Rigidbody used for falling.
            rigidbodyComponent.isKinematic = true;
            rigidbodyComponent.constraints = RigidbodyConstraints.FreezeRotation;// | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                        
            if( !IsAuthority )
            {
                navigationAgentComponent.Disable();
            }
            else
            {
                navigationAgentComponent.StaticDestinationStoppingDistance  = staticDestinationStoppingDistance;
                navigationAgentComponent.TargetStoppingDistance             = targetStoppingDistance;
            }

            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.UpdateAndFixed;
                UpdateComponent.UpdateRate = 0.0f;
            }

            if( IsAuthority )
            {
                ColourArmour();
            }
            SpawnRespawnEffect();
        }


        public override void OnActorInitialisePostSnapshot()
        {
            base.OnActorInitialisePostSnapshot();

            if( !IsAuthority )
            {
                ColourArmour();
            }
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );

            if( IsAuthority )
            {
                if( IsFiring )
                {
                    FaceHookFirePoint( fireAtPosition );
                }
            }
            if( !IsPerformingMelee && !IsCoolingDown && navigationAgentComponent != null && navigationAgentComponent.Target != null )
            {
                var damagable = navigationAgentComponent.Target;
                var manager   = ContainingWorld.GameManager as HookGameMatchManager;

                if( damagable != null && manager != null && 
                    Vector3.Distance( damagable.transform.position, transform.position ) < meleeRange
                    && damagable.IsStatModifiable( damagable.GetStat( StatDefs.currentHealthName.Id, false ), this ) 
                    && manager.CouldDoAnyDamage( damagable, this ) )
                {
                    FlagMelee();
                    navigationAgentComponent.Pause();
                    
                    if( animator != null )
                    {
                        animator.OnStartMelee( () => { OnMeleeHit( damagable ); } );
                    }
                    else
                    {
                        OnMeleeHit( damagable );
                    }
                }
            }
        }


        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            if( IsAuthority )
            {
                if( IsFalling )
                {
                    UpdateFalling();
                }
            }
        }


        public override void OnActorDestroy()
        {
            base.OnActorDestroy();

            if( attachedHook != null )
            {
                attachedHook.DetachPullable();   
            }
            if( hook != null )
            {
                ContainingWorld.DestroyActor( hook );
            }
        }


        public override void WriteSnapshot( NetBuffer buffer )
        {
            base.WriteSnapshot( buffer );

            buffer.Write( repActionFlags );
        }


        public override void ReadSnapshot( NetBuffer buffer, bool isFuture )
        {
            base.ReadSnapshot( buffer, isFuture );

            repActionFlags      = buffer.ReadByte( repActionFlags, isFuture );
        }


        public override bool IsStatModifiable( Stat stat, Actor instigator )
        {
            // Can't be hurt if we're dead.
            if( stat == CurrentHealth && IsDying )
            {
                return false;
            }
            return base.IsStatModifiable( stat, instigator );
        }


        public override bool CanModifyStat( Stat stat, StatModifier modifier, Actor instigator )
        {
            if( stat == currentHealth )
            {
                return CanModifyHealth( stat, modifier, instigator );
            }
            return base.CanModifyStat( stat, modifier, instigator );
        }


        public override void PostModifyStat( Stat stat, StatModifier modifer, bool baseModifier, Actor instigator )
        {
            base.PostModifyStat( stat, modifer, baseModifier, instigator );

            if( stat == currentHealth )
            {
                PostModifyHealth( stat, modifer, instigator );
            }
        }


        public void SetHookType( HookActor newHookPrefab )
        {
            if( !IsAuthority )
            {
                return;
            }

            if( hook != null )
            {
                ContainingWorld.DestroyActor( hook );
            }
            if( newHookPrefab == null )
            {
                return;
            }

            // No handle. We don't know what to attach the hook to.
            Assert.IsTrue( hookHandle != null );

            // Create the hook relevant to every connected player and every future connected player.
            hook                    = ( HookActor )ContainingWorld.InstantiateReplicatedActor( newHookPrefab, Vector3.zero, Quaternion.identity, NetworkOwner, true, ResponsibleController );

            latestSetHook           = ContainingWorld.NetworkSystem.BufferRPCCall( ContainingWorld.RPC( MulticastSetHook, RPCType.Multicast, this, hook ), latestSetHook );

            MulticastSetHookUserAndHandle();
            latestSetHookHandle     = ContainingWorld.NetworkSystem.BufferRPCCall( ContainingWorld.RPC( MulticastSetHookUserAndHandle, RPCType.Multicast, this ), latestSetHookHandle );
        }


        public bool StartGrapple()
        {
            if( !IsAuthority )
            {
                return false;
            }

            FlagGrappling();
            navigationAgentComponent.Disable();
            return true;
        }


        public void EndGrapple()
        {
            if( !IsAuthority )
            {
                return;
            }

            RemoveFlag( HookCharacterActions.Grappling );
            FlagFalling();
            rigidbodyComponent.isKinematic = false;
        }


        public bool Attach( HookActor hook )
        {
            if( IsDying )
            {
                return false;
            }

            // We've been attached by two hooks, headshot.
            if( IsAttachedToHook )
            {
                // Make sure this game manager allows head shots.
                var matchManager = ContainingWorld.GameManager as HookGameMatchManager;
                if( matchManager != null && matchManager.CanHeadshot )
                {
                    Die( hook, new StatModifier( -99999.0f, new StatModifierType(), true, false, false ) );
                    return false;
                }
            }

            attachedHook = hook;
            FlagPulled();
            navigationAgentComponent.Disable();
            return true;
        }


        public void Detach( HookActor hook )
        {
            attachedHook = null;
            RemoveFlag( HookCharacterActions.Pulled );
            FlagFalling();
            rigidbodyComponent.isKinematic = false;
        }


        public void FireHook( Vector3 at )
        {
            if( IsAuthority && hook != null )
            {
                FlagFiring();
                fireAtPosition          = at;
                FaceHookFirePoint( at );

                if( animator != null )
                {
                    animator.OnStartThrowHook( () => { OnThrowHook( at ); } );
                }
                else
                {
                    OnThrowHook( at );
                }
            }
        }


        public void NavigateToDestination( Vector3 destination )
        {
            navigationAgentComponent.MoveToDestination( destination );
        }


        public void NavigateToActor( Actor actor )
        {
            navigationAgentComponent.FollowActor( actor, false );
        }


        public Vector3 AttachOffset
        {
            get
            {
                return transform.position - HookAttachPosition.position;
            }
        }


        public NavigationAgentComponent Nav
        {
            get
            {
                return navigationAgentComponent;
            }
        }


        public HudMessageSpawnerComponent HudMessageSpawnerComponent
        {
            get
            {
                return hudMessageSpawnComponent;
            }
        }


        public HookHandleActor HookHandle
        {
            get
            {
                return hookHandle;
            }
        }


        // Pullable move check.
        public bool CanMove
        {
            get
            {
                return IsMovementFree;
            }
        }


        // Damagable damage check.
        public bool CanBeDamaged
        {
            get
            {
                // Can't be hit if we're dead.
                return !IsDying;
            }
        }


        public bool IsMovementFree
        {
            get
            {
                return ( repActionFlags & HookCharacterActions.Dying ) == 0
                        && ( repActionFlags & HookCharacterActions.Grappling ) == 0
                        && ( repActionFlags & HookCharacterActions.Pulled ) == 0
                        && ( repActionFlags & HookCharacterActions.Falling ) == 0;
            }
        }


        public bool CanFireHook
        {
            get
            {
                return hook != null && hook.IsReadyToFire && ( repActionFlags & HookCharacterActions.Dying ) == 0
                       && ( repActionFlags & HookCharacterActions.Firing ) == 0 && ( repActionFlags & HookCharacterActions.Falling ) == 0;
            }
        }


        public bool IsAttachedToHook
        {
            get
            {
                return attachedHook != null;
            }
        }


        public float AttachMinimum
        {
            get
            {
                return attachMinimum;
            }
        }


        public bool IsDying
        {
            get
            {
                return ( repActionFlags & HookCharacterActions.Dying ) != 0;
            }
        }


        public bool IsFiring
        {
            get
            {
                return ( repActionFlags & HookCharacterActions.Firing ) != 0;
            }
        }


        public bool IsFalling
        {
            get
            {
                return ( repActionFlags & HookCharacterActions.Falling ) != 0;
            }
        }


        public bool IsPerformingMelee
        {
            get
            {
                return ( repActionFlags & HookCharacterActions.Melee ) != 0;
            }
        }


        private bool IsCoolingDown
        {
            get
            {
                return isCoolingDown;
            }
        }


        public Stat CurrentHealth
        {
            get
            {
                return currentHealth;
            }
        }


        // Protected:


        [SerializeField, HideInInspector]
        protected CapsuleCollisionComponent     collisionComponent;

        [SerializeField, HideInInspector]
        protected NavigationAgentComponent      navigationAgentComponent;

        [SerializeField, HideInInspector]
        protected HudMessageSpawnerComponent    hudMessageSpawnComponent        = null;

        [SerializeField, FormerlySerializedAs( "AbilityTargetComponent" ), HideInInspector]
        protected AbilityTargetComponent        abilityTargetComponent          = null;

        [SerializeField, FormerlySerializedAs( "SelectableComponent" ), HideInInspector]
        protected HookCharacterSelectableComponent  selectableComponent             = null;

        [SerializeField, FormerlySerializedAs( "RigidbodyComponent" ), HideInInspector]
        protected Rigidbody                         rigidbodyComponent              = null;

        protected HookActor                     hook;


        // Private:


        private class HookCharacterActions
        {
            public const byte            Firing      = 1;
            public const byte            Dying       = 2;
            public const byte            Grappling   = 4;
            public const byte            Pulled      = 8;
            public const byte            Melee       = 16;
            public const byte            Falling     = 32;
        }

        [SerializeField]
        private Transform                       eyePosition;

        [SerializeField]
        private Transform                       centerPosition;

        [SerializeField]
        private HookActor                       hookPrefab;

        [SerializeField]
        private HookHandleActor                 hookHandle;

        [SerializeField]
        private PlayOnceParticleActor           bloodSplatterPrefab;

        [SerializeField]
        private HookCharacterRespawnEffectActor respawnEffectPrefab;

        [SerializeField]
        private Transform                       HookAttachPosition;

        [SerializeField]
        private float                           staticDestinationStoppingDistance   = 0.0f;

        [SerializeField]
        private float                           targetStoppingDistance              = 0.0f;

        [SerializeField]
        private float                           meleeRange                          = 0.0f;

        [SerializeField]
        private float                           attachMinimum                       = 0.0f;

        [SerializeField]
        private float                           groundedHeight                      = 0.4f;

        [SerializeField]
        private ArmourActor                     armour                              = null;

        private HookActor                       attachedHook            = null;

        private byte                            repActionFlags          = 0;

        // Our current statistics relevant to this character.
        private Stat                            currentHealth           = null;    

        // Our animator interface. We will notify this of one time events such as throwing our hook that
        // require a callback.
        private IHookCharacterControllableActorAnimator     animator       = null;

        private Vector3                         fireAtPosition             = Vector3.zero;


        // Melee stuff.
        private bool                            isCoolingDown              = false;


        [SerializeField]
        private Material blueBack = null;

        [SerializeField]
        private Material blueHelm = null;

        [SerializeField]
        private Material blueGauntlet = null;

        [SerializeField]
        private Material blueShoulder = null;

        [SerializeField]
        private Material blueLeg = null;

        [SerializeField]
        private Material blueCuirass = null;

        [SerializeField]
        private Material blueWing = null;


        [SerializeField]
        private Material redBack = null;
                         
        [SerializeField] 
        private Material redHelm = null;
                         
        [SerializeField] 
        private Material redGauntlet = null;
                         
        [SerializeField] 
        private Material redShoulder = null;
                         
        [SerializeField] 
        private Material redLeg = null;
                         
        [SerializeField] 
        private Material redCuirass = null;
                         
        [SerializeField]
        private Material redWing = null;


        private void Die( Actor instigator, StatModifier modifier )
        {
            FlagDying();
            collisionComponent.Disable();
            navigationAgentComponent.Disable();
            rigidbodyComponent.isKinematic = true;
            rigidbodyComponent.useGravity  = false;

            if( hook != null )
            {
                hook.ForceEnd();
            }

            // No animator, just call this ourselves.
            if( animator == null )
            {
                OnPostDeath();
            }
            else
            {
                // Pass the method to our animator, it should call it back when the death animation is complete.
                animator.OnDeath( OnPostDeath );
            }

            // Inform the game manager of the death.
            var manager = ContainingWorld.GameManager as HookGameMatchManager;
            if( manager == null )
            {
                return;
            }
            manager.OnActorKilled( this, instigator, modifier );
        }


        private bool CanModifyHealth( Stat healthStat, StatModifier modifier, Actor instigator )
        {
            return !IsDying;
        }


        private void PostModifyHealth( Stat healthStat, StatModifier modifier, Actor instigator )
        {
            if( !IsAuthority )
            {
                return;
            }

            if( currentHealth.ModifiedValue <= 0.0f )
            {
                Die( instigator, modifier );
            }

            if( !modifier.ShouldShowOnScreen )
            {
                return;
            }

            var damageType = modifier.ModifierType as HookGameMatchDamageType;
            if( !ContainingWorld.IsDedicatedServer )
            {
                MulticastHealthModified( modifier.Value, damageType == null ? false : damageType.ShouldCauseBlood(), instigator );
            }
            ContainingWorld.RPC( MulticastHealthModified, RPCType.Multicast, this, modifier.Value, damageType == null ? false : damageType.ShouldCauseBlood(), instigator );
        }


        private void FaceHookFirePoint( Vector3 at )
        {
            var direction = ( at - transform.position ).normalized;
            transform.forward = new Vector3( direction.x, transform.forward.y, direction.z );
        }


        private void UpdateFalling()
        {
            Assert.IsTrue( IsFalling );

            if( IsOnGround() )
            {
                RemoveFlag( HookCharacterActions.Falling );
                rigidbodyComponent.isKinematic = true;
                navigationAgentComponent.Enable();
            }
        }


        private bool IsOnGround()
        {
            // Sphere cast from the center of the capsule to the bottom + grounded height. The sphere will have half
            // the radius of the capsule to allow the capsule to slide down slopes without being considered grounded.
            var center          =  collisionComponent.GetCollider().bounds.center;
            var distance        = groundedHeight + collisionComponent.GetCollider().bounds.extents.y * 0.95f;
            var ray             = new Ray( center, Vector3.down );

            return Physics.SphereCast( ray, ( ( CapsuleCollider )collisionComponent.GetCollider() ).radius * 0.5f, distance );
        }


        private void OnThrowHook( Vector3 at )
        {
            hook.transform.parent = null;
            hook.FireHook( at );
            RemoveFlag( HookCharacterActions.Firing );
        }


        private void OnMeleeHit( Actor target )
        {
            if( target == null )
            {
                return;
            }

            // Try to cause damage.
            var matchManager        = ContainingWorld.GameManager as HookGameMatchManager;
            var targetHealth        = target.GetStat( StatDefs.currentHealthName.Id, false );
            if( matchManager != null && targetHealth != null )
            {
                var modifier = new StatModifier( -5.0f, new MeleeDamageType(), true, false, true );
                matchManager.TryModifyStat( targetHealth, modifier, false, this );
            }

            RemoveFlag( HookCharacterActions.Melee );
            navigationAgentComponent.Resume();

            // Cool down.
            StartCooldown();
            ContainingWorld.TimerSystem.SetTimerHandle( this, EndCooldown, 0, 1.5f );
        }


        private void ColourArmour()
        {
            if( armour == null )
            {
                NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Gameplay, "ColourArmour could not find armour actor" );
                return;
            }

            var stats = ResponsibleStats as TeamDeathmatchPlayerStats;
            if( stats == null )
            {
                NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Gameplay, 
                    "ColourArmour could not find stats for player." );
                return;
            }

            var team = stats.TeamType;

            if( team == TeamType.Blue )
            {
                armour.SetArmourMaterial( blueBack, blueHelm, blueGauntlet, blueShoulder, blueLeg, blueCuirass, blueWing );
            }
            else
            {
                armour.SetArmourMaterial( redBack, redHelm, redGauntlet, redShoulder, redLeg, redCuirass, redWing );
            }
        }


        private void SpawnRespawnEffect()
        {
            if( respawnEffectPrefab == null || centerPosition == null )
            {
                return;
            }

            var effect = ContainingWorld.InstantiateLocalActor( respawnEffectPrefab, null );

            effect.transform.position = centerPosition.position;
            effect.transform.parent   = transform;
        }


        private void OnPostDeath()
        {

        }


        private void StartCooldown()
        {
            isCoolingDown = true;
        }


        private void EndCooldown()
        {
            isCoolingDown = false;
        }


        private void FlagDying()
        {
            int flags = repActionFlags;

            // Can't be firing while dying.
            flags |= HookCharacterActions.Dying;
            flags &= ~HookCharacterActions.Firing;

            repActionFlags = ( byte )flags;
        }


        private void FlagFiring()
        {
            Assert.IsFalse( ( repActionFlags & HookCharacterActions.Dying ) != 0
                 || ( repActionFlags & HookCharacterActions.Grappling ) != 0
                 || ( repActionFlags & HookCharacterActions.Falling ) != 0 );

            int flags = repActionFlags;
            flags |= HookCharacterActions.Firing;
            repActionFlags = ( byte )flags;
        }


        private void FlagGrappling()
        {
            Assert.IsFalse( ( repActionFlags & HookCharacterActions.Firing ) != 0
                || ( repActionFlags & HookCharacterActions.Dying ) != 0
                || ( repActionFlags & HookCharacterActions.Falling ) != 0 );

            int flags = repActionFlags;
            flags |= HookCharacterActions.Grappling;
            repActionFlags = ( byte )flags;
        }


        private void FlagPulled()
        {
            Assert.IsFalse( ( repActionFlags & HookCharacterActions.Dying ) != 0 );

            int flags = repActionFlags;
            flags |= HookCharacterActions.Pulled;
            repActionFlags = ( byte )flags;
        }


        private void FlagMelee()
        {
            Assert.IsFalse( ( repActionFlags & HookCharacterActions.Dying ) != 0
                            || ( repActionFlags & HookCharacterActions.Falling ) != 0 );

            int flags = repActionFlags;
            flags |= HookCharacterActions.Melee;
            repActionFlags = ( byte )flags;
        }


        private void FlagFalling()
        {
            Assert.IsFalse( ( repActionFlags & HookCharacterActions.Dying ) != 0
                            && ( repActionFlags & HookCharacterActions.Firing ) != 0
                            && ( repActionFlags & HookCharacterActions.Melee ) != 0
                            && ( repActionFlags & HookCharacterActions.Pulled ) != 0
                            && ( repActionFlags & HookCharacterActions.Grappling ) != 0 );
            int flags = repActionFlags;
            flags |= HookCharacterActions.Falling;
            repActionFlags = ( byte )flags;
        }


        private void RemoveFlag( int flag )
        {
            int flags = repActionFlags;
            flags &= ~flag;
            repActionFlags = ( byte )flags;
        }


        // RPCs:


        private RPCCall     latestSetHookHandle     = null;
        private RPCCall     latestSetHook           = null;


        private void MulticastSetHook( HookActor hook )
        {
            this.hook = hook;
            if( hook != null )
            {
                hook.SetUser( this, hookHandle, true );
            }
        }


        private void MulticastSetHookUserAndHandle()
        {
            if( hook == null )
            {
                return;
            }
            hook.SetUser( this, hookHandle, true );
        }


        private void MulticastHealthModified( float amount, bool shouldSpawnBlood, Actor instigator )
        {
            if( amount < 0.0f )
            {
                // Damage.
                HudMessageSpawnerComponent.DisplayHudMessage( Mathf.Abs( amount ).ToString( "#." ), HudMessageType.Negative );
            }
            else if( amount > 0.0f )
            {
                // Heal.
                HudMessageSpawnerComponent.DisplayHudMessage( amount.ToString( "#." ), HudMessageType.Positive );
            }
            else
            {
                // Nothing.
                HudMessageSpawnerComponent.DisplayHudMessage( "0", HudMessageType.Passive );
            }

            if( shouldSpawnBlood && bloodSplatterPrefab != null )
            {
                // Spawn the blood at our attach position, and make it go backwards from the direction we were attacked.
                Vector3             position    = transform.position - AttachOffset;
                Quaternion          rotation    = Quaternion.LookRotation( instigator == null ? transform.forward : -( transform.position - instigator.transform.position ).normalized );

                ContainingWorld.InstantiateLocalActor( bloodSplatterPrefab, position, rotation, null );
            }
        }


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var oldColour = Gizmos.color;

            // Target stopping distance.
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere( transform.position, targetStoppingDistance );

            // Melee range.
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere( transform.position, meleeRange );

            // Attach minimum.
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere( transform.position, attachMinimum );

            // Grounded check length.
            var feet =  collisionComponent.GetCollider().bounds.center;
            feet.y -= collisionComponent.GetCollider().bounds.extents.y * 0.95f;
            Gizmos.DrawLine( feet, feet + ( Vector3.down * groundedHeight ) );

            Gizmos.color = oldColour;
        }
#endif
    }


    public enum HookCharacterDeathType
    {
        OutOfHealth,
        Headshot,
    }
}

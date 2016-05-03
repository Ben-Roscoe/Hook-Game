using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nixin
{
    public class HookActor : Actor
    {


        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

            rigidbodyComponent                  = ConstructDefaultComponent<Rigidbody>( this, "RigidbodyComponent", rigidbodyComponent );
            staticMeshRendererComponent         = ConstructDefaultStaticComponent<StaticMeshRendererComponent>( this, "StaticMeshRendererComponent", staticMeshRendererComponent );
            boxCollisionComponent               = ConstructDefaultComponent<BoxCollisionComponent>( this, "BoxCollisionComponent", boxCollisionComponent );
        }


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            linkPool = new LocalActorPool<HookLinkActor>( ContainingWorld, linkPrefab );

            interpolatedPosition.From   = transform.position;
            interpolatedPosition.To     = transform.position;
            interpolatedRotation.From   = transform.rotation;
            interpolatedRotation.To     = transform.rotation;

            // Register our rpc methods.
            RegisterRPC( MulticastFireHook );
            RegisterRPC( MulticastEndFireHook );

            if( IsAuthority )
            {
                boxCollisionComponent.OnCollisionEnterEvent.AddHandler( OnBoxColliderCollision );
                boxCollisionComponent.Disable();
            }
            else
            {
                // Don't need these client-side.
                boxCollisionComponent.Disable();
                rigidbodyComponent.detectCollisions = false;
            }

            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
                UpdateComponent.UpdateRate = 0.0f;
            }

            // Kinematic movement but a rigidbody is needed for collision.
            rigidbodyComponent.constraints = RigidbodyConstraints.FreezeAll;
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );

            if( !IsAuthority )
            {
                if( state == HookState.Firing || state == HookState.Retracting )
                {
                    // Get our interpolated transform.
                    transform.position = interpolatedPosition.GetWorldNetworkInterpolatedVector( ContainingWorld );
                    transform.rotation = interpolatedRotation.GetWorldNetworkInterpolatedQuaternion( ContainingWorld );
                }
                else
                {
                    // Make sure we always update these to our current transform, so that when the hook is fired again
                    // we interpolate from our current position rather than the last time we finished firing.
                    interpolatedPosition.From   = transform.position;
                    interpolatedPosition.To     = transform.position;
                    interpolatedRotation.From   = transform.rotation;
                    interpolatedRotation.To     = transform.rotation;
                }
            }

            if( state == HookState.Firing )
            {
                if( IsAuthority && firstFrameFired )
                {
                    // Reverse any movement done from parent since fired.
                    transform.position      = firedPosition;
                    transform.rotation      = firedRotation;
                    firstFrameFired         = false;
                }
                UpdateFiring( deltaTime );
            }
            else if( state == HookState.Retracting )
            {
                UpdateRetracting( deltaTime );
            }
            else if( state == HookState.Grappling )
            {
                UpdateGrappling( deltaTime );
            }
            else if( state == HookState.Idle )
            {
                UpdateIdle( deltaTime );
            }

            if( handle != null )
            {
                lastHandlePosition = handle.transform.position;
            }

            if( state != HookState.Idle )
            {
                ModifyLinkFromHandle();
            }
        }


        public override void OnActorDestroy()
        {
            base.OnActorDestroy();

            DestroyAllLinks();
            DetachPullable();
            
            if( linkPool != null )
            {
                linkPool.ClearPool( true );
                linkPool = null;
            }
        }


        public void DetachPullable()
        {
            if( pullable == null )
            {
                return;
            }
            pullable.Detach( this );
            pullable = null;
        }


        public override void WriteSnapshot( NetBuffer buffer )
        {
            base.WriteSnapshot( buffer );

            buffer.Write( ( byte )state );
            if( state == HookState.Firing || state == HookState.Retracting )
            {
                buffer.Write( Position.x );
                buffer.Write( Position.y );
                buffer.Write( Position.z );

                buffer.Write( Rotation.x );
                buffer.Write( Rotation.y );
                buffer.Write( Rotation.z );
                buffer.Write( Rotation.w );
            }
        }


        public override void ReadSnapshot( NetBuffer buffer, bool isFuture )
        {
            base.ReadSnapshot( buffer, isFuture );

            var tempState = ( HookState )buffer.ReadByte();
            if( tempState == HookState.Firing || tempState == HookState.Retracting )
            {
                if( !isFuture )
                {
                    interpolatedPosition.From   = new Vector3( buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat() );
                    interpolatedRotation.From   = new Quaternion( buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat() );
                }
                else
                {
                    interpolatedPosition.To = new Vector3( buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat() );
                    interpolatedRotation.To = new Quaternion( buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat() );
                }
            }
            if( !isFuture )
            {
                state = tempState;
            }
        }


        public virtual void ForceEnd()
        {
            if( !IsAuthority || state == HookState.Idle )
            {
                return;
            }
            DestroyAllLinks();
            ReturnToHandle();
        }


        public virtual void FireHook( Vector3 at )
        {
            if( !IsAuthority || state != HookState.Idle )
            {
                return;
            }

            var direction                   = ( at - transform.position ).normalized;
            var flatDirection               = new Vector3( direction.x, 0.0f, direction.z );
            transform.forward               = flatDirection;

            state                           = HookState.Firing;
            MulticastFireHook();
            lastStateChangeCall             = ContainingWorld.NetworkSystem.BufferRPCCall( ContainingWorld.RPC( MulticastFireHook, RPCType.Multicast, this ), lastStateChangeCall );

            fireDistanceTraveled            = 0.0f;
            lastPosition                    = transform.position;

            boxCollisionComponent.Enable();

            firstFrameFired                 = true;
            firedPosition                   = transform.position;
            firedRotation                   = transform.rotation;
        }


        public void SetUser( HookCharacterControllableActor user, HookHandleActor handle, bool resetTransform = true )
        {
            this.user = user;
            this.handle = handle;
            transform.parent = handle.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            if( handle != null )
            {
                lastHandlePosition = handle.transform.position;
            }
        }


        public float DistanceMultiplier
        {
            get
            {
                return distanceMultiplier;
            }
            set
            {
                distanceMultiplier = value;
            }
        }


        public HookType HookType
        {
            get
            {
                return hookType;
            }
            set
            {
                hookType = value;
            }
        }


        public HookCharacterControllableActor User
        {
            get
            {
                return user;
            }
        }


        public bool IsReadyToFire
        {
            get
            {
                return state == HookState.Idle;
            }
        }


        public bool IsGrappling
        {
            get
            {
                return state == HookState.Grappling;
            }
        }


        public bool IsReturning
        {
            get
            {
                return state == HookState.Grappling || state == HookState.Retracting;
            }
        }


        public Controller Instigator
        {
            get
            {
                if( user == null )
                {
                    return null;
                }
                return user.Instigator;
            }
        }


        public Transform CharacterAttachPosition
        {
            get
            {
                return characterAttachPosition;
            }
        }


        // Protected:


        protected virtual void StartRetracting()
        {
            state               = HookState.Retracting;
        }


        protected virtual void AttachPullable( IPullable pullable )
        {
            // If the pullable is too close, don't actually move it otherwise it'll look
            // weird.
            if( IsTooCloseToAttach( pullable ) )
            {
                physicallyAttach = false;
            }
            else
            {
                physicallyAttach = true;
            }

            this.pullable                   = pullable;
            initialAttachOffset             = ( characterAttachPosition.position + pullable.AttachOffset ) - pullable.transform.position;
            initialAttachRotation           = Quaternion.Inverse( transform.rotation ) * pullable.transform.rotation;
            pullableAttachCurrentSmoothTime = 0.0f;
            StartRetracting();
        }


        protected virtual bool IsTooCloseToAttach( IPullable pullable )
        {
            return links.Count * linkPrefab.DistanceFromNextLink <= pullable.AttachMinimum;
        }


        protected virtual void StartGrapple()
        {
            state = HookState.Grappling;
        }


        protected virtual void ReflectOffCollision( Collision collision )
        {
            if( !IsAuthority || collision.contacts.Length <= 0 )
            {
                return;
            }

            // By default we reflect off generic collisions.
            Vector3 averagedNormal = Vector3.zero;
            for( int i = 0; i < collision.contacts.Length; ++i )
            {
                averagedNormal += collision.contacts[i].normal;
            }
            averagedNormal /= collision.contacts.Length;

            // Constrain to a plane.
            averagedNormal.y = 0.0f;

            transform.forward = Vector3.Reflect( transform.forward, averagedNormal );

			if( sparkActorPrefab != null )
			{
				ContainingWorld.InstantiateLocalActor( sparkActorPrefab, collision.contacts[0].point, 
				                                       Quaternion.LookRotation( collision.contacts[0].normal ), null );
			}
        }


        protected virtual void OnBoxColliderCollision( CollisionComponent collisionComponent, Collision collision )
        {
            if( !IsAuthority || collision.gameObject == null || state == HookState.Idle )
            {
                return;
            }

            // Reflect off non-actors.
            Actor actor     = collision.gameObject.GetComponent<Actor>();
            if( actor == null )
            {
                ReflectOffCollision( collision );
                return;
            }

            // No self collisions.
            if( actor == User )
            {
                return;
            }


            // Try to pull this actor towards us.
            IPullable       pullableActor   = actor as IPullable;
            if( pullableActor != null )
            {
                if( ( state == HookState.Firing || state == HookState.Retracting ) && pullable == null && pullableActor.Attach( this ) )
                {
                    AttachPullable( pullableActor );

                    // Try to cause damage.
                    var matchManager = ContainingWorld.GameManager as HookGameMatchManager;
                    if( matchManager != null )
                    {
                        var currentHealth = actor.GetStat( StatDefs.currentHealthName.Id, false );
                        if( actor != null && currentHealth != null )
                        {
                            matchManager.TryModifyStat( currentHealth, new StatModifier( -30.0f, new HookDamageType(), true, false, true ), false, this );
                        }
                    }
                }
                return;
            }

            // Try to grapple to the actor.
            IGrapplable     grapplable = actor as IGrapplable;
            if( grapplable != null )
            {
                if( state == HookState.Firing && grapplable.StartGrapple( this ) && User != null )
                {
                    User.StartGrapple();
                    StartGrapple();
                    return;
                }
            }

            // Only reflect going forward.
            if( state == HookState.Firing )
            {
                ReflectOffCollision( collision );
            }
        }


        protected void ReturnToHandle()
        {
            if( !IsAuthority )
            {
                return;
            }

            if( User != null && state == HookState.Grappling )
            {
                User.EndGrapple();
            }
            DetachPullable();

            boxCollisionComponent.Disable();

            state = HookState.Idle;
            MulticastEndFireHook();
            lastStateChangeCall = ContainingWorld.NetworkSystem.BufferRPCCall( ContainingWorld.RPC( MulticastEndFireHook, RPCType.Multicast, this ), lastStateChangeCall );
        }


        // Private:


        [SerializeField, HideInInspector]
        private Rigidbody                           rigidbodyComponent;

        [SerializeField, HideInInspector]
        private StaticMeshRendererComponent         staticMeshRendererComponent;

        [SerializeField, HideInInspector]
        private BoxCollisionComponent               boxCollisionComponent;

        [SerializeField]
        private Transform                           characterAttachPosition         = null;

        [SerializeField]
        private float                               baseDistance;

        [SerializeField]
        private float                               baseSpeed;

        [SerializeField]
        private HookType                            hookType;

        [SerializeField]
        private HookLinkActor                       linkPrefab;

		// Particle Stuff
		[SerializeField]
		private SparkCollisionActor					sparkActorPrefab 				= null;

        private LocalActorPool<HookLinkActor>       linkPool                        = null;
        private List<HookLinkActor>                 links                           = new List<HookLinkActor>();
        private HookHandleActor                     handle;
        public 	HookState                           state                           = HookState.Idle;
        private float                               fireDistanceTraveled            = 0.0f;

        private float                               distanceMultiplier              = 1.0f;
        private float                               speedMultiplier                 = 1.0f;

        private Vector3                             lastPosition                    = Vector3.zero;
        private Vector3                             lastHandlePosition              = Vector3.zero;

        private NetInterpolatedVector               interpolatedPosition            = new NetInterpolatedVector();
        private NetInterpolatedQuaternion           interpolatedRotation            = new NetInterpolatedQuaternion();

        private HookCharacterControllableActor      user                            = null;

        // The pullable we are currently pulling.
        [SerializeField, FormerlySerializedAs( "PullableAttachSmoothTime" )]
        private float                               pullableAttachSmoothTime        = 1.0f;
        private IPullable                           pullable                        = null;
        private bool                                physicallyAttach                = true;
        private Vector3                             initialAttachOffset             = Vector3.zero;
        private Quaternion                          initialAttachRotation           = Quaternion.identity;
        private float                               pullableAttachCurrentSmoothTime = 0.0f;

        private RPCCall                             lastStateChangeCall             = null;

        // Unity doesn't seem to detach a child until the end of the frame. This throws off the hook's
        // trajectory.
        private bool                                firstFrameFired                 = false;
        private Vector3                             firedPosition                   = Vector3.zero;
        private Quaternion                          firedRotation                   = Quaternion.identity;

        [SerializeField, FormerlySerializedAs( "ReturnLerpTime" )]
        private float                               returnLerpTotalTime             = 0.2f;

        private Vector3                             returnLerpStartPosition         = Vector3.zero;
        private Quaternion                          returnLerpStartRotation         = Quaternion.identity;
        private float                               returnLerpCurrentTime           = 0.0f;


        private void AddLinkToFront()
        {
            // When we push a link, place it 1 length behind the last so we can fill the gap of links.
            var last = GetLinkTransform( links.Count - 1 );
            var direction = ( last.position - handle.transform.position ).normalized;
            var lookRotation    = direction.sqrMagnitude > 0.0f ? Quaternion.LookRotation( direction ) : Quaternion.identity;
            HookLinkActor link = linkPool.GetOrInstantiate( last.position - ( direction * linkPrefab.DistanceFromNextLink ), lookRotation, ResponsibleController );
            links.Add( link );
        }


        private void AddLinkToEnd()
        {
            var last            = GetLinkTransform( links.Count - 1 );
            var direction       = ( transform.position - last.position ).normalized;
            var lookRotation    = direction.sqrMagnitude > 0.0f ? Quaternion.LookRotation( direction ) : Quaternion.identity;
            HookLinkActor link = linkPool.GetOrInstantiate( last.position - ( direction * linkPrefab.DistanceFromNextLink ), lookRotation, ResponsibleController );
            links.Add( link );
        }


        private void RemoveLinkFromFront()
        {
            ContainingWorld.DestroyActor( links[links.Count - 1] );
            links.RemoveAt( links.Count - 1 );
        }


        private void RemoveLinkFromEnd()
        {
            ContainingWorld.DestroyActor( links[0] );
            links.RemoveAt( 0 );
        }


        private void UpdateFiring( float deltaTime )
        {
            if( IsAuthority )
            {
                fireDistanceTraveled += Vector3.Distance( lastPosition, transform.position );
                lastPosition = transform.position;

                if( fireDistanceTraveled >= baseDistance * distanceMultiplier )
                {
                    StartRetracting();
                }
            }


            if( IsAuthority )
            {
                transform.Translate( transform.forward * baseSpeed * speedMultiplier * deltaTime, Space.World );
            }

            for( int i = 0; i < links.Count; ++i )
            {
                var current         = GetLinkTransform( i );
                var next            = GetLinkTransform( i - 1 );

                current.forward = ( next.position - current.position ).normalized;
                current.Translate( current.forward * baseSpeed * speedMultiplier * deltaTime, Space.World );
            }
            CullLinks();
        }


        private void UpdateGrappling( float deltaTime )
        {
            for( int i = 0; i < links.Count; ++i )
            {
                var current = GetLinkTransform( i );
                var next = GetLinkTransform( i - 1 );
                var last = GetLinkTransform( i + 1 );

                if( Vector3.Distance( last.position, next.position ) < linkPrefab.DistanceFromNextLink )
                {
                    ContainingWorld.DestroyActor( links[i] );
                    links.RemoveAt( i );
                    --i;
                    continue;
                }

                current.forward = ( next.position - current.position ).normalized;
                current.Translate( current.forward * baseSpeed * speedMultiplier * deltaTime, Space.World );
            }
            if( IsAuthority )
            {
                var current                 = links.Count > 0 ? GetLinkTransform( links.Count - 1 ) : transform;
                var directionToCurrent      = ( current.position - User.HookHandle.transform.position ).normalized;

                User.transform.forward = new Vector3( directionToCurrent.x, User.transform.forward.y, directionToCurrent.z );
                directionToCurrent = ( current.position - User.HookHandle.transform.position ).normalized;

                User.transform.Translate( directionToCurrent * Mathf.Min( baseSpeed * speedMultiplier * deltaTime, Vector3.Distance( current.position, User.HookHandle.transform.position ) ), Space.World );
            }

            ModifyLinkFromHook();

            if( state == HookState.Idle )
            {
                User.EndGrapple();
            }
        }


        private void UpdateRetracting( float deltaTime )
        {
            var closestToHandlePosition     = GetLinkTransform( links.Count - 1 ).position;

            if( IsAuthority )
            {
                var linkAfterHook = GetLinkTransform( 0 );
                var between       = transform.position - linkAfterHook.position;
                
                if( between != Vector3.zero )
                {
                    transform.forward = between.normalized;
                }
                transform.Translate( -transform.forward * Mathf.Min( ( baseSpeed * speedMultiplier ) * deltaTime, Vector3.Distance( transform.position, linkAfterHook.position ) ), Space.World );

                if( pullable != null )
                {
                    if( physicallyAttach )
                    {
                        pullableAttachCurrentSmoothTime += deltaTime;
                        var t = pullableAttachCurrentSmoothTime / pullableAttachSmoothTime;
                        pullable.transform.position = ( characterAttachPosition.position + pullable.AttachOffset ) - Vector3.Lerp( initialAttachOffset, Vector3.zero, t );
                        pullable.transform.rotation = transform.rotation * initialAttachRotation;
                    }

                    if( IsTooCloseToAttach( pullable ) )
                    {
                        DetachPullable();
                    }
                }
            }

            for( int i =  links.Count - 1; i >= 0; --i )
            {
                var current             = GetLinkTransform( i );
                var next                = GetLinkTransform( i - 1 );
                var last                = GetLinkTransform( i + 1 );
                var between             = ( next.position - current.position );

                if( between != Vector3.zero )
                {
                    current.transform.forward = between.normalized;
                }

                var   direction                 = ( last.position - current.position ).normalized;
                current.transform.Translate( direction * ( baseSpeed * speedMultiplier ) * deltaTime, Space.World );
            }
            CullLinks();
        }


        private void UpdateIdle( float deltaTime )
        {
            if( returnLerpCurrentTime < returnLerpTotalTime )
            {
                // Lerp the hook back to it's parent's orientation smoothly.
                returnLerpCurrentTime += deltaTime;
                float t = returnLerpCurrentTime / returnLerpTotalTime;

                transform.localPosition = Vector3.Lerp( returnLerpStartPosition, Vector3.zero, t );
                transform.localRotation = Quaternion.Lerp( returnLerpStartRotation, Quaternion.identity, t );
            }
        }


        private void CullLinks()
        {
            for( int i = 0; i < links.Count; ++i )
            {
                var next            = GetLinkTransform( i - 1 );
                var last            = GetLinkTransform( i + 1 );
                if( Vector3.Distance( last.position, next.position ) < linkPrefab.DistanceFromNextLink )
                {
                    ContainingWorld.DestroyActor( links[i] );
                    links.RemoveAt( i );
                    --i;
                    continue;
                }
            }
        }


        private void ModifyLinkFromHandle()
        {
            // Check for extension.
            var back = GetLinkTransform( links.Count - 1 );
            while( IsInAddingDistance( handle.transform.position, back ) )
            {
                AddLinkToFront();
                back = GetLinkTransform( links.Count - 1 );
            }


            // Check for retraction.
            var link = GetLinkTransform( links.Count - 2 );
            while( IsInDeletionDistance( handle.transform.position, link ) )
            {
                if( link != transform )
                {
                    RemoveLinkFromFront();
                }
                else if( IsReturning )
                {
                    ReturnToHandle();
                }
                break;//link = GetLinkTransform( links.Count - 2 );
            }
        }


        private void ModifyLinkFromHook()
        {
            // Check for extension.
            var back = GetLinkTransform( 0 );
            while( IsInAddingDistance( transform.position, back ) )
            {
                AddLinkToFront();
                back = GetLinkTransform( links.Count - 1 );
                break;
            }


            // Check for retraction.
            var link = GetLinkTransform( 1 );
            while( IsInDeletionDistance( transform.position, link ) )
            {
                if( link != handle.transform )
                {
                    RemoveLinkFromFront();
                }
                else if( IsReturning )
                {
                    ReturnToHandle();
                }
                break;//link = GetLinkTransform( links.Count - 2 );
            }
        }


        private bool IsInAddingDistance( Vector3 relativeTo, Transform link )
        {
            return Vector3.Distance( link.position, relativeTo ) >= linkPrefab.DistanceFromNextLink;
        }

    
        private bool IsInDeletionDistance( Vector3 relativeTo, Transform link )
        {
            return Vector3.Distance( link.position, relativeTo ) < linkPrefab.DistanceFromNextLink;
        }


        private Transform GetLinkTransform( int i )
        {
            if( i < 0 )
            {
                return transform;
            }
            if( i >= links.Count )
            {
                return handle.transform;
            }
            return links[i].transform;
        }


        private void DestroyAllLinks()
        {
            foreach( var link in links )
            {
                if( link == null )
                {
                    continue;
                }
                ContainingWorld.DestroyActor( link );
            }
            links.Clear();
        }


        // RPCs:


        private void MulticastFireHook()
        {
            transform.parent = null;

            if( handle != null )
            {
                handle.AudiosSourceComponent.SourceComponent.loop = true;
                handle.AudiosSourceComponent.SourceComponent.clip = linkPrefab.MovingSound;
                handle.AudiosSourceComponent.SourceComponent.Play();
            }
        }


        private void MulticastEndFireHook()
        {
            transform.parent            = handle.transform;
            returnLerpStartPosition     = transform.localPosition;
            returnLerpStartRotation     = transform.localRotation;
            returnLerpCurrentTime       = 0.0f;

            if( handle != null )
            {
                handle.AudiosSourceComponent.SourceComponent.Stop();
            }


            // Remove any links that may not have been destroyed( small desyncs ).
            DestroyAllLinks();
        }
    }

    public enum HookState
    {
        Idle            = 0,
        Firing          = 1,
        Retracting      = 2,
        Grappling       = 3,
    }
    public enum HookType
    {
        Pull,
        Grapple,
    }
}

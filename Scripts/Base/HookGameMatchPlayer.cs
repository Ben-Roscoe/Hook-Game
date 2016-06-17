using UnityEngine;
using System.Collections;
using System;
using Lidgren.Network;
using UnityEngine.Serialization;

namespace Nixin
{
    public class HookGameMatchPlayer : Player
    {


        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

            hookCharacterOwnerComponent = ConstructDefaultComponent<HookCharacterOwnerComponent>( this, 
                "HookCharacterOwnerComponent", hookCharacterOwnerComponent );
            selectorComponent           = ConstructDefaultComponent<HookCharacterOwnerSelectorComponent>( this, 
                "SelectorComponent", selectorComponent );
        }


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            RegisterRPC<Vector3>( ServerFireHook );
            RegisterRPC<Vector3>( ServerMoveToPosition );

            if( CameraActor != null )
            {
                matchPlayerCamera = CameraActor as MatchPlayerCameraActor;
            }

            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
                UpdateComponent.UpdateRate = 0.0f;
            }

            HookCharacterOwnerComponent.OnHookCharacterSpawned.AddHandler( OnHookCharacterSpawned );
        }


        public override void OnActorDestroy()
        {
            base.OnActorDestroy();
            HookCharacterOwnerComponent.OnHookCharacterSpawned.RemoveHandler( OnHookCharacterSpawned );
        }


        public override void Possess( ControllableActor controllableActor )
        {
            base.Possess( controllableActor );

            if( !IsAuthority || Stats == null )
            {
                return;
            }

            // Setup the controllable actor's current health.
            var currentHealth   = controllableActor.GetStat( StatDefs.currentHealthName.Id, false );
            var maxHealth       = Stats.GetStat( StatDefs.healthName.Id, false );
            if( currentHealth != null && maxHealth != null )
            {
                currentHealth.StatMax       = maxHealth;
                currentHealth.BaseValue     = maxHealth.ModifiedValue;
            }
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );
            
            // Allow the camera to scroll when the mouse is close enough to an edge of the screen.
            if( IsLocalPlayer && matchPlayerCamera != null )
            {
                var mousePositionNullable = InputComponent.MousePosition;
                if( mousePositionNullable.HasValue )
                {
                    Vector2 mouseDirection = new Vector2( 0.0f, 0.0f );
                    if( mousePositionNullable.Value.x < matchPlayerCamera.DistanceFromEdgeToScroll )
                    {
                        mouseDirection.x = -1.0f;
                    }
                    else if( mousePositionNullable.Value.x > matchPlayerCamera.CameraComponent.pixelWidth - 
                             matchPlayerCamera.DistanceFromEdgeToScroll )
                    {
                        mouseDirection.x = 1.0f;
                    }

                    if( mousePositionNullable.Value.y < matchPlayerCamera.DistanceFromEdgeToScroll )
                    {
                        mouseDirection.y = -1.0f;
                    }
                    else if( mousePositionNullable.Value.y > matchPlayerCamera.CameraComponent.pixelHeight -
                             matchPlayerCamera.DistanceFromEdgeToScroll )
                    {
                        mouseDirection.y = 1.0f;
                    }

                    matchPlayerCamera.Scroll( mouseDirection, deltaTime );
                    FindHover();
                }
            }
        }


        public override void OnPossessedControllableDeath( Actor killer, StatModifier modifier )
        {
            base.OnPossessedControllableDeath( killer, modifier );

            HookCharacterOwnerComponent.OnControllableActorDeath( killer, modifier );
        }


        public override void WriteSnapshot( NetBuffer buffer )
        {
            base.WriteSnapshot( buffer );

//#if NDEBUG
            buffer.Write( printInputTiming );
            buffer.Write( serverInputReadTime );
            printInputTiming = false;
//#endif
        }


        public override void ReadSnapshot( NetBuffer buffer, bool isFuture )
        {
            base.ReadSnapshot( buffer, isFuture );

//#if NDEBUG
            printInputTiming    = buffer.ReadBoolean();
            serverInputReadTime = buffer.ReadDouble( serverInputReadTime, isFuture );
            if( printInputTiming && isFuture )
            {
                NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Networking, Time.time + ": Click took: " + ( NetTime.Now - inputTime ) + ", Seen took: " + 
                    ( serverInputReadTime - inputTime ) + ", Get took: " + ( ContainingWorld.NetworkSystem.CurrentSnapshot.GetTime - inputTime ) );
            }
//#endif
        }


        public override Stat GetStat( int nameHash, bool includeChildren )
        {
            // Just make sure that things can get our current health and magic from here.
            var ret = base.GetStat( nameHash, includeChildren );
            if( ret != null || PossessedActor == null || !includeChildren )
            {
                return ret;
            }
            return PossessedActor.GetStat( nameHash, includeChildren );
        }


        public HookCharacterOwnerComponent HookCharacterOwnerComponent
        {
            get
            {
                return hookCharacterOwnerComponent;
            }
        }


        // Protected:


        protected override void SetupInputComponent( InputComponent inputComponent )
        {
            base.SetupInputComponent( inputComponent );

            inputComponent.BindAction( "MoveToPositionButton", InputState.Down, OnLeftMouseClick, this );
            inputComponent.BindAction( "FireHook", InputState.Down, OnFireHookPressed, this );
            inputComponent.BindAction( "FocusCameraOnHookCharacter", InputState.Down, FocusCameraOnHookCharacter, this );

            inputComponent.BindAxis( "CameraHorizontal", HorizontalButtonCameraAxis, this );
            inputComponent.BindAxis( "CameraVertical", VerticalButtonCameraAxis, this );
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "HookCharacterOwnerComponent" ), HideInInspector]
        private HookCharacterOwnerComponent                             hookCharacterOwnerComponent = null;

        [SerializeField, FormerlySerializedAs( "SelectorComponent" ), HideInInspector]
        private HookCharacterOwnerSelectorComponent                     selectorComponent       = null;

        [SerializeField]
        private PositionMarkerActor                                     positionMarkerPrefab    = null;

        private MatchPlayerCameraActor                                  matchPlayerCamera       = null;

        // Debug information for input delay.
//#if NIXIN_DEBUG
        private bool    printInputTiming        = false;
        private double  inputTime               = 0.0;
        private double  serverInputReadTime     = 0.0;
//#endif

        private void FocusCameraOnHookCharacter()
        {
            if( matchPlayerCamera != null && HookCharacterOwnerComponent.HookCharacter != null )
            {
                matchPlayerCamera.LerpToPosition( HookCharacterOwnerComponent.HookCharacter.transform.position );
            }
        }


        private void OnFireHookPressed()
        {
            if( CameraActor == null || PossessedActor == null )
            {
                return;
            }

            var mousePositionNullable = InputComponent.MousePosition;
            if( !mousePositionNullable.HasValue )
            {
                return;
            }

            var ray             = CameraActor.CameraComponent.ScreenPointToRay( mousePositionNullable.Value );
            var planeNormal     = PossessedActor.transform.up;
            var result          = Vector3.Dot( planeNormal, ray.direction );
            if( result == 0.0f )
            {
                return;
            }
            var distance        = Vector3.Dot( new Vector3( PossessedActor.transform.position.x, PossessedActor.transform.position.y + 0.5f, PossessedActor.transform.position.z ) - ray.origin, planeNormal ) / result;
            var hitPoint        = ray.origin + ( ray.direction * distance );

            // Firing hook deselects.
            ContainingWorld.RPC( ServerFireHook, RPCType.Server, this, hitPoint );
            selectorComponent.Select( null );
        }


        private void OnLeftMouseClick()
        {
            if( CameraActor == null )
            {
                return;
            }

//#if NIXIN_DEBUG
            inputTime         = NetTime.Now;
//#endif
            var ray           = CameraActor.CameraComponent.ScreenPointToRay( Input.mousePosition );
            FindSelection( ray.origin, ray.direction );
        }


        private void ServerFireHook( Vector3 at )
        {
            if( HookCharacterOwnerComponent.HookCharacter == null || 
                !HookCharacterOwnerComponent.HookCharacter.CanFireHook || !InputComponent.IsRemotelyEnabled )
            {
                return;
            }

            HookCharacterOwnerComponent.HookCharacter.FireHook( at );
        }


        private void ServerMoveToPosition( Vector3 pos )
        {
            if( hookCharacterOwnerComponent.HookCharacter == null || !InputComponent.IsRemotelyEnabled )
            {
                return;
            }
            hookCharacterOwnerComponent.HookCharacter.NavigateToDestination( pos );

            // DEBUG.
            printInputTiming = true;
            if( Relevancies.Count > 0 )
            {
                serverInputReadTime = Relevancies[0].Client.NetConnection.GetRemoteTime( NetTime.Now );
            }
        }


        private void OnHookCharacterSpawned( HookCharacterOwnerComponent characterOwner )
        {
            if( IsLocalPlayer && matchPlayerCamera != null && HookCharacterOwnerComponent.HookCharacter != null )
            {
                matchPlayerCamera.LerpToPosition( HookCharacterOwnerComponent.HookCharacter.transform.position );
            }
        }


        private void FindHover()
        {
            if( !InputComponent.IsEnabled || CameraActor == null )
            {
                return;
            }

            var mousePositionNullable = InputComponent.MousePosition;
            if( !mousePositionNullable.HasValue )
            {
                return;
            }

            // Look for selectables that have been hovered over.
            Ray        ray = CameraActor.CameraComponent.ScreenPointToRay( mousePositionNullable.Value );
            RaycastHit hit;
            if( Physics.Raycast( ray, out hit, Mathf.Infinity, LayerDefs.Selectable ) )
            {
                var gameObject = hit.collider == null ? null : hit.collider.gameObject;
                if( gameObject != null )
                {
                    var collisionComponent = gameObject.GetComponent<CollisionComponent>();
                    if( collisionComponent != null )
                    {
                        var selectable = collisionComponent.Receiver.Owner.GetNixinComponent<SelectableComponent>();
                        selectorComponent.HoverSelect( selectable );
                        return;
                    }
                }
            }
            selectorComponent.HoverSelect( null );
        }


        private void FindSelection( Vector3 worldPosition, Vector3 worldDirection )
        {
            // Hit either nav mesh or selectables.
            RaycastHit hit;
            if( !Physics.Raycast( new Ray( worldPosition, worldDirection ), out hit, Mathf.Infinity,
                LayerDefs.PotentialNavMesh | LayerDefs.Selectable ) )
            {
                return;
            }

            if( hit.collider.gameObject.layer == LayerDefs.PotentialNavMeshPos )
            {
                // Move the character to a location on the map.
                if( hookCharacterOwnerComponent.HookCharacter != null &&
                    hookCharacterOwnerComponent.HookCharacter.IsMovementFree )
                {
                    // Fuck unity's plane.
                    var a = ContainingWorld.InstantiateLocalActor( positionMarkerPrefab, hit.point, Quaternion.LookRotation( hit.normal ), null );
                    a.transform.Rotate( new Vector3( 90.0f, 0.0f, 0.0f ), Space.Self );

                    ContainingWorld.RPC( ServerMoveToPosition, RPCType.Server, this, hit.point );
                }
                selectorComponent.Select( null );
            }
            else
            {
                var collisionComponent = hit.collider.GetComponent<CollisionComponent>();
                if( collisionComponent != null )
                {
                    var receivingActor = collisionComponent.Receiver.Owner;
                    var selectable = receivingActor.GetNixinComponent<SelectableComponent>();
                    selectorComponent.Select( selectable );
                }
                else
                {
                    selectorComponent.Select( null );
                }
            }
        }


        private void HorizontalButtonCameraAxis( float v, float deltaTime )
        {
            if( IsLocalPlayer && matchPlayerCamera != null )
            {
                matchPlayerCamera.Scroll( new Vector2( v, 0 ), deltaTime );
            }
        }


        private void VerticalButtonCameraAxis( float v, float deltaTime )
        {
            if( IsLocalPlayer && matchPlayerCamera != null )
            {
                matchPlayerCamera.Scroll( new Vector2( 0, v ), deltaTime );
            }
        }
    }



}

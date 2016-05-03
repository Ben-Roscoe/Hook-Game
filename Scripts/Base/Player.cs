using UnityEngine;
using System.Collections;
using System;
using Lidgren.Network;

namespace Nixin
{
    public class Player : Controller
    {



        // Public:


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections,
            Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            RegisterRPC( MulticastEnableInput );
            RegisterRPC( MulticastDisableInput );

            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
                UpdateComponent.UpdateRate = 0.0f;
            }

            // Input component will exist on both server and client so the server can 
            // disable input remotely.
            inputComponent = new InputComponent( ContainingWorld );

            // If we are the local player, setup input and HUD.
            if( IsLocalPlayer )
            {
                SetupInputComponent( InputComponent );
                if( Camera.main != null )
                {
                    var defaultCamera = Camera.main.gameObject.GetComponent<CameraActor>();
                    if( defaultCamera != null )
                    {
                        SetCameraActor( defaultCamera );
                    }
                }
                CreateHud();
            }
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );
            if( IsLocalPlayer )
            {
                InputComponent.UpdateInput( deltaTime );
            }
        }


        public override void OnActorDestroy()
        {
            base.OnActorDestroy();

            if( IsLocalPlayer )
            {
                inputComponent.RemoveAllWithOwner( this );
                inputComponent.Uninitialise();
            }
        }


        public override void WriteSnapshot( NetBuffer buffer )
        {
            base.WriteSnapshot( buffer );

            buffer.WriteActor( Stats );
        }


        public override void ReadSnapshot( NetBuffer buffer, bool isFuture )
        {
            base.ReadSnapshot( buffer, isFuture );

            Stats = buffer.ReadActor<StatsBase>( ContainingWorld, Stats, isFuture );
        }


        public virtual void SetCameraActor( CameraActor newCameraActor )
        {
            if( cameraActor != null )
            {
                cameraActor.CameraComponent.enabled = false;
            }
            cameraActor         = newCameraActor;
            cameraActor.enabled = true;
        }


        public void EnableRemotePlayerInput()
        {
            if( IsAuthority )
            {
                MulticastEnableInput();
                if( ContainingWorld.NetworkSystem.IsServer )
                {
                    ContainingWorld.RPC( MulticastEnableInput, RPCType.Multicast, this );
                }
            }
        }


        public void DisableRemotePlayerInput()
        {
            if( IsAuthority )
            {
                MulticastDisableInput();
                if( ContainingWorld.NetworkSystem.IsServer )
                {
                    ContainingWorld.RPC( MulticastDisableInput, RPCType.Multicast, this );
                }
            }
        }


        public void EnableLocalPlayerInput()
        {
            if( IsLocalPlayer )
            {
                InputComponent.ToggleLocallyEnabled( true );
            }
        }


        public void DisableLocalPlayerInput()
        {
            if( IsLocalPlayer )
            {
                InputComponent.ToggleLocallyEnabled( false );
            }
        }


        public InputComponent InputComponent
        {
            get
            {
                return inputComponent;
            }
        }


        public CameraActor CameraActor
        {
            get
            {
                return cameraActor;
            }
        }


        public bool IsLocalPlayer
        {
            get
            {
                return ContainingWorld.NetworkSystem.Id == NetworkOwner;
            }
        }


        public HudBase Hud
        {
            get
            {
                return hud;
            }
        }


        public GameState GameState
        {
            get
            {
                if( ContainingWorld == null )
                {
                    return null;
                }
                return ContainingWorld.GameState;
            }
        }


        // Protected:


        protected virtual void SetupInputComponent( InputComponent inputComponent )
        {
            inputComponent.RemoveAllWithOwner( this );
        }


        // Private:


        private InputComponent      inputComponent              = null;
        private HudBase             hud                         = null;
        private CameraActor         cameraActor                 = null;


        private void CreateHud()
        {
            if( ContainingWorld != null && ContainingWorld.CurrentGameModeResource != null && ContainingWorld.CurrentGameModeResource.HudPrefab != null )
            {
                hud = ( HudBase )ContainingWorld.InstantiateUIActor( ContainingWorld.CurrentGameModeResource.HudPrefab, null );
                hud.SetUp( this );
            }
        }


        // Disable input client side as well, so local actions can't be performed.
        // Still validate server side.

        private void MulticastEnableInput()
        {
            InputComponent.ToggleRemotelyEnabled( true );
        }


        private void MulticastDisableInput()
        {
            InputComponent.ToggleRemotelyEnabled( false );
        }
    }


    [System.Serializable]
    public class PlayerWeakReference : WeakUnityReference<Player>
    {
    }
}
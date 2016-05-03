using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nixin
{
    public class HookCharacterOwnerComponent : NixinComponent
    {


        // Public:


        public override void OnOwningActorInitialised()
        {
            base.OnOwningActorInitialised();
            Owner.RegisterRPC<HookCharacterControllableActor>( MulticastOnHookCharacterSpawned );
            Owner.RegisterRPC( MulticastOnHookCharacterDespawn );
            
            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
                UpdateComponent.UpdateRate      = 0.0f;
            }
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );
            if( respawnTimerHandle != null )
            {
                repRespawnSeconds = ( byte )respawnTimerHandle.TimeRemaining;
            }
        }


        public void OnControllableActorDeath( Actor killer, StatModifier modifier )
        {
            if( ControllerOwner == null || !ControllerOwner.IsAuthority )
            {
                return;
            }

            var manager = ContainingWorld.GameManager as HookGameMatchManager;
            if( manager != null && manager.CanRespawn( ControllerOwner ) )
            {
                ContainingWorld.TimerSystem.RemoveTimerHandle( respawnTimerHandle );
                respawnTimerHandle = ContainingWorld.TimerSystem.SetTimerHandle( this, RespawnHookCharacter, 0, ( float )manager.GetRespawnTime( ControllerOwner ) );
                state = HookCharacterOwnerState.Respawning;
            }
        }


        public void RespawnHookCharacter()
        {
            if( ControllerOwner != null && !ControllerOwner.IsAuthority )
            {
                return;
            }

            respawnTimerHandle  = null;
            state               = HookCharacterOwnerState.Playing;

            // Get rid of dead body.
            DespawnHookCharacter();
            SpawnHookCharacter();
        }


        private void SpawnHookCharacter()
        {
            if( ControllerOwner == null || !ControllerOwner.IsAuthority )
            {
                return;
            }
            if( HookCharacter != null )
            {
                return;
            }

            var matchManager    = ContainingWorld.GameManager as HookGameMatchManager;
            var spawnPoint      = matchManager == null ? null : matchManager.GetSpawnPoint( ControllerOwner );

            hookCharacter       = ( HookCharacterControllableActor )ContainingWorld.InstantiateReplicatedActor( hookCharacterPrefab, spawnPoint == null ? Vector3.zero : spawnPoint.GetSpawnPosition()
                                                                                                              , spawnPoint == null ? Quaternion.identity : spawnPoint.GetSpawnRotation(), ControllerOwner.NetworkOwner, true, ControllerOwner );
            ControllerOwner.Possess( hookCharacter );

            MulticastOnHookCharacterSpawned( hookCharacter );
            ContainingWorld.RPC( MulticastOnHookCharacterSpawned, RPCType.Multicast, ControllerOwner, hookCharacter );
        }


        private void DespawnHookCharacter()
        {
            if( ControllerOwner == null || HookCharacter == null )
            {
                return;
            }

            ControllerOwner.Unpossess();
            ContainingWorld.DestroyActor( hookCharacter );
            hookCharacter = null;

            MulticastOnHookCharacterDespawn();
            ContainingWorld.RPC( MulticastOnHookCharacterDespawn, RPCType.Multicast, ControllerOwner );
        }


        public override void WriteSnapshot( NetBuffer buffer )
        {
            base.WriteSnapshot( buffer );
            buffer.Write( ( byte )state );
            buffer.Write( repRespawnSeconds );
        }


        public override void ReadSnapshot( NetBuffer buffer, bool isFuture )
        {
            base.ReadSnapshot( buffer, isFuture );
            state               = ( HookCharacterOwnerState )buffer.ReadByte( ( byte )state, isFuture );
            repRespawnSeconds   = buffer.ReadByte( repRespawnSeconds, isFuture );
        }


        public HookCharacterControllableActor HookCharacter
        {
            get
            {
                return hookCharacter;
            }
        }


        public Controller ControllerOwner
        {
            get
            {
                return Owner as Controller;
            }
        }


        public bool IsRespawning
        {
            get
            {
                return state == HookCharacterOwnerState.Respawning;
            }
        }


        public byte RemainingRespawnSeconds
        {
            get
            {
                if( !IsRespawning )
                {
                    return 0;
                }
                return repRespawnSeconds;
            }
        }


        public NixinEvent<HookCharacterOwnerComponent> OnHookCharacterSpawned
        {
            get
            {
                return onHookCharacterSpawned;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "HookCharacterPrefab" )]
        private HookCharacterControllableActor                          hookCharacterPrefab     = null;

        private HookCharacterControllableActor                          hookCharacter           = null;
        private TimerHandle                                             respawnTimerHandle      = null;
        private byte                                                    repRespawnSeconds       = 0;

        private HookCharacterOwnerState                                 state                    = HookCharacterOwnerState.WaitingForSpawn;

        private NixinEvent<HookCharacterOwnerComponent>                 onHookCharacterSpawned  = new NixinEvent<HookCharacterOwnerComponent>();


        private void MulticastOnHookCharacterSpawned( HookCharacterControllableActor hookCharacter )
        {
            this.hookCharacter  = hookCharacter;
            OnHookCharacterSpawned.Invoke( this );
        }


        private void MulticastOnHookCharacterDespawn()
        {
            this.hookCharacter  = null;
        }
    }

    public enum HookCharacterOwnerState
    {
        Respawning,
        Playing,
        WaitingForSpawn,
    }
}

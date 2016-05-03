using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Lidgren.Network;
using UnityEngine;

namespace Nixin
{
    public class NixinComponent : NixinBehaviour, IUpdatable
    {


        // Public


        public virtual void EditorComponentConstructor( Actor actor, string name )
        {

        }


        public virtual void RegisterReplicatedProperties()
        {

        }


        public virtual void OnRegistered( Actor owner, byte id )
        {
            componentId = id;
            this.owner  = owner;

            UpdateComponent.Initialise( this );
            UpdateComponent.IsEnabled = true;
        }


        public virtual void OnUnregistered()
        {
            UpdateComponent.IsEnabled = false;
        }


        public virtual void OnUpdate( float deltaTime )
        {

        }


        public virtual void OnFixedUpdate()
        {

        }


        public virtual void OnOwningActorInitialised()
        {

        }


        public virtual void OnOwningActorUpdate()
        {

        }


        public virtual void OnOwningActorFixedUpdate()
        {

        }


        public virtual  void WriteSnapshot( NetBuffer buffer )
        {

        }


        public virtual void ReadSnapshot( NetBuffer buffer, bool isFuture )
        {

        }


        public virtual void PreSendSnapshot( int networkStep )
        {
        }


        public virtual void PostSendSnapshot( int networkStep )
        {

        }


        public virtual void PreApplyInterpolatedSnapshot( int fromNetworkStep, int toNetworkStep, float t )
        {
        }


        public virtual void PostApplyInterpolatedSnapshot( int fromNetworkStep, int toNetworkStep, float t )
        {
        }


        public Actor Owner
        {
            get
            {
                return owner;
            }
            set
            {
                owner = value;
            }
        }


        public byte ComponentId
        {
            get
            {
                return componentId;
            }
        }


        public World ContainingWorld
        {
            get
            {
                if( Owner == null )
                {
                    return null;
                }
                return Owner.ContainingWorld;
            }
        }


        public UpdateComponent UpdateComponent
        {
            get
            {
                return updateComponent;
            }
        }


        // Private:


        [SerializeField]
        private bool            updateWithOwner                     = true;

        [SerializeField]
        private UpdateComponent updateComponent;

        private Actor           owner                               = null;
        private byte            componentId                         = 0;
    }
}

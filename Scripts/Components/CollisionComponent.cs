using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Nixin
{
    public abstract class CollisionComponent : NixinComponent 
    {


        // Public:


        public override void OnRegistered( Actor owner, byte id )
        {
            base.OnRegistered( owner, id );

            ForwardToParent = forwardToParent;
        }


        public void Enable()
        {
            GetCollider().enabled = true;
        }


        public void Disable()
        {
            GetCollider().enabled = false;
        }


        public NixinEvent<CollisionComponent, Collision> OnCollisionEnterEvent
        {
            get
            {
                return onCollisionEnterEvent;
            }
        }


        public NixinEvent<CollisionComponent, Collision> OnCollisionStayEvent
        {
            get
            {
                return onCollisionStayEvent;
            }
        }


        public NixinEvent<CollisionComponent, Collision> OnCollisionExitEvent
        {
            get
            {
                return onCollisionExitEvent;
            }
        }


        public NixinEvent<CollisionComponent, Collider> OnTriggerEnterEvent
        {
            get
            {
                return onTriggerEnterEvent;
            }
        }


        public NixinEvent<CollisionComponent, Collider> OnTriggerStayEvent
        {
            get
            {
                return onTriggerStayEvent;
            }
        }


        public NixinEvent<CollisionComponent, Collider> OnTriggerExitEvent
        {
            get
            {
                return onTriggerExitEvent;
            }
        }


        public CollisionComponent Receiver
        {
            get
            {
                return receiver;
            }
        }


        public bool ForwardToParent
        {
            get
            {
                return forwardToParent;
            }
            set
            {
                forwardToParent = value;
                if( forwardToParent )
                {
                    if( parentCollisionCompoent == null )
                    {
                        var parent = transform.parent;
                        if( parent != null )
                        {
                            parentCollisionCompoent = parent.GetComponent<CollisionComponent>();
                        }
                    }
                    receiver = parentCollisionCompoent == null ? this : parentCollisionCompoent;
                }
                else
                {
                    receiver = this;
                }
            }
        }


        public abstract Collider GetCollider();


        // Private:


        private NixinEvent<CollisionComponent, Collision> onCollisionEnterEvent  = new NixinEvent<CollisionComponent, Collision>();
        private NixinEvent<CollisionComponent, Collision> onCollisionStayEvent   = new NixinEvent<CollisionComponent, Collision>();
        private NixinEvent<CollisionComponent, Collision> onCollisionExitEvent   = new NixinEvent<CollisionComponent, Collision>();
                                                          
        private NixinEvent<CollisionComponent, Collider>  onTriggerEnterEvent    = new NixinEvent<CollisionComponent, Collider>();
        private NixinEvent<CollisionComponent, Collider>  onTriggerStayEvent     = new NixinEvent<CollisionComponent, Collider>();
        private NixinEvent<CollisionComponent, Collider>  onTriggerExitEvent     = new NixinEvent<CollisionComponent, Collider>();

        private CollisionComponent receiver                 = null;
        private CollisionComponent parentCollisionCompoent  = null;

        [SerializeField, FormerlySerializedAs( "ForwardToParent" )]
        private bool               forwardToParent          = false;
        

        private void OnCollisionEnter( Collision collision )
        {
            if( GetCollider() == null )
            {
                return;
            }

            receiver.OnCollisionEnterEvent.Invoke( this, collision );
        }


        private void OnCollisionStay( Collision collision )
        {
            if( GetCollider() == null )
            {
                return;
            }
            receiver.OnCollisionStayEvent.Invoke( this, collision );
        }


        private void OnCollisionExit( Collision collision )
        {
            if( GetCollider() == null )
            {
                return;
            }
            receiver.OnCollisionExitEvent.Invoke( this, collision );
        }


        private void OnTriggerEnter( Collider collider )
        {
            if( GetCollider() == null )
            {
                return;
            }
            receiver.OnTriggerEnterEvent.Invoke( this, collider );
        }


        private void OnTriggerStay( Collider collider )
        {
            if( GetCollider() == null )
            {
                return;
            }
            receiver.OnTriggerStayEvent.Invoke( this, collider );
        }


        private void OnTriggerExit( Collider collider )
        {
            if( GetCollider() == null )
            {
                return;
            }
            receiver.OnTriggerExitEvent.Invoke( this, collider );
        }
    }

    [System.Serializable]
    public class SubClassOfCollisionComponent : SubClassOf<CollisionComponent>
    {
    }
}
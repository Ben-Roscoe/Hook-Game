using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class NavigationAgentComponent : NixinComponent
    {


        // Public:


        public override void EditorComponentConstructor( Actor actor, string name )
        {
            base.EditorComponentConstructor( actor, name );

            agent = ConstructDefaultStaticComponent<NavMeshAgent>( actor, "NavMeshAgent", agent );
        }


        public override void OnRegistered( Actor owner, byte id )
        {
            base.OnRegistered( owner, id );

            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
            }
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );

            if( ( ( target != null && stopWhenTargetReached ) || staticDestination ) && !isPaused && !agent.pathPending && agent.hasPath && agent.remainingDistance <= agent.stoppingDistance )
            {
                CallbackWithReason( NavigationEndReason.Complete );
                StopAgent();
            }
            if( target != null && !isPaused )
            {
                if( !agent.SetDestination( target.transform.position ) )
                {
                    CallbackWithReason( NavigationEndReason.Blocked );
                }
            }
        }


        public void Enable()
        {
            if( !agent.enabled )
            {
                agent.enabled = true;
            }
        }


        public void Disable()
        {
            if( agent.enabled )
            {
                StopAgent();
                agent.enabled = false;
            }
        }


        public void MoveToDestination( Vector3 destination )
        {
            if( target != null || staticDestination )
            {
                CallbackWithReason( NavigationEndReason.Overwritten );
            }
            StopAgent();

            agent.Resume();
            staticDestination = true;
            if( !agent.SetDestination( destination ) )
            {
                CallbackWithReason( NavigationEndReason.Blocked );
                StopAgent();
            }

            agent.stoppingDistance = staticDestinationStoppingDistance;
        }


        public void FollowActor( Actor target, bool stopWhenTargetReached )
        {
            if( target != null || staticDestination )
            {
                CallbackWithReason( NavigationEndReason.Overwritten );
            }
            StopAgent();

            agent.Resume();
            this.target = target;
            if( !agent.SetDestination( target.transform.position ) )
            {
                CallbackWithReason( NavigationEndReason.Blocked );
                StopAgent();
            }

            agent.stoppingDistance      = targetStoppingDistance;
            this.stopWhenTargetReached  = stopWhenTargetReached;
        }


        public void Pause()
        {
            isPaused = true;
            agent.Stop();
        }


        public void Resume()
        {
            isPaused = false;
            agent.Resume();
            if( target != null && !agent.SetDestination( target.transform.position ) )
            {
                CallbackWithReason( NavigationEndReason.Blocked );
                StopAgent();
            }
        }


        public void Stop()
        {
            CallbackWithReason( NavigationEndReason.Canceled );
            StopAgent();
        }


        public NixinEvent<NavigationAgentComponent, NavigationEndReason> OnMovementFinished
        {
            get
            {
                return onMovementFinsihed;
            }
        }


        public NavMeshAgent Agent
        {
            get
            {
                return agent;
            }
        }


        public Actor Target
        {
            get
            {
                return target;
            }
        }


        public float StaticDestinationStoppingDistance
        {
            get
            {
                return staticDestinationStoppingDistance;
            }
            set
            {
                staticDestinationStoppingDistance = value;
            }
        }


        public float TargetStoppingDistance
        {
            get
            {
                return targetStoppingDistance;
            }
            set
            {
                targetStoppingDistance = value;
            }
        }


        public bool StopWhenTargetReached
        {
            get
            {
                return stopWhenTargetReached;
            }
            set
            {
                stopWhenTargetReached = value;
            }
        }


        // Private:


        [SerializeField, HideInInspector]
        private NavMeshAgent                                                      agent;

        private Actor                                                             target;
        private bool                                                              stopWhenTargetReached         = true;
        private bool                                                              isPaused;
        private bool                                                              staticDestination             = false;

        private float                                                             staticDestinationStoppingDistance = 0.0f;
        private float                                                             targetStoppingDistance            = 0.0f;

        private NixinEvent<NavigationAgentComponent, NavigationEndReason>      onMovementFinsihed = new NixinEvent<NavigationAgentComponent, NavigationEndReason>();


        private void CallbackWithReason( NavigationEndReason reason )
        {
            OnMovementFinished.Invoke( this, reason );
        }


        private void StopAgent()
        {
            target = null;
            staticDestination = false;
            agent.SetDestination( transform.position );
            agent.Stop();
        }
    }


    public enum NavigationEndReason
    {
        Complete,
        Overwritten,
        Blocked,
        Canceled,
    }
}

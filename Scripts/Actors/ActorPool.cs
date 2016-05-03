using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Assertions;

namespace Nixin
{
    public abstract class ActorPool<T> : IActorPool where T : Actor
    {


        // Public:


        public ActorPool( World world, T actorPrefab )
        {
            this.world          = world;
            this.actorPrefab    = actorPrefab;
        }


        public void ClearPool( bool destroyActors )
        {
            Assert.IsNotNull( world );

            for( int i = 0; i < allocatedActors.Count; ++i )
            {
                FreeActor( allocatedActors[i], destroyActors );
            }
            if( destroyActors )
            {
                for( int i = 0; i < freeActors.Count; ++i )
                {
                    freeActors[i].Pool = null;
                    world.DestroyActor( freeActors[i] );
                }
                freeActors.Clear();
            }
        }


        public void FreeActor( Actor actor, bool destroyActor )
        {
            Assert.IsNotNull( world );

            T actorSpecific = actor as T;
            if( !allocatedActors.Remove( actorSpecific ) )
            {
                return;
            }
            actor.OnPoolDeallocate();
            if( destroyActor )
            {
                actor.Pool = null;
                world.DestroyActor( actor );
            }
            else
            {
                freeActors.Add( actorSpecific );
            }
        }


        public World World
        {
            get
            {
                return world;
            }
        }


        public T ActorPrefab
        {
            get
            {
                return actorPrefab;
            }
        }


        public List<T> AllocatedActors
        {
            get
            {
                return allocatedActors;
            }
        }


        // Protected:


        protected bool AnyFreeActors()
        {
            return freeActors.Count > 0;
        }


        protected T AllocateFreeActor()
        {
            int index   = freeActors.Count - 1;
            T   actor   = freeActors[index];

            allocatedActors.Add( freeActors[index] );
            freeActors.RemoveAt( index );

            actor.OnPoolAllocate();
            return actor;
        }


        protected void AllocateNewActor( T actor )
        {
            actor.Pool = this;
            actor.OnPoolAllocate();
            allocatedActors.Add( actor );
        }


        // Private:


        private World           world           = null;
        private T               actorPrefab     = null;

        private List<T>         allocatedActors = new List<T>();
        private List<T>         freeActors      = new List<T>();
    }
}

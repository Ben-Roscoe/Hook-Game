using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nixin
{
    public class LocalActorPool<T> : ActorPool<T> where T : Actor
    {


        // Public:


        public LocalActorPool( World world, T actorPrefab ) : base( world, actorPrefab )
        {
        }


        public T GetOrInstantiate( Controller responsibleController )
        {
            return GetOrInstantiate( Vector3.zero, Quaternion.identity, responsibleController );
        }


        public T GetOrInstantiate( Vector3 position, Quaternion rotation, Controller responsibleController )
        {
            Assert.IsNotNull( World );

            T newActor = null;

            if( AnyFreeActors() )
            {
                newActor = AllocateFreeActor();
                newActor.transform.position = position;
                newActor.transform.rotation = rotation;
                return newActor;
            }

            newActor = ( T )World.InstantiateLocalActor( ActorPrefab, position, rotation, responsibleController );
            AllocateNewActor( newActor );
            return newActor;
        }
    }
}

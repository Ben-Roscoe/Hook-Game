using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nixin
{
    public class UIActorPool<T> : ActorPool<T> where T : UIActor
    {


        // Public:


        public UIActorPool( World world, T actorPrefab ) : base( world, actorPrefab )
        {
        }


        public T GetOrInstantiate( UICanvasActor canvas )
        {
            return GetOrInstantiate( canvas, Vector3.zero, Quaternion.identity );
        }


        public T GetOrInstantiate( UICanvasActor canvas, Vector3 position, Quaternion rotation )
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

            newActor = ( T )World.InstantiateUIActor( ActorPrefab, canvas, position, rotation );
            AllocateNewActor( newActor );
            return newActor;
        }

    }
}

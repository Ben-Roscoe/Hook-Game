using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class PlayOnceParticleActor : Actor
    {


        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

            particleSystemComponent = ConstructDefaultComponent<ParticleSystem>( this, "ParticleSystem", particleSystemComponent );
        }


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );
            ContainingWorld.TimerSystem.SetTimerHandle( this, EndLifeTime, 0, particleSystemComponent.duration );
        }


        // Private:


        [SerializeField, HideInInspector]
        private ParticleSystem particleSystemComponent = null;


        private void EndLifeTime()
        {
            ContainingWorld.DestroyActor( this );
        }
    }
}

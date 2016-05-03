using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class PositionMarkerActor : Actor
    {


        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

            positionMarkerAnimationComponent = ConstructDefaultComponent<FinishOnStateAnimatorComponent>( this, "PositionMarkerAnimationComponent", positionMarkerAnimationComponent );
        }


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            positionMarkerAnimationComponent.OnReachedFinishState.AddHandler( OnAnimatorCompleted );
        }


        // Private:


        [SerializeField, HideInInspector]
        private FinishOnStateAnimatorComponent               positionMarkerAnimationComponent;


        private void OnAnimatorCompleted( FinishOnStateAnimatorComponent animatorComponent )
        {
            ContainingWorld.DestroyActor( this );
        }
    }
}

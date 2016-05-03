using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class HookCharacterRespawnEffectActor : Actor
    {


        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

            animatorComponent = ConstructDefaultComponent<FinishOnStateAnimatorComponent>( this, "AnimatorComponent", animatorComponent );
        }


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            animatorComponent.OnReachedFinishState.AddHandler( OnAnimatorReachedFinishState );
        }


        // Private:


        [SerializeField, HideInInspector]
        private FinishOnStateAnimatorComponent  animatorComponent = null;


        private void OnAnimatorReachedFinishState( FinishOnStateAnimatorComponent sender )
        {
            ContainingWorld.DestroyActor( this );
        }
    }
}

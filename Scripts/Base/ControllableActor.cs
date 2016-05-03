using UnityEngine;
using System.Collections;
using System;

namespace Nixin
{
    public class ControllableActor : Actor
    {


        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

            movementComponent                = ConstructDefaultComponent<MovementComponent>( this, "MovementComponent", movementComponent );
        }


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            inputComponent = new InputComponent( ContainingWorld );
            SetupInput( inputComponent );
            inputComponent.ToggleLocallyEnabled( false );
        }


        public override void OnActorDestroy()
        {
            base.OnActorDestroy();
            inputComponent.Uninitialise();
        }


        public virtual void OnPossess( Controller possesser )
        {
            if( possesser != null )
            {
                OnUnpossess();
            }
            owner = possesser;

            // Enable input for player controllers.
            if( possesser != null && possesser is Player )
            {
                inputComponent.ToggleLocallyEnabled( true );
            }
        }


        public virtual void OnUnpossess()
        {
            owner = null;
            inputComponent.ToggleLocallyEnabled( false );
        }


        public Controller Instigator
        {
            get
            {
                return owner;
            }
        }


        // Protected:


        [SerializeField, HideInInspector]
        protected MovementComponent       movementComponent;


        protected virtual void SetupInput( InputComponent inputComponent )
        {

        }


        // Private:


        private Controller              owner               = null;
        private InputComponent          inputComponent      = null;
    }
}
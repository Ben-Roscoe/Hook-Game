using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class FinishOnStateAnimatorComponent : AnimatorComponent
    {


        // Public:


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

            if( UnityAnitmatorComponent != null && UnityAnitmatorComponent.GetCurrentAnimatorStateInfo( finsihedLayer ).IsName( finishedStateName ) )
            {
                onReachedFinishState.Invoke( this );
            }
        }


        public NixinEvent<FinishOnStateAnimatorComponent> OnReachedFinishState
        {
            get
            {
                return onReachedFinishState;
            }
        }


        // Private:


        [SerializeField]
        private int         finsihedLayer           = 0;

        [SerializeField]
        private string      finishedStateName       = "Finished";

        private NixinEvent<FinishOnStateAnimatorComponent> onReachedFinishState = new NixinEvent<FinishOnStateAnimatorComponent>();
    }
}

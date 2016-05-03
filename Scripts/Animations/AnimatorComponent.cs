using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    [RequireComponent( typeof( Animator ) )]
    public class AnimatorComponent : NixinComponent
    {


        // Public:


        public override void OnRegistered( Actor owner, byte id )
        {
            base.OnRegistered( owner, id );

            unityAnimatorComponent = GetComponent<Animator>();
        }


        public Animator UnityAnitmatorComponent
        {
            get
            {
                return unityAnimatorComponent;
            }
        }


        // Private:


        [SerializeField, HideInInspector]
        private Animator                        unityAnimatorComponent;
    }
}

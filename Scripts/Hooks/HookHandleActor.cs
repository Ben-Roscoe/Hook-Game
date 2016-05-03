using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class HookHandleActor : Actor
    {


        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

            audioSourceComponent = ConstructDefaultStaticComponent<AudioSourceComponent>( this, "AudioSourceComponent", audioSourceComponent );
        }


        public AudioSourceComponent AudiosSourceComponent
        {
            get
            {
                return audioSourceComponent;
            }
        }


        // Private:


        [SerializeField, HideInInspector]
        private AudioSourceComponent                audioSourceComponent;
    }
}

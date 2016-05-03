using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class AudioListenerComponent : NixinComponent
    {


        // Public:


        public override void EditorComponentConstructor( Actor actor, string name )
        {
            base.EditorComponentConstructor( actor, name );

            listenerComponent = ConstructDefaultStaticComponent<AudioListener>( actor, name + " : AudioListenerComponent", listenerComponent );
        }


        public AudioListener ListenerComponent
        {
            get
            {
                return listenerComponent;
            }
        }


        // Private:


        [SerializeField, HideInInspector]
        private AudioListener               listenerComponent;
    }
}

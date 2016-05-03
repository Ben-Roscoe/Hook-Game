using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class AudioSourceComponent : NixinComponent
    {


        // Public:


        public override void EditorComponentConstructor( Actor actor, string name )
        {
            base.EditorComponentConstructor( actor, name );

            sourceComponent = ConstructDefaultStaticComponent<AudioSource>( actor, name + " : AudioSourceComponent", sourceComponent );
        }


        public AudioSource SourceComponent
        {
            get
            {
                return sourceComponent;
            }
        }


        // Private:


        [SerializeField, HideInInspector]
        private AudioSource                 sourceComponent;
    }
}

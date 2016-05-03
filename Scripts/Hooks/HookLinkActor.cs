using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    class HookLinkActor : Actor
    {


        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

            staticMeshRendererComponent         = ConstructDefaultStaticComponent<StaticMeshRendererComponent>( this, "StaticMeshRendererComponent", staticMeshRendererComponent );
        }


        public float DistanceFromNextLink
        {
            get
            {
                return distanceFromNextLink;
            }
        }


        public AudioClip MovingSound
        {
            get
            {
                return movingSound;
            }
        }


        // Private:


        [SerializeField, HideInInspector]
        StaticMeshRendererComponent             staticMeshRendererComponent;

        [SerializeField]
        float                                   distanceFromNextLink;

        [SerializeField]
        AudioClip                               movingSound             = null;
    }
}

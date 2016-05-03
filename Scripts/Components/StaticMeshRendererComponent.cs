using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    class StaticMeshRendererComponent : NixinComponent
    {


        // Public:


        public override void EditorComponentConstructor( Actor actor, string name )
        {
            base.EditorComponentConstructor( actor, name );

            meshRendererComponent   = ConstructDefaultStaticComponent<MeshRenderer>( actor, "MeshRendererComponent", meshRendererComponent );
            meshFilterComponent     = ConstructDefaultStaticComponent<MeshFilter>( actor, "MeshFilterComponent", meshFilterComponent );
        }


        // Private:


        [SerializeField, HideInInspector]
        private MeshRenderer                meshRendererComponent;

        [SerializeField, HideInInspector]
        private MeshFilter                  meshFilterComponent;
    }
}

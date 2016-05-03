using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class ArmourActor : Actor
    {


        // Public:


        public void SetArmourMaterial( Material back, Material helm, Material gauntlet, Material shoulder, Material leg, Material cuirass, Material wing )
        {
            var renderer = GetComponent<SkinnedMeshRenderer>();
            if( renderer == null )
            {
                return;
            }

            var newMaterials = new List<Material>();
            newMaterials.Add( renderer.materials[0] );

            newMaterials.Add( cuirass );
            newMaterials.Add( gauntlet );
            newMaterials.Add( leg );
            newMaterials.Add( shoulder );
            newMaterials.Add( back );
            newMaterials.Add( wing );
            newMaterials.Add( helm );

            renderer.materials = newMaterials.ToArray();
        }


        // Private:


        [SerializeField]
        private GameObject          armourMaterialHolder = null;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Nixin
{
    [RequireComponent( typeof( Camera ) )]
    public class FogOfWarRenderComponent : NixinComponent
    {


        // Public:


        public override void OnRegistered( Actor owner, byte id )
        {
            base.OnRegistered( owner, id );

            Assert.IsTrue( fogOfWarMaterial != null, "Fog of war material cannot be null." );

            Camera cam = GetComponent<Camera>();

            cam.depthTextureMode = DepthTextureMode.Depth;
            
        }


        public override void OnOwningActorUpdate()
        {
            base.OnOwningActorUpdate();

            Camera cam = GetComponent<Camera>();

            fogOfWarMaterial.SetTexture( "_VisibilityTex", map.VisibilityMap );
            fogOfWarMaterial.SetVector( "_CameraDirection", transform.forward );
            fogOfWarMaterial.SetVector( "_MapStart", map.GridStart );
            fogOfWarMaterial.SetFloat( "_MapWidth", map.CellsAcross );
            fogOfWarMaterial.SetFloat( "_MapHeight", map.CellsDown );
            fogOfWarMaterial.SetMatrix( "_InverseProjection", ( cam.projectionMatrix ).inverse );
            fogOfWarMaterial.SetMatrix( "_InverseWorld", cam.cameraToWorldMatrix );
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "fogOfWarMaterial" )]
        private Material fogOfWarMaterial = null;

        [SerializeField]
        private FogOfWarMapComponent map = null;

        private void OnRenderImage( RenderTexture src, RenderTexture dest )
        {
            Graphics.Blit( src, dest, fogOfWarMaterial );
        }
    }
}

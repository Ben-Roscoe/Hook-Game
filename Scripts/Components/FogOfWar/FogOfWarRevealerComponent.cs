using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Nixin
{
    public class FogOfWarRevealerComponent : NixinComponent
    {


        // Public:


        public override void OnRegistered( Actor owner, byte id )
        {
            base.OnRegistered( owner, id );

            Debug.Log( "Registered!" );

            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
                UpdateComponent.UpdateRate = 0.0f;
            }
        }

        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );
          //  CreateMesh();

            // Don't want parents rotation.
          //  fogClearingMeshObject.transform.rotation = Quaternion.identity;
        }


        // Private:


        private const int   NumberOfRays    = 32;
        private const float VisionDegrees   = 360.0f;

        // TODO: Make this a stat probably?
        private const float VisionLength    = 6.0f;

        private Mesh        mesh            = null;

        [SerializeField, FormerlySerializedAs( "meshObject" )]
        private GameObject  fogClearingMeshObject      = null;

        private Vector3[]   vertexBuffer    = new Vector3[NumberOfRays + 1];
        private int[]       triangleBuffer  = new int[NumberOfRays * 3];


        private void CreateMesh()
        {
            if( mesh == null )
            {
                mesh = new Mesh();
            }

            // Raycast outwards. If we hit something, use the hit position for
            // the next vertex, if we don't hit something, use the max difference.
            // This will give us a mesh that defines what we can see.
            vertexBuffer[0] = Vector3.zero;
            for( int i = 1; i < NumberOfRays + 1; ++i )
            {
                var direction = Quaternion.AngleAxis( ( ( i - 1 ) / ( float )( NumberOfRays ) ) * VisionDegrees, 
                                Vector3.up ) * Vector3.forward;
                var ray       = new Ray( fogClearingMeshObject.transform.position, direction );

                RaycastHit hit;
                if( Physics.Raycast( ray, out hit, VisionLength, LayerDefs.FogOfWarBlocker ) )
                {
                    vertexBuffer[i] = hit.point - fogClearingMeshObject.transform.position;
                }
                else
                {
                    vertexBuffer[i] = direction * VisionLength;
                }
            }
            mesh.vertices = vertexBuffer;

            // Define the triangles. Each triangle will be drawn between vertex
            // 0, i, and i + 1, creating a fan.
            for( int i = 1; i < NumberOfRays; ++i )
            {
                triangleBuffer[( i - 1 ) * 3] = 0;
                triangleBuffer[( i - 1 ) * 3 + 1] = i;
                triangleBuffer[( i - 1 ) * 3 + 2] = i + 1;
            }
            triangleBuffer[( NumberOfRays - 1 ) * 3] = 0;
            triangleBuffer[( NumberOfRays - 1 ) * 3 + 1] = NumberOfRays;
            triangleBuffer[( NumberOfRays - 1 ) * 3 + 2] = 1;
            mesh.triangles = triangleBuffer;

            // Upload the new mesh data to the gpu.
            mesh.UploadMeshData( false );

            // Assign the mesh.
            var filter = fogClearingMeshObject.GetComponent<MeshFilter>();
            Assert.IsTrue( filter != null, "Fog clearing mesh object must has a mesh filter and a mesh rendered." );
            filter.mesh = mesh;
        }
    }
}

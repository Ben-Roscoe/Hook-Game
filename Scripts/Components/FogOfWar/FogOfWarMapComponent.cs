using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Nixin
{
    public class FogOfWarMapComponent : NixinComponent
    {


        // Public:


        public override void OnRegistered( Actor owner, byte id )
        {
            base.OnRegistered( owner, id );

            InitialiseMaps( CellSize );
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );
            BuildCollisionMatrix();
            BuildVisibilityMap();
        }


        public void RegisterBlocker( FogOfWarBlockerComponent blocker )
        {
            blockers.Add( blocker );
        }


        public Texture2D VisibilityMap
        {
            get
            {
                return visibilityMap;
            }
        }


        public Vector3 GridStart
        {
            get
            {
                return gridStart;
            }
        }


        public int CellsAcross
        {
            get
            {
                return cellsAcross;
            }
        }


        public int CellsDown
        {
            get
            {
                return cellsDown;
            }
        }


        // Private:


        private const float CellSize = 0.5f;

        [SerializeField, FormerlySerializedAs( "fogOfWarBounds" )]
        private BoxCollider       fogOfWarBounds = null;

        private List<FogOfWarBlockerComponent>  blockers  = new List<FogOfWarBlockerComponent>();
        private List<FogOfWarRevealerComponent> revealers = new List<FogOfWarRevealerComponent>();
        
        private FogMapGridType[] collisionMatrix = null;
        private Texture2D        visibilityMap   = null;

        private float            cellSize        = 1.0f;
        private Vector3          gridStart       = Vector3.zero;
        private Vector3          gridEnd         = Vector3.zero;

        private int              cellsAcross     = 0;
        private int              cellsDown       = 0;


        private void InitialiseMaps( float cellSize )
        {
            this.cellSize = cellSize;

            Assert.IsTrue( fogOfWarBounds != null, "Fog of war bounds cannot be null." );

            gridStart = fogOfWarBounds.transform.position + ( fogOfWarBounds.center - ( fogOfWarBounds.size * 0.5f ) );
            gridEnd   = fogOfWarBounds.transform.position + ( fogOfWarBounds.center + ( fogOfWarBounds.size * 0.5f ) );

            // Align all positions to an actual cell multiple.
            gridStart.x = ( ( int )( gridStart.x / cellSize ) ) * cellSize;
            gridStart.z = ( ( int )( gridStart.z / cellSize ) ) * cellSize;
            gridEnd.x   = ( ( int )( gridEnd.x / cellSize ) ) * cellSize;
            gridEnd.z   = ( ( int )( gridEnd.z / cellSize ) ) * cellSize;

            cellsAcross = ( int )( ( gridEnd.x - gridStart.x ) / cellSize );
            cellsDown   = ( int )( ( gridEnd.z - gridStart.z ) / cellSize );

            collisionMatrix = new FogMapGridType[cellsAcross * cellsDown];
            visibilityMap   = new Texture2D( cellsAcross, cellsDown );
        }


        private void BuildCollisionMatrix()
        {
            for( int i = 0; i < collisionMatrix.Length; ++i )
            {
                collisionMatrix[i] = FogMapGridType.Clear;
            }

            for( int i = 0; i < blockers.Count; ++i )
            {
                Collider collisionComponent = blockers[i].gameObject.GetComponent<Collider>();
                if( collisionComponent != null )
                {
                    BoxCollider boxComponent = collisionComponent as BoxCollider;
                    if( boxComponent != null )
                    {
                        Vector3 boxPosition = ( boxComponent.transform.position + 
                                              ( boxComponent.center - boxComponent.size * 0.5f ) - gridStart ) / cellSize;
                        Vector3 boxSize     = boxComponent.size / cellSize;

                        int cellXStart      = Mathf.Clamp( ( int )boxPosition.x, 0, cellsAcross );
                        int cellXEnd        = Mathf.Clamp( ( int )( boxPosition.x + boxSize.x ), 0, cellsAcross );
                        int cellYStart      = Mathf.Clamp( ( int )boxPosition.z, 0, cellsDown );
                        int cellYEnd        = Mathf.Clamp( ( int )( boxPosition.z + boxSize.z ), 0, cellsDown );

                        for( int y = cellYStart; y < cellYEnd; ++y )
                        {
                            for( int x = cellXStart; x < cellXEnd; ++x )
                            {
                                collisionMatrix[x + ( y * cellsAcross )] = FogMapGridType.Blocked;
                            }
                        }
                    }
                }
            }
        }


        private void BuildVisibilityMap()
        {
            for( int y = 0; y < cellsDown; ++y )
            {
                for( int x = 0; x < cellsAcross; ++x )
                {
                    visibilityMap.SetPixel( x, y, collisionMatrix[x + ( y * cellsAcross )] == FogMapGridType.Clear ? Color.white : Color.white * 0.3f );
                }
            }
            visibilityMap.Apply();
        }


        private void OnGUI()
        {
            GUI.DrawTexture( new Rect( 100.0f, 100.0f, 200.0f, 200.0f ), visibilityMap );
        }
    }


    public enum FogMapGridType : byte
    {
        Clear       = 0,
        Blocked     = 1,
    }
}

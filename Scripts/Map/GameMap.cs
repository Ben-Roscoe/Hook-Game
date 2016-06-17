using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Assertions;

namespace Nixin
{
    public class GameMap
    {


        // Public:


        public GameMap()
        {
        }


        public GameMap( MapChunk map, GameModeChunk mode )
        {
            this.Map        = map;
            this.Mode       = mode;
        }


        public void Write( NetBuffer buffer )
        {
            buffer.Write( map == null ? "" : map.AssetName );
            buffer.Write( mode == null ? "" : mode.AssetName );
            for( int i = 0; i < vars.Count; ++i )
            {
                vars[i].Write( buffer );
            }
        }


        public void Read( NetBuffer buffer, bool isFuture, World world )
        {
            string oldMapName  = map == null ? "" : map.AssetName;
            string oldModeName = mode == null ? "" : mode.AssetName;
            
            string newMapName  = buffer.ReadString( oldMapName, isFuture );
            string newModeName = buffer.ReadString( oldModeName, isFuture );

            // Change of map.
            if( newMapName != "" && newMapName != oldMapName )
            {
                Map = world.ResourceSystem.GetMapChunk( newMapName );
            }

            // Change of mode.
            if( newModeName != "" && newModeName != oldModeName )
            {
                Mode = world.ResourceSystem.GetGameModeChunk( newModeName );
            }

            for( int i = 0; i < vars.Count; ++i )
            {
                vars[i].Read( buffer, isFuture );
            }
        }


        public bool IsValid()
        {
            return Map != null && Mode != null && ( Extension != null || !Mode.RequiresMapExtension )
                && vars.Count == mode.GameVarDelcs.Count;
        }


        public MapChunk Map
        {
            get
            {
                return map;
            }
            set
            {
                map = value;
                SetExtension();
            }
        }


        public GameModeChunk Mode
        {
            get
            {
                return mode;
            }
            set
            {
                GameModeChunk oldMode = mode;
                mode = value;
                
                if( mode != oldMode && mode != null )
                {
                    SetExtension();

                    vars = new List<GameVar>();
                    for( int i = 0; i < mode.GameVarDelcs.Count; ++i )
                    {
                        vars.Add( new GameVar( mode.GameVarDelcs[i] ) );
                    }
                }
            }
        }


        public GameModeMapExtensionChunk Extension
        {
            get
            {
                return extension;
            }
        }


        public List<GameVar> Vars
        {
            get
            {
                return vars;
            }
            set
            {
                vars = value;
            }
        }


        // Private:


        private MapChunk                     map         = null;
        private GameModeChunk                mode        = null;
        private GameModeMapExtensionChunk    extension   = null;
        private List<GameVar>                vars        = new List<GameVar>();


        private void SetExtension()
        {
            if( Map != null && Mode != null )
            {
                extension = Map.GetExtension( Mode );
            }
        }
    }


    public class GameMapException : Exception
    {
        public GameMapException() : base()
        {
        }


        public GameMapException( string msg ) : base( msg )
        {
        }


        public GameMapException( string msg, Exception inner ) : base( msg, inner )
        {
        }
    }
}

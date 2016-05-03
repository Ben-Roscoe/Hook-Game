using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class NetworkGameMetaData
    {


        // Public:


        public bool CanStartNetworkGame()
        {
            return gameMap.IsValid();
        }


        public NetworkGameEntryMetaData GetEntryMetaData()
        {
            return new NetworkGameEntryMetaData( Name, playerCount, GameMap.Mode == null ? "" : 
                GameMap.Mode.name, GameMap.Map == null ? "" : GameMap.Map.name );
        }


        public void WriteSnapshot( NetBuffer buffer )
        {
            buffer.Write( name );
            buffer.Write( playerCount );
            gameMap.Write( buffer );
        }


        public void ReadSnapshot( NetBuffer buffer, bool isFuture, World world )
        {
            name            = buffer.ReadString( name, isFuture );
            playerCount     = buffer.ReadInt16( playerCount, isFuture );
            gameMap.Read( buffer, isFuture, world );
        }


        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }


        public GameMap GameMap
        {
            get
            {
                return gameMap;
            }
        }


        // Private:


        private string      name                = "";
        private Int16       playerCount         = 0;
        private GameMap     gameMap             = new GameMap();
    }

    public class NetworkGameEntryMetaData
    {



        // Public:


        public NetworkGameEntryMetaData( string name, Int16 playerCount, string gameModeName, string mapName )
        {
            this.name               = name;
            this.playerCount        = playerCount;
            this.gameModeName       = gameModeName;
            this.mapName            = mapName;
        }


        public NetworkGameEntryMetaData( NetIncomingMessage msg, ResourceSystem resourceSystem )
        {
            Read( msg, resourceSystem );
        }


        public void Write( NetBuffer buffer )
        {
            buffer.Write( name );
            buffer.Write( playerCount );
            buffer.Write( gameModeName );
            buffer.Write( mapName );
        }


        public void Read( NetIncomingMessage msg, ResourceSystem resourceSystem )
        {
            name                = msg.ReadString();
            playerCount         = msg.ReadInt16();
            gameModeName        = msg.ReadString();
            mapName             = msg.ReadString();

            ping                = ( Int16 )0;
            endPoint            = msg.SenderEndPoint;

            // Get the map's icon. We might not have this map, which means we might not have the icon
            // so check.
            var mapChunk        = resourceSystem.GetMatchMapChunk( mapName );
            if( mapChunk != null )
            {
                mapIcon = mapChunk.Icon;
            }
            else
            {
                mapIcon = null;
            }
        }


        public string Name
        {
            get
            {
                return name;
            }
        }


        public Int16 PlayerCount
        {
            get
            {
                return playerCount;
            }
            set
            {
                playerCount = value;
            }
        }


        public string GameModeName
        {
            get
            {
                return gameModeName;
            }
        }


        public string MapName
        {
            get
            {
                return mapName;
            }
        }


        public Sprite MapIcon
        {
            get
            {
                return mapIcon;
            }
        }


        public int Ping
        {
            get
            {
                return ping;
            }
            set
            {
                ping = value;
            }
        }


        public System.Net.IPEndPoint EndPoint
        {
            get
            {
                return endPoint;
            }
        }


        // Private:


        private string          name              = "";
        private Int16           playerCount       = 0;
        private string          gameModeName      = null;
        private string          mapName           = null;
        private Sprite          mapIcon           = null;

        // Receiver data.
        private int                     ping                  = 0;
        private System.Net.IPEndPoint   endPoint              = null;
    }
}

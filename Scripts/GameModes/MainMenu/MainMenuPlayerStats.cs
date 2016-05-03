using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Nixin
{
    public class MainMenuPlayerStats : StatsBase
    {


        // Public:


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );
        }


        public override void WriteSnapshot( NetBuffer buffer )
        {
            base.WriteSnapshot( buffer );

            var owningClient = ContainingWorld.NetworkSystem.GetClientStateFromId( NetworkOwner );
            if( owningClient != null && owningClient.NetConnection != null )
            {
                LobbyMetaData.Ping = ( int )owningClient.NetConnection.AverageRoundtripTime;
            }
            else
            {
                LobbyMetaData.Ping = 0;
            }
            lobbyMetaData.Write( buffer );
        }


        public override void ReadSnapshot( NetBuffer buffer, bool isFuture )
        {
            base.ReadSnapshot( buffer, isFuture );
            lobbyMetaData.Read( buffer, isFuture );
        }


        public PlayerLobbyMetaData LobbyMetaData
        {
            get
            {
                return lobbyMetaData;
            }
        }


        public override Controller Controller
        {
            get
            {
                return base.Controller;
            }
            set
            {
                base.Controller = value;
                if( IsAuthority )
                {
                    lobbyMetaData.Name = Controller != null ? Controller.NetworkOwner.ToString() : "";
                }
            }
        }


        // Private:


        private PlayerLobbyMetaData     lobbyMetaData = new PlayerLobbyMetaData();
    }
}

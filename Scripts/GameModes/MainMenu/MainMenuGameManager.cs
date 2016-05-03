using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Nixin
{
    public class MainMenuGameManager : GameManager
    {


        // Public:


        public override void InitialiseGameManager( List<GameVar> gameVars )
        {
            base.InitialiseGameManager( gameVars );

            if( !ContainingWorld.IsDedicatedServer )
            {
                AddController( null, JoinType.Player );
            }
        }


        public override void OnClientConnect( ClientState client )
        {
            base.OnClientConnect( client );

            AddController( client, JoinType.Player );
        }


        public override void OnClientDisconnect( ClientState client )
        {
            base.OnClientDisconnect( client );
            var gameState = ContainingWorld.GameState as MainMenuGameState;
        }


        public void CreateNetworkGame( string name )
        {
            var gameState = ContainingWorld.GameState as MainMenuGameState;
            gameState.NetworkGameMetaData.Name = name;   
        }


        public override void HandleNetDiscoveryRequest( NetIncomingMessage msg, NetOutgoingMessage response )
        {
            base.HandleNetDiscoveryRequest( msg, response );

            var gameState = ContainingWorld.GameState as MainMenuGameState;
            var entryMeta = gameState.NetworkGameMetaData.GetEntryMetaData();
            entryMeta.Write( response );
        }


        // Private:



    }
}

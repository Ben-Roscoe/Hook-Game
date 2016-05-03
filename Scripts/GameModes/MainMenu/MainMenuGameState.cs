using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Nixin
{
    public class MainMenuGameState : GameState
    {


        // Public:


        public bool CanStartNetworkGame()
        {
            return NetworkGameMetaData.CanStartNetworkGame();
        }


        public override void WriteSnapshot( NetBuffer buffer )
        {
            base.WriteSnapshot( buffer );

            networkGameMetaData.WriteSnapshot( buffer );
        }


        public override void ReadSnapshot( NetBuffer buffer, bool isFuture )
        {
            base.ReadSnapshot( buffer, isFuture );

            networkGameMetaData.ReadSnapshot( buffer, isFuture, ContainingWorld );
        }


        public NetworkGameMetaData NetworkGameMetaData
        {
            get
            {
                return networkGameMetaData;
            }
        }


        // Private:


        private NetworkGameMetaData         networkGameMetaData = new NetworkGameMetaData();
    }
}

using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class PlayerLobbyMetaData
    {


        // Public:


        public void Write( NetBuffer buffer )
        {
            buffer.Write( name );
            buffer.Write( ping );
        }


        public void Read( NetBuffer buffer, bool isFuture )
        {
            name = buffer.ReadString( name, isFuture );
            ping = buffer.ReadInt32( ping, isFuture );
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


        // Private:


        private string  name     = "";
        private int     ping     = 0;
    }
}

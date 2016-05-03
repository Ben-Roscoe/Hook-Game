using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using UnityEngine;

namespace Nixin
{
    public class TeamDeathmatchPlayerStats : HookGameMatchPlayerStats
    {


        // Public:


        public TeamType TeamType
        {
            get
            {
                return repTeam ? TeamType.Blue : TeamType.Red;
            }
            set
            {
                if( IsAuthority )
                {
                    repTeam = value == TeamType.Red ? false : true;
                }
            }
        }


        public override void WriteSnapshot( NetBuffer buffer )
        {
            base.WriteSnapshot( buffer );

            buffer.Write( repTeam );
        }


        public override void ReadSnapshot( NetBuffer buffer, bool isFuture )
        {
            base.ReadSnapshot( buffer, isFuture );

            repTeam = buffer.ReadBoolean( repTeam, isFuture );
        }


        // Private:


        private bool              repTeam       = false;
    }
}

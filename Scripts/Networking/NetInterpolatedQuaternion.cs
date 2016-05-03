using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    class NetInterpolatedQuaternion
    {


        // Public:

            
        public Quaternion      From { get; set; }
        public Quaternion      To   { get; set; }


        public Quaternion GetWorldNetworkInterpolatedQuaternion( World world )
        {
            if( world == null || world.NetworkSystem == null )
            {
                return From;
            }


            double tyme = world.NetworkSystem.CurrentClientInterpolationTime;
            double styp = NetTime.Now;
            double currentStepDelta = ( world.NetworkSystem.CurrentClientInterpolationTime - world.NetworkSystem.CurrentSnapshot.Timestamp );
            double total            = 0.0;
            if( world.NetworkSystem.CurrentSnapshot != world.NetworkSystem.NextSnapshot )
            {
                total = world.NetworkSystem.NextSnapshot.Timestamp - world.NetworkSystem.CurrentSnapshot.Timestamp;
            }
            else
            {
                total = world.NetworkSystem.NetworkSendPeriod;
            }

            float  t                = ( float )( currentStepDelta / ( total ) );// Mathf.Clamp( , 0.0f, 1.0f );
            t = Mathf.Clamp( t, 0.0f, 1.2f );
            return Quaternion.LerpUnclamped( From, To, t );
        }
    }
}

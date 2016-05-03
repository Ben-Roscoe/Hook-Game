using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class NetInterpolatedVector
    {


        // Public:


        public Vector3     From { get; set; }
        public Vector3     To   { get; set; }


        public NetInterpolatedVector()
        {
            From    = Vector3.zero;
            To      = Vector3.zero;
        }


        public Vector3 GetWorldNetworkInterpolatedVector( World world )
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

            float  t                = ( float )( currentStepDelta / ( total ) );
            t = Mathf.Clamp( t, 0.0f, 1.2f );
            return Vector3.LerpUnclamped( From, To, t );
        }



        // Private:



    }
}

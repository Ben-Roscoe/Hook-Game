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


            double currentStepDelta = ( world.NetworkSystem.CurrentClientInterpolationTime - world.NetworkSystem.CurrentSnapshot.Timestamp );
            double total            = 0.0;
            total = world.NetworkSystem.NextSnapshot.Timestamp - world.NetworkSystem.CurrentSnapshot.Timestamp;
            float  t                = ( float )( currentStepDelta / ( total ) );

            Quaternion result = Quaternion.LerpUnclamped( From, To, t );
            return result;
        }
    }
}

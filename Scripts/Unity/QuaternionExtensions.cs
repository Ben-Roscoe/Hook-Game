using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public static class QuaternionExtensions
    {


        // Public:


        public static Quaternion LerpNoClamp( Quaternion from, Quaternion to, float t )
        {
            var fromAngle     = 0.0f;
            var fromAxis      = Vector3.zero;
            from.ToAngleAxis( out fromAngle, out fromAxis );

            var toAngle   = 0.0f;
            var toAxis    = Vector3.zero;
            to.ToAngleAxis( out toAngle, out toAxis );

            return Quaternion.Euler( Vector3Extensions.LerpNoClamp( fromAngle * fromAxis, toAngle * toAxis, t ) );
        }
    }
}

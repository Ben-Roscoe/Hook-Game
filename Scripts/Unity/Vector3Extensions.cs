using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public static class Vector3Extensions
    {


        // Public:


        public static Vector3 LerpNoClamp( Vector3 from, Vector3 to, float t )
        {
            return from + ( ( to - from ) * t );
        }
    }
}

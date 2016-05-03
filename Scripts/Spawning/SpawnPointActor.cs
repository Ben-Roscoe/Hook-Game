using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class SpawnPointActor : Actor
    {


        // Public:


        public virtual Vector3 GetSpawnPosition()
        {
            return transform.position;
        }


        public virtual Quaternion GetSpawnRotation()
        {
            return transform.rotation;
        }
    }
}

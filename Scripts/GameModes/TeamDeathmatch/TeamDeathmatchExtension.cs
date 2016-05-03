using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class TeamDeathmatchExtension : GameModeMapExtension
    {


        // Public:


        public List<TeamDeathmatchSpawnPoint> SpawnPoints
        {
            get
            {
                return spawnPoints;
            }
        }


        // Private:


        [SerializeField]
        private List<TeamDeathmatchSpawnPoint>       spawnPoints = new List<TeamDeathmatchSpawnPoint>();
    }
}

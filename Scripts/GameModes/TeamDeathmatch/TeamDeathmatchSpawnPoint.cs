using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class TeamDeathmatchSpawnPoint : SpawnPointActor
    {


        // Public:


        public TeamType Team
        {
            get
            {
                return team;
            }
        }


        // Private:


        [SerializeField]
        private TeamType        team;
    }
}

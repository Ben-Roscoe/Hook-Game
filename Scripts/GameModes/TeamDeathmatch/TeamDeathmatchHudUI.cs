using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class TeamDeathmatchHudUI : MatchGameHudBase
    {

        // Public:


        public override void SetUp( Player localPlayer )
        {
            base.SetUp( localPlayer );
        }


        public TeamDeathmatchScoreUI TeamDeathmatchScore
        {
            get
            {
                return teamDeathmatchScore;
            }
        }


        // Private:


        [SerializeField]
        private TeamDeathmatchScoreUI teamDeathmatchScore = null;
    }
}

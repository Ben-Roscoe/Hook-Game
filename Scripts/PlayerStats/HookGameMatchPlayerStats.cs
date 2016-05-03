using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nixin
{
    public class HookGameMatchPlayerStats : StatsBase
    {


        // Public:


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            // Register stats.
            health       = AddStat( StatDefs.healthName );
            meleeDamage  = AddStat( StatDefs.meleeDamageName );
            hookDamage   = AddStat( StatDefs.hookDamageName );
            hookSpeed    = AddStat( StatDefs.hookSpeedName );
            hookDistance = AddStat( StatDefs.hookDistanceName );
            gold         = AddStat( StatDefs.goldName );
            experience   = AddStat( StatDefs.experienceName );
            skillPoint   = AddStat( StatDefs.skillPointName );

            health.BaseValue        = 100.0f;
            skillPoint.BaseValue    = 0.0f;

            experience.ValueChanged.AddHandler( OnExperienceChanged );
        }


        public override void OnActorDestroy()
        {
            base.OnActorDestroy();

            experience.ValueChanged.RemoveHandler( OnExperienceChanged );
        }


        public Stat Health
        {
            get
            {
                return health;
            }
            set
            {
                health = value;
            }
        }


        public Stat MeleeDamage
        {
            get
            {
                return meleeDamage;
            }
            set
            {
                meleeDamage = value;
            }
        }


        public Stat HookDamage
        {
            get
            {
                return hookDamage;
            }
            set
            {
                hookDamage = value;
            }
        }


        public Stat HookSpeed
        {
            get
            {
                return hookSpeed;
            }
            set
            {
                hookSpeed = value;
            }
        }


        public Stat HookDistance
        {
            get
            {
                return hookDistance;
            }
            set
            {
                hookDistance = value;
            }
        }


        public Stat HookRadius
        {
            get
            {
                return hookRadius;
            }
            set
            {
                hookRadius = value;
            }
        }


        public Stat Gold
        {
            get
            {
                return gold;
            }
            set
            {
                gold = value;
            }
        }


        public Stat Experience
        {
            get
            {
                return experience;
            }
            set
            {
                experience = value;
            }
        }


        public Stat SkillPoint
        {
            get
            {
                return skillPoint;
            }
            set
            {
                skillPoint = value;
            }
        }


        // Private:


        private Stat      health          = null;
        private Stat      meleeDamage     = null;
        private Stat      hookDamage      = null;
        private Stat      hookSpeed       = null;
        private Stat      hookDistance    = null;
        private Stat      hookRadius      = null;

        private Stat      gold            = null;
        private Stat      experience      = null;
        private Stat      skillPoint      = null;


        private void OnExperienceChanged( Stat experienceStat, float old, float current )
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Nixin
{
    public class HealthMagicUI : UIActor
    {


        // Public:


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections,
            Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
                UpdateComponent.UpdateRate = 0.0f;
            }
        }


        public override void OnPostHierarchyInitialise()
        {
            base.OnPostHierarchyInitialise();
            SetPlayer( null );
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );

            if( player == null || player.Stats == null )
            {
                return;
            }

            GetStats();

            if( healthSlider == null || currentHealth == null || maxHealth == null )
            {
                return;
            }

            healthSlider.maxValue   = maxHealth.ModifiedValue;
            healthSlider.value      = currentHealth.ModifiedValue;
        }


        public void SetPlayer( Player newPlayer )
        {
            player = newPlayer;

            if( healthSlider == null )
            {
                return;
            }

            healthSlider.minValue = 0.0f;

            if( player == null || player.Stats == null )
            {
                healthSlider.maxValue = 0.0f;
                return;
            }

            currentHealth   = null;
            maxHealth       = null;
            GetStats();

            if( currentHealth != null && maxHealth != null )
            {
                healthSlider.maxValue = maxHealth.ModifiedValue;
            }
        }


        // Private:


        [SerializeField]
        private Slider                              healthSlider    = null;                      


        private float                               lastHealth      = 0.0f;
        private float                               lastMagic       = 0.0f;

        private Player                              player          = null;
        private Stat                                currentHealth   = null;
        private Stat                                maxHealth       = null;


        private void GetStats()
        {
            if( player == null )
            {
                return;
            }

            currentHealth   = player.Stats.GetStat( StatDefs.currentHealthName.Id, true );
            maxHealth       = player.Stats.GetStat( StatDefs.healthName.Id, true );
        }
    }
}

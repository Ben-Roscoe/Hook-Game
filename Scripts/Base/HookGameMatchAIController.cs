using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nixin
{
    public class HookGameMatchAIController : AIController
    {


        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

            hookCharacterOwnerComponent = ConstructDefaultComponent<HookCharacterOwnerComponent>( this, "HookCharacterOwnerComponent", hookCharacterOwnerComponent );
        }


        public override void Possess( ControllableActor controllableActor )
        {
            base.Possess( controllableActor );

            if( !IsAuthority || Stats == null )
            {
                return;
            }

            // Setup the controllable actor's current health.
            var currentHealth   = controllableActor.GetStat( StatDefs.currentHealthName.Id, false );
            var maxHealth       = Stats.GetStat( StatDefs.healthName.Id, false );
            if( currentHealth != null && maxHealth != null )
            {
                currentHealth.StatMax = maxHealth;
                currentHealth.BaseValue = maxHealth.ModifiedValue;
            }
        }


        public override void OnPossessedControllableDeath( Actor killer, StatModifier modifier )
        {
            base.OnPossessedControllableDeath( killer, modifier );
            HookCharacterOwnerComponent.OnControllableActorDeath( killer, modifier );
        }


        public override Stat GetStat( int nameHash, bool includeChildren )
        {
            // Just make sure that things can get our current health and magic from here.
            var ret = base.GetStat( nameHash, includeChildren );
            if( ret != null || PossessedActor == null || !includeChildren )
            {
                return ret;
            }
            return PossessedActor.GetStat( nameHash, includeChildren );
        }


        public HookCharacterOwnerComponent HookCharacterOwnerComponent
        {
            get
            {
                return hookCharacterOwnerComponent;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "HookCharacterOwnerComponent" ), HideInInspector]
        private HookCharacterOwnerComponent                             hookCharacterOwnerComponent = null;

    }
}

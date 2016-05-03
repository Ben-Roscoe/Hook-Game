using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class AbilityTargetComponent : NixinComponent
    {


        // Public:


        public void AddAbility( AbilityInstance ability )
        {
            activeAbilities.Add( ability );
        }


        public void AddUnique( AbilityInstance ability )
        {
            if( activeAbilities.Find( x => x.Id == ability.Id ) == null )
            {
                activeAbilities.Add( ability );
            }
        }


        public void RemoveAbility( AbilityInstance ability )
        {
            int index = activeAbilities.IndexOf( ability );
            if( index >= 0 )
            {
                activeAbilities[index].Meta.DestroyInstance( activeAbilities[index] );
                activeAbilities.RemoveAt( index );
            }
        }


        public void RemoveAbility( NixinName id )
        {
            for( int i = 0; i < activeAbilities.Count; ++i )
            {
                if( activeAbilities[i].Id == id )
                {
                    activeAbilities[i].Meta.DestroyInstance( activeAbilities[i] );
                    activeAbilities.RemoveAt( i );
                    --i;
                }
            }
        }


        // Private:


        private List<AbilityInstance>       activeAbilities = new List<AbilityInstance>();
    }
}

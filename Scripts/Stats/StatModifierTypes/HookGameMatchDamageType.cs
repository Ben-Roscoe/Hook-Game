using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class HookGameMatchDamageType : HookGameMatchModifierType
    {


        // Public:


        public virtual bool ShouldCauseBlood()
        {
            return true;
        }


        // Private:



    }
}

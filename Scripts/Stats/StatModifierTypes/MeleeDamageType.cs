using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class MeleeDamageType : HookGameMatchDamageType
    {


        // Public:


        public override bool ShouldCauseBlood()
        {
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class NixinSystem
    {


        // Public:


        public NixinSystem( World containingWorld )
        {
            this.containingWorld = containingWorld;
        }


        public World ContainingWorld
        {
            get
            {
                return containingWorld;
            }
        }


        // Private:


        private World               containingWorld = null;
    }
}

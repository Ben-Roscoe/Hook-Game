using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public struct Trigger
    {


        // Public:


        public void Load( bool v )
        {
            if( v != last )
            {
                isLoaded    = v;
                last        = v;
            }
        }


        public bool Fire()
        {
            var current = isLoaded;
            isLoaded    = false;
            return current;
        }


        // Private:


        private bool            isLoaded;
        private bool            last;
    }
}

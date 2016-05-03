using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public struct NixinName
    {

        public static bool operator==( NixinName a, NixinName b )
        {
            return a.id == b.id;
        }


        public static bool operator!=( NixinName a, NixinName b )
        {
            return a.id != b.id;
        }


        // Public:


        public NixinName( string name )
        {
            this.id   = name.GetHashCode();
        }


        public override int GetHashCode()
        {
            return id;
        }


        public override bool Equals( object obj )
        {
            if( obj is NixinName )
            {
                var other = ( NixinName )obj;
                return other.id == id;
            }
            return false;
        }


        public int Id
        {
            get
            {
                return id;
            }
        }


        // Private:


        private int    id;
    }
}

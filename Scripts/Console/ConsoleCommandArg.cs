using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public struct ConsoleCommandArg
    {


        // Public:


        public ConsoleCommandArg( string name,string typeName )
        {
            this.name           = name;
            this.typeName       = typeName;
        }


        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }


        public string TypeName
        {
            get
            {
                return typeName;
            }
            set
            {
                typeName = value;
            }
        }


        // Private:


        private string                      name;
        private string                      typeName;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false, Inherited = true )]
    public class ConsoleCommandDefAttribute : Attribute
    {


        // Public:


        public ConsoleCommandDefAttribute( string commandName )
        {
            this.commandName = commandName;
        }


        public string CommandName
        {
            get
            {
                return commandName;
            }
        }


        // Private:


        private string commandName = null;
    }
}

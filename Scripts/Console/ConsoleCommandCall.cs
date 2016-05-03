using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class ConsoleCommandCall
    {


        // Public:


        public ConsoleCommandCall( string commandName, List<ConsoleCommandArgInstance> argInstances )
        {
            this.commandName        = commandName;
            this.argInstances       = argInstances;
        }


        public object[] GetParameterData()
        {
            return argInstances.Select( x => x.Data ).ToArray();
        }


        public string CommandName
        {
            get
            {
                return commandName;
            }
            set
            {
                commandName = value;
            }
        }


        public List<ConsoleCommandArgInstance> ArgInstances
        {
            get
            {
                return argInstances;
            }
            set
            {
                argInstances = value;
            }
        }


        // Private:


        private string                                  commandName     = null;
        private List<ConsoleCommandArgInstance>         argInstances    = null;
    }
}

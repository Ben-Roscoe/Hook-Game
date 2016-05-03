using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public enum ConnectToServerRequestResult
    {
        Connected,
        Unconnected,
        Canceled,
        None,
    }

    public class ConnectToServerRequest
    {


        // Public:


        public ConnectToServerRequest( string address, int port )
        {
            this.address = address;
            this.port    = port;
        }


        public NixinEvent<ConnectToServerRequest> OnCompleted
        {
            get
            {
                return onCompleted;
            }
        }


        public void Completed( ConnectToServerRequestResult result )
        {
            this.result = result;
            OnCompleted.Invoke( this );
            OnCompleted.RemoveAll();
        }


        public NetConnection Connection
        {
            get
            {
                return connection;
            }
            set
            {
                connection = value;
            }
        }


        public string Address
        {
            get
            {
                return address;
            }
        }


        public int Port
        {
            get
            {
                return port;
            }
        }


        public ConnectToServerRequestResult Result
        {
            get
            {
                return result;
            }
        }



        // Private:


        private NixinEvent<ConnectToServerRequest>  onCompleted = new NixinEvent<ConnectToServerRequest>();
        
        private string                              address     = "";
        private int                                 port        = -1;
        private ConnectToServerRequestResult        result      = ConnectToServerRequestResult.None;
        private NetConnection                       connection  = null;
    }
}

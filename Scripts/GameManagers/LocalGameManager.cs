using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class LocalGameManager : Actor
    {


        // Public:


        public virtual void ReadNetDiscoveryResponse( NetIncomingMessage msg )
        {
        }


        public virtual void OnConnectToServer()
        {
        }


        // Private:



    }


    [System.Serializable]
    public class LocalGameManagerWeakReference : WeakUnityReference<LocalGameManager>
    {
    }
}

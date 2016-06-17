using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Nixin
{
    public class MainMenuLocalGameManager : LocalGameManager
    {


        // Public:


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            Assert.IsTrue( mainMenuUIPrefab != null );
            mainMenuUI = ( MainMenuUI )ContainingWorld.InstantiateUIActor( mainMenuUIPrefab, null );
        }


        public override void ReadNetDiscoveryResponse( NetIncomingMessage msg )
        {
            base.ReadNetDiscoveryResponse( msg );

            NetworkGameEntryMetaData entry = new NetworkGameEntryMetaData( msg, ContainingWorld.ResourceSystem );
            entryMetaDatas.Add( entry );

            OnGetRunningGameMetaDataComplete.Invoke( entryMetaDatas );
        }


        public ConnectToServerRequest JoinLobby( NetworkGameEntryMetaData lobby )
        {
            ConnectToServerRequest request = new ConnectToServerRequest( lobby.EndPoint.Address.ToString(), lobby.EndPoint.Port );
            ContainingWorld.NetworkSystem.ConnectToServer( request );
            return request;
        }


        public void GetRunningGameMetaData()
        {
            entryMetaDatas.Clear();

            // TODO: Remove this!
            for( int i = HookGameWorld.startPort; i < HookGameWorld.endPort; ++i )
            {
                ContainingWorld.NetworkSystem.SendNetworkDiscoveryRequest( i );
            }
        }


        public NixinEvent<List<NetworkGameEntryMetaData>> OnGetRunningGameMetaDataComplete
        {
            get
            {
                return onGetRunningGameMetaDataComplete;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "MainMenuUIPrefab" )]
        private MainMenuUI              mainMenuUIPrefab        = null;

        private MainMenuUI              mainMenuUI              = null;

        private NixinEvent<List<NetworkGameEntryMetaData>>      onGetRunningGameMetaDataComplete = new NixinEvent<List<NetworkGameEntryMetaData>>();
        private List<NetworkGameEntryMetaData>                  entryMetaDatas                   = new List<NetworkGameEntryMetaData>();
    }
}

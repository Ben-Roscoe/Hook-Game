using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nixin
{
    public enum NetType
    {
        Unconnected,
        Server,
        Client,
    }

    public enum DataMessageType
    {
        Snapshot,
        SnapshotACK,
        ServerCommand,
    }


    public class NetworkSystem : NixinSystem
    {


        // Public:


        public NetworkSystem( World containingWorld, string appIdentifier, int port ) : base( containingWorld )
        {
            netConfig           = new NetPeerConfiguration( appIdentifier );
            netConfig.Port      = port;
            
            receivedSnapshots   = new SnapshotBuffer();
            currentSnapshot     = receivedSnapshots.CurrentSnapshot;
            nextSnapshot        = receivedSnapshots.CurrentSnapshot;

            netConfig.EnableMessageType( NetIncomingMessageType.DiscoveryRequest );
            netConfig.EnableMessageType( NetIncomingMessageType.DiscoveryResponse );
            netConfig.EnableMessageType( NetIncomingMessageType.ConnectionApproval );
            netConfig.EnableMessageType( NetIncomingMessageType.StatusChanged );
            netConfig.EnableMessageType( NetIncomingMessageType.Data );
            netConfig.AcceptIncomingConnections = true;

            // TODO: Shouldn't be here probably.
            //netConfig.SimulatedRandomLatency    = 0.050f;
            //netConfig.SimulatedMinimumLatency   = 0.100f;
           // netConfig.SimulatedDuplicatesChance = 0.25f;
           // netConfig.SimulatedLoss             = 0.25f;

            // TODO: Maybe this can be moved elsewhere?
            netPeer = new NetPeer( netConfig );

            netType = NetType.Unconnected;

            StartPeer();
        }


        public void StartPeer()
        {
            Assert.IsTrue( netType == NetType.Unconnected, "NetType must be unconnected to start a peer." );
            netPeer.Start();
        }


        public void StartServer()
        {
            Assert.IsTrue( netType == NetType.Unconnected, "Must be unconnected to start a server." );
            netType = NetType.Server;

            timeTilNextSend = NetworkSendPeriod;
        }


        public void ConnectToServer( ConnectToServerRequest request )
        {
            Assert.IsTrue( netType == NetType.Unconnected, "This peer is either a server or is connected to one." );
            
            request.Connection = netPeer.Connect( request.Address, request.Port );
            connectToServerRequests.Add( request );
        }


        public void Disconnect()
        {
            if( netType == NetType.Client )
            {
                Assert.IsTrue( netType == NetType.Client, "NetType must be Client in order to disconnect from a server." );
                netPeer.Connections[0].Disconnect( "Cya" );
            }
            else if( netType == NetType.Server )
            {
                for( int i = 0; i < netPeer.Connections.Count; ++i )
                {
                    netPeer.Connections[i].Disconnect( "Cya" );
                }
            }
            netType = NetType.Unconnected;
        }


        public void Shutdown()
        {
            NetPeer.Shutdown( "Cya" );
        }


        public void SendNetworkDiscoveryRequest( int port )
        {
            Assert.IsTrue( NetPeer != null && NetPeer.Status == NetPeerStatus.Running );
            NetPeer.DiscoverLocalPeers( port );
        }


        public void SendNetworkDiscoveryResponse( NetOutgoingMessage response, System.Net.IPEndPoint to )
        {
            Assert.IsTrue( NetPeer != null && NetPeer.Status == NetPeerStatus.Running );
            NetPeer.SendDiscoveryResponse( response, to );
        }


        public RPCCall CreateRPCCall( Actor destinationActor, RPCMethod method, bool reliable, object[] parameters )
        {
            Assert.IsTrue( method != null && destinationActor != null );

            // Parse parameters to our message format.
            List<RPCParameter> rpcParameters = new List<RPCParameter>( parameters.Length );
            foreach( object parameter in parameters )
            {
                RPCParameterId parameterId = RPCCall.GetParameterIdFromData( parameter );
                Assert.IsTrue( parameterId != RPCParameterId.Invalid );

                rpcParameters.Add( new RPCParameter( parameter, parameterId ) );
            }

            // Create the call given the parameters.
            return new RPCCall( destinationActor.Id, method.Id, rpcParameters, reliable, destinationActor.AcceptsNewConnections, networkStep );
        }


        public void AddMulticastRPC( RPCCall call, Actor destination )
        {
            Assert.IsTrue( IsAuthoritative );

            foreach( Relevancy relevency in destination.Relevancies )
            {
                if( relevency.Type == RelevancyType.Active && !relevency.Client.IsLoadingCurrentMap )
                {
                    relevency.Client.SnapshotBuffer.CurrentSnapshot.AddRPC( call );
                }
            }
        }


        public void SendServerRPC( RPCCall call, bool reliable )
        {
            Assert.IsTrue( !IsAuthoritative );

            // Write the message.
            NetOutgoingMessage message = CreateNewDataMessage( DataMessageType.ServerCommand );
            call.WriteRPC( message );

            // TODO: Probably don't want to send messages for each command like this.
            NetPeer.SendMessage( message, ServerConnection, NetDeliveryMethod.ReliableOrdered );
        }


        public RPCCall BufferRPCCall( RPCCall call, RPCCall previous )
        {
            // Only buffer calls that are relevant to every one.
            if( call == null || !IsAuthoritative )
            {
                return null;
            }

            // Remove last call if there is one.
            bufferedRPCCalls.Remove( previous );
            bufferedRPCCalls.Add( call );

            Actor destinationActor = ContainingWorld.Actors[call.DestinationActorId];
            for( int i = 0; i < destinationActor.Relevancies.Count; ++i )
            {
                if( destinationActor.Relevancies[i].Type == RelevancyType.Deactive )
                {
                    destinationActor.Relevancies[i].Client.BufferedRPCs.Remove( previous );
                    destinationActor.Relevancies[i].Client.BufferedRPCs.Add( call );
                }
            }

            return call;
        }


        public void UnbufferRPCCall( RPCCall call )
        {
            if( !IsAuthoritative )
            {
                return;
            }

            // Remove the call if there is one.
            bufferedRPCCalls.Remove( call );

            Actor destinationActor = ContainingWorld.Actors[call.DestinationActorId];
            for( int i = 0; i < destinationActor.Relevancies.Count; ++i )
            {
                if( destinationActor.Relevancies[i].Type == RelevancyType.Deactive )
                {
                    destinationActor.Relevancies[i].Client.BufferedRPCs.Remove( call );
                }
            }
        }


        public void UnbufferRPCCallForActor( Actor actor )
        {
            if( !IsAuthoritative )
            {
                return;
            }

            // Remove all calls with this actor id.
            bufferedRPCCalls.RemoveAll( x => x.DestinationActorId == actor.Id );

            for( int i = 0; i < actor.Relevancies.Count; ++i )
            {
                if( actor.Relevancies[i].Type == RelevancyType.Deactive )
                {
                    actor.Relevancies[i].Client.BufferedRPCs.RemoveAll( x => x.DestinationActorId == actor.Id );
                }
            }
        }


        public void UnbufferRPCCallForActorComponent( NixinComponent component )
        {
            if( !IsAuthoritative )
            {
                return;
            }

            bufferedRPCCalls.RemoveAll( x => x.DestinationActorId == component.Owner.Id );

            if( component.Owner == null )
            {
                return;
            }

            Actor destinationActor = ContainingWorld.Actors[component.Owner.Id];
            if( destinationActor == null )
            {
                return;
            }

            for( int i = 0; i < destinationActor.Relevancies.Count; ++i )
            {
                if( destinationActor.Relevancies[i].Type == RelevancyType.Deactive )
                {
                    destinationActor.Relevancies[i].Client.BufferedRPCs.RemoveAll( x => x.DestinationActorId == destinationActor.Id );
                }
            }
        }


        public void CreateRelevancy( ClientState client, Actor actor, RelevancyType type )
        {
            Relevancy relevancy = new Relevancy( client, actor, type );
            client.Relevancies.Add( relevancy );
            actor.Relevancies.Add( relevancy );
        }


        public void RemoveRelevancy( ClientState client, Actor actor )
        {
            client.Relevancies.RemoveAll( x => x.Actor == actor );
            actor.Relevancies.RemoveAll( x => x.Client == client );
        }


        public void ExecuteServerCommands()
        {
            for( int i = 0; i < serverCommandsToExecute.Count; ++i )
            {
                InvokeRPCCall( serverCommandsToExecute[i] );
            }
            serverCommandsToExecute.Clear();
        }


        public void Update()
        {
            if( IsAuthoritative )
            {
                ExecuteServerCommands();
            }
            else
            {
                ProcessSnapshots();
            }
        }


        public void ReadUpdate()
        {
            if( NetPeer == null )
            {
                return;
            }
            ProcessNetworkMessages();
        }


        public void TrySendUpdate( float deltaTime )
        {
            if( NetPeer == null || netType != NetType.Server )
            {
                return;
            }

            timeTilNextSend -= deltaTime;
            if( timeTilNextSend <= 0.0 )
            {
                // Copy the world to each client's snapshot.
                for( int i = 0; i < clients.Count; ++i )
                {
                    clients[i].SnapshotBuffer.CurrentSnapshot.CopyClientWorldState( clients[i], ContainingWorld.LocalActorStart );
                    SendSnapshot( clients[i] );
                    clients[i].SnapshotBuffer.NewSnapshot( networkStep + 1 );
                }

                ++networkStep;
                timeTilNextSend = NetworkSendPeriod;
            }
        }


        public NetOutgoingMessage CreateNewDataMessage( DataMessageType messageType )
        {
            NetOutgoingMessage message = netPeer.CreateMessage();
            message.Write( ( byte )messageType );
            return message;
        }


        public ClientState GetClientStateFromId( long id )
        {
            Assert.IsTrue( netType == NetType.Server );
            return clients.Find( x => x.Id == id );
        }


        public ClientState GetClientStateFromConnection( NetConnection connection )
        {
            return GetClientStateFromId( connection.RemoteUniqueIdentifier );
        }


        public double CurrentClientInterpolationTime
        {
            get
            {
                // The current time the client is running at.
                return ( NetTime.Now - ( interpolationBufferLegnth ) * NetworkSendPeriod )
                     - ( ServerConnection == null ? 0.0 : ServerConnection.AverageRoundtripTime / 2.0 );
            }
        }


        public long Id
        {
            get
            {
                return netPeer.UniqueIdentifier;
            }
        }


        public List<ClientState> Clients
        {
            get
            {
                return clients;
            }
        }


        public double NetworkSendPeriod
        {
            get
            {
                return networkSendRate > 0 ? 1.0 / ( networkSendRate ) : 0;
            }
        }


        public NetConnection ServerConnection
        {
            get
            {
                if( netType == NetType.Client )
                {
                    return NetPeer.Connections[0];
                }
                return null;
            }
        }


        public long AuthorityId
        {
            get
            {
                long ret = 0;
                if( netType == NetType.Server || netType == NetType.Unconnected )
                {
                    ret = NetPeer.UniqueIdentifier;
                }
                else
                {
                    ret = NetPeer.Connections[0].RemoteUniqueIdentifier;
                }
                return ret;
            }
        }


        public NetPeerConfiguration NetConfig
        {
            get
            {
                return netConfig;
            }
        }


        public NetPeer NetPeer
        {
            get
            {
                return netPeer;
            }
        }


        public bool IsAuthoritative
        {
            get
            {
                return netType == NetType.Unconnected || netType == NetType.Server;
            }
        }


        public NetType CurrentNetType
        {
            get
            {
                return netType;
            }
        }


        public Snapshot CurrentSnapshot
        {
            get
            {
                return currentSnapshot;
            }
        }


        public Snapshot NextSnapshot
        {
            get
            {
                return nextSnapshot;
            }
        }


        // Private:


        private NetType                     netType     = NetType.Unconnected;
        private NetPeer                     netPeer     = null;
        private NetPeerConfiguration        netConfig   = null;
        
        private double                      networkSendRate                 = 20.0;
        private int                         interpolationBufferLegnth       = 2;

        private List<RPCCall>               bufferedRPCCalls                = new List<RPCCall>();

        private List<ClientState>           clients                         = new List<ClientState>();

        private int                         networkStep                     = 0;
        private int                         currentClientStep               = -1;
        private double                      timeTilNextSend                 = 0.0f;

        // Client only stuff.
        private SnapshotBuffer              receivedSnapshots               = null;
        private Snapshot                    currentSnapshot                 = null;
        private Snapshot                    nextSnapshot                    = null;
        private int                         lastAckdSnapshot                = -1;
        private int                         receivedSnapshotCount           = 0;

        private List<NetIncomingMessage>    incomingMsgs                    = new List<NetIncomingMessage>();
        private List<RPCCall>               serverCommandsToExecute         = new List<RPCCall>();
        private List<ConnectToServerRequest> connectToServerRequests        = new List<ConnectToServerRequest>();


        private void OnClientConnected( NetConnection connection )
        {
            Assert.IsTrue( netType == NetType.Server, "Non-server received client connected event." );

            // Create the new client state.
            ClientState client = new ClientState( connection );
            clients.Add( client );

            CreateInitialSnapshot( client );

            ContainingWorld.OnConnected( connection );
        }


        private void OnClientDisconnect( NetConnection connection )
        {
            Assert.IsTrue( netType == NetType.Server, "Non-server received client disconnected event." );

            ClientState client = GetClientFromConnection( connection );

            for( int i = 0; i < ContainingWorld.LocalActorStart; ++i )
            {
                if( ContainingWorld.Actors[i] != null )
                {
                    RemoveRelevancy( client, ContainingWorld.Actors[i] );
                }
            }
            ContainingWorld.OnDisconnected( connection );

            // Remove the client state. It's only valid for this session.
            clients.RemoveAll( x => x.NetConnection == connection );
        }


        private void OnConnectToServer( NetConnection serverConnection )
        {
            Assert.IsTrue( netType == NetType.Unconnected, "Non-unconnected received connected to server event." );
            ContainingWorld.OnConnected( serverConnection );

            netType = NetType.Client;
        }


        private void OnDisconnectFromServer( NetConnection serverConnection )
        {
            Assert.IsTrue( netType == NetType.Client, "Non-client received diconnected from server event." );
            ContainingWorld.OnDisconnected( serverConnection );

            netType = NetType.Unconnected;
        }


        private void CreateInitialSnapshot( ClientState client )
        {
            if( netType != NetType.Server )
            {
                return;
            }

            // Add all buffered rpcs to the client's list.
            client.SnapshotBuffer.NewSnapshot( networkStep );
            for( int i = 0; i < bufferedRPCCalls.Count; ++i )
            {
                client.BufferedRPCs.Add( bufferedRPCCalls[i] );
            }

            // Add all actors that accept new relevancies as relevant to this client.
            for( int i = 0; i < ContainingWorld.LocalActorStart; ++i )
            {
                if( ContainingWorld.Actors[i] != null && ContainingWorld.Actors[i].AcceptsNewConnections )
                {
                    CreateRelevancy( client, ContainingWorld.Actors[i], RelevancyType.Deactive );
                }
            }
        }


        private void SendSnapshot( ClientState client )
        {
            if( netType != NetType.Server )
            {
                return;
            }

            NetOutgoingMessage msg = CreateNewDataMessage( DataMessageType.Snapshot );
            client.SnapshotBuffer.CurrentSnapshot.WriteDelta( msg, client.SnapshotBuffer.GetLastACKSnapshot(), client.SnapshotBuffer.GetMissedCalls() );
            NetPeer.SendMessage( msg, client.NetConnection, NetDeliveryMethod.Unreliable );
        }


        private void SendSnapshotACK( Snapshot snapshot )
        {
            if( IsAuthoritative )
            {
                return;
            }

            NetOutgoingMessage msg = CreateNewDataMessage( DataMessageType.SnapshotACK );
            msg.Write( snapshot.SnapshotNum );
            NetPeer.SendMessage( msg, ServerConnection, NetDeliveryMethod.ReliableUnordered );
            NetPeer.FlushSendQueue();
            lastAckdSnapshot = snapshot.SnapshotNum;
        }


        private void ProcessNetworkMessages()
        {
            // Drain messages from net peer.
            incomingMsgs.Clear();
            netPeer.ReadMessages( incomingMsgs );
            
            for( int i = 0; i < incomingMsgs.Count; ++i )
            {
                if( incomingMsgs[i].MessageType == NetIncomingMessageType.StatusChanged )
                {
                    ProcessStatusChangedMessage( incomingMsgs[i] );
                }
                else if( incomingMsgs[i].MessageType == NetIncomingMessageType.Data )
                {
                    ProcessDataMessage( incomingMsgs[i] );
                }
                else if( incomingMsgs[i].MessageType == NetIncomingMessageType.DebugMessage )
                {
                    ProcessDebugMessage( incomingMsgs[i] );
                }
                else if( incomingMsgs[i].MessageType == NetIncomingMessageType.ConnectionApproval )
                {
                    // TODO: More logic here to determine whether we should approve or not.
                    incomingMsgs[i].SenderConnection.Approve();
                }
                else if( netType == NetType.Server && incomingMsgs[i].MessageType == NetIncomingMessageType.DiscoveryRequest )
                {
                    ContainingWorld.ReadNetDiscoveryRequest( incomingMsgs[i] );
                }
                else if( netType == NetType.Unconnected && incomingMsgs[i].MessageType == NetIncomingMessageType.DiscoveryResponse )
                {
                    ContainingWorld.ReadNetDiscoveryResponse( incomingMsgs[i] );
                }
            }
        }


        private void ProcessStatusChangedMessage( NetIncomingMessage message )
        {
            // Read the new status for this connection.
            NetConnectionStatus status = ( NetConnectionStatus )message.ReadByte();

            if( status == NetConnectionStatus.Connected )
            {
                if( netType == NetType.Server )
                {
                    OnClientConnected( message.SenderConnection );
                }
                else if( netType == NetType.Unconnected )
                {
                    OnConnectToServer( message.SenderConnection );
                    for( int i = 0; i < connectToServerRequests.Count; ++i )
                    {
                        if( connectToServerRequests[i].Connection == message.SenderConnection )
                        {
                            connectToServerRequests[i].Completed( ConnectToServerRequestResult.Connected );
                            connectToServerRequests.RemoveAt( i );
                            --i;
                        }
                    }
                }
            }
            else if( status == NetConnectionStatus.Disconnected )
            {
                if( netType == NetType.Server )
                {
                    OnClientDisconnect( message.SenderConnection );
                }
                else if( netType == NetType.Client )
                {
                    OnDisconnectFromServer( message.SenderConnection );
                }
            }
        }


        private void ProcessDataMessage( NetIncomingMessage msg )
        {
            // Data message, read the 8-bit sub type.
            DataMessageType type = ( DataMessageType )msg.ReadByte();
            
            if( IsAuthoritative )
            {
                if( type == DataMessageType.SnapshotACK )
                {
                    ClientState client = GetClientFromConnection( msg.SenderConnection );
                    if( client != null )
                    {
                        int num         = msg.ReadInt32();
                        if( client.LastAchknowledgedSnapshot < num )
                        {
                            client.LastAchknowledgedSnapshot = num;
                            client.SnapshotBuffer.Acknowledge( num );
                        }
                    }
                }
                else if( type == DataMessageType.ServerCommand )
                {
                    RPCCall call = new RPCCall( msg );
                    serverCommandsToExecute.Add( call );
                }
            }
            if( netType == NetType.Client && type == DataMessageType.Snapshot )
            {
                int num         = msg.ReadInt32();

                // Old snapshot, we don't want it.
                if( num <= lastAckdSnapshot )
                {
                    return;
                }

                int      deltaFrom   = msg.ReadInt32();
                Snapshot last        = receivedSnapshots.GetSnapshotWithNum( deltaFrom );
                Snapshot newSnapshot = new Snapshot( msg, last, num );
                if( last.SnapshotNum <= lastAckdSnapshot )
                {
                    newSnapshot.RemoveRPCsFrom( lastAckdSnapshot );
                }

                ContainingWorld.OnSnapshotArrive( newSnapshot );
                receivedSnapshots.AddSnapshot( newSnapshot );
                SendSnapshotACK( newSnapshot );
                ++receivedSnapshotCount;
            }
        }


        private void ProcessDebugMessage( NetIncomingMessage message )
        {
            string debugMessage = message.ReadString();
            NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Networking, debugMessage );
        }


        private void ProcessSnapshots()
        {
            for( int i = receivedSnapshots.Snapshots.Length - 1; i > 0; --i )
            {
                if( receivedSnapshots.Snapshots[i].Timestamp <= CurrentClientInterpolationTime
                    && receivedSnapshots.Snapshots[i].SnapshotNum > currentClientStep )
                {
                    // This is our new current snapshot. Invoke RPCs.
                    Snapshot newest          = receivedSnapshots.Snapshots[i];
                    currentClientStep = newest.SnapshotNum;
                    for( int c = 0; c < newest.RPCCalls.Count; ++c )
                    {
                        InvokeRPCCall( newest.RPCCalls[c].call );
                    }

                    currentSnapshot = newest;
                    nextSnapshot = receivedSnapshots.Snapshots[i - 1];

                    // Read the snapshot for the current time.
                    newest.Buffer.Position = 0;
                    for( int s = 0; s < newest.ActorStates.Count; ++s )
                    {
                        Actor actor   = ContainingWorld.GetReplicatedActor( newest.ActorStates[s].ActorId );

                        // Read the current state of the world.
                        actor.ReadSnapshot( newest.Buffer, false );
                        newest.Buffer.ReadPadBits();
                    }

                    // Peek the future state of the world for interpolation.
                    for( int s = 0; s < nextSnapshot.ActorStates.Count; ++s )
                    {
                        Actor actor   = ContainingWorld.GetReplicatedActor( nextSnapshot.ActorStates[s].ActorId );
                        if( actor == null )
                        {
                            continue;
                        }

                        nextSnapshot.Buffer.Position = nextSnapshot.ActorStates[s].Start;
                        actor.ReadSnapshot( nextSnapshot.Buffer, true );
                    }

                    // Post snapshot initialisation.
                    for( int a = 0; a < ContainingWorld.LocalActorStart; ++a )
                    {
                        if( ContainingWorld.Actors[a] != null && !ContainingWorld.Actors[a].PostSnapshotInitialised )
                        {
                            ContainingWorld.Actors[a].OnActorInitialisePostSnapshot();
                        }
                    }
                }
            }
        }


        private void InvokeRPCCall( RPCCall call )
        {
            // Find the actor the call is for.
            Actor actor = null;
            actor = ContainingWorld.GetReplicatedActor( call.DestinationActorId );
            Assert.IsTrue( actor != null, "Received RPC but destination actor could not be found. Id: " 
                            + call.DestinationActorId + ", Method Id: " + call.MethodId );

            // Parse parameters.
            object[] parameters = new object[call.Parameters.Count];
            for( int i = 0; i < parameters.Length; ++i )
            {
                if( call.Parameters[i].ParameterId == RPCParameterId.ActorId )
                {
                    parameters[i] = ContainingWorld.GetReplicatedActor( ( UInt16 )call.Parameters[i].Data );
                    Assert.IsTrue( parameters[i] != null, "Received RPC but an actor passed as a parameter could not be found. Actor Id: " 
                                   + call.Parameters[i].Data + ", Method Id: " + call.MethodId );
                    continue;
                }
                if( call.Parameters[i].ParameterId == RPCParameterId.ComponentId )
                {
                    KeyValuePair<UInt16, byte> ids              = ( KeyValuePair<UInt16, byte> )call.Parameters[i].Data;
                    Actor                      componentOwner   = ContainingWorld.GetReplicatedActor( ids.Key );
                    Assert.IsTrue( componentOwner != null, "Received RPC but an actor passed as a parameter could not be found. Actor Id: " 
                                   + call.Parameters[i].Data + ", Method Id: " + call.MethodId );

                    parameters[i] = componentOwner.FindComponent( ids.Value );
                    Assert.IsTrue( parameters[i] != null, "Received RPC but a component passed as a parameter could not be found. Ids: "
                                   + call.Parameters[i].Data + ", Method Id: " + call.MethodId );
                    continue;
                }
                parameters[i] = call.Parameters[i].Data;
            }

            // Use reflection to dynamically invoke the method.
            RPCMethod method = actor.FindRPCMethodById( call.MethodId );
            Assert.IsTrue( method != null, "Received RPC but destination method could not be found. Actor Id: " 
                           + call.DestinationActorId + " Method id:" + call.MethodId );
            method.MethodInfo.Invoke( method.Owner, parameters );
        }


        private ClientState GetClientFromConnection( NetConnection connection )
        {
            return clients.Find( x => x.NetConnection == connection );
        }
    }
}

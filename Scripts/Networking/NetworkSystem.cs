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



    public enum DataMessageType
    {
        Snapshot,
        SnapshotACK,
        ServerCommand,
    }


    public class NetworkSystem : NixinSystem
    {


        // Public:


        public NetworkSystem( World containingWorld, string appIdentifier ) : base( containingWorld )
        {
            netConfig           = new NetPeerConfiguration( appIdentifier );

            receivedSnapshots           = new SnapshotBuffer();
            currentSnapshot             = receivedSnapshots.CurrentSnapshot;
            nextSnapshot                = receivedSnapshots.CurrentSnapshot;

            netPeer = new NetServer( netConfig );
        }


        public void StartServer( int port )
        {
            Assert.IsTrue( !IsConnected );
            netConfig.EnableMessageType( NetIncomingMessageType.DiscoveryRequest );
            netPeer = new NetServer( netConfig );
            netConfig.Port  = port;
            netPeer.Start();

            // Reset all actor ids to match the new one we've generated.
            for( int i = 0; i < ContainingWorld.Actors.Count; ++i )
            {
                if( ContainingWorld.Actors[i] == null )
                {
                    continue;
                }
                ContainingWorld.Actors[i].NetworkOwner = Id;
            }
        }


        public void StartClient()
        {
            Assert.IsTrue( !IsConnected );
            netConfig.EnableMessageType( NetIncomingMessageType.DiscoveryResponse );
            netPeer = new NetClient( netConfig );
            netPeer.Start();

            // Reset all actor ids to match the new one we've generated.
            for( int i = 0; i < ContainingWorld.Actors.Count; ++i )
            {
                if( ContainingWorld.Actors[i] == null )
                {
                    continue;
                }
                ContainingWorld.Actors[i].NetworkOwner = Id;
            }
        }


        public void ConnectToServer( ConnectToServerRequest request )
        {
            Assert.IsTrue( !IsConnected );

            netPeer     = new NetClient( netConfig );
            netPeer.Start();
            request.Connection = netPeer.Connect( request.Address, request.Port );
            connectToServerRequests.Add( request );
        }


        public void DisconnectFromServer()
        {
            Assert.IsTrue( IsConnected && IsClient );
            ( ( NetClient )netPeer ).Disconnect( "Cya" );
        }


        public void Shutdown()
        {
            if( NetPeer != null )
            {
                NetPeer.Shutdown( "Cya" );
                netConfig = new NetPeerConfiguration( NetConfig.AppIdentifier );
            }
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
            foreach( var parameter in parameters )
            {
                var parameterId = RPCCall.GetParameterIdFromData( parameter );
                Assert.IsTrue( parameterId != RPCParameterId.Invalid );

                rpcParameters.Add( new RPCParameter( parameter, parameterId ) );
            }

            // Create the call given the parameters.
            return new RPCCall( destinationActor.Id, method.Id, rpcParameters, reliable, destinationActor.AcceptsNewConnections, networkStep );
        }


        public void AddMulticastRPC( RPCCall call, Actor destination )
        {
            Assert.IsTrue( IsAuthoritative );

            foreach( var relevency in destination.Relevancies )
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

            NetPeer.SendMessage( message, ServerConnection, NetDeliveryMethod.Unreliable );
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

            var destinationActor = ContainingWorld.Actors[call.DestinationActorId];
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

            var destinationActor = ContainingWorld.Actors[call.DestinationActorId];
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

            var destinationActor = ContainingWorld.Actors[component.Owner.Id];
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
            var relevancy = new Relevancy( client, actor, type );
            client.Relevancies.Add( relevancy );
            actor.Relevancies.Add( relevancy );
        }


        public void RemoveRelevancy( ClientState client, Actor actor )
        {
            client.Relevancies.RemoveAll( x => x.Actor == actor );
            actor.Relevancies.RemoveAll( x => x.Client == client );
        }


        public void CheckForSnapshotActivation()
        {
            for( int i = receivedSnapshots.Snapshots.Length - 1; i >= 0; --i )
            {
                if( receivedSnapshots.Snapshots[i].Timestamp <= CurrentClientInterpolationTime && receivedSnapshots.Snapshots[i].SnapshotNum > currentClientStep )
                {
                    // This is our new current snapshot. Invoke RPCs.
                    var newest          = receivedSnapshots.Snapshots[i];
                    currentClientStep = newest.SnapshotNum;
                    newest.RPCCalls.ForEach( x => InvokeRPCCall( x.call ) );

                    currentSnapshot = newest;
                    nextSnapshot = receivedSnapshots.Snapshots[Mathf.Clamp( i - 1, 0, receivedSnapshots.Snapshots.Length - 1 )];

                    // Send out actor states.
                    newest.Buffer.Position       = 0;
                    for( int s = 0; s < newest.ActorStates.Count; ++s )
                    {
                        var actor   = ContainingWorld.GetReplicatedActor( newest.ActorStates[s].ActorId );

                        // Read the current state of the world.
                        actor.ReadSnapshot( newest.Buffer, false );
                        newest.Buffer.ReadPadBits();
                    }
                    for( int s = 0; s < nextSnapshot.ActorStates.Count; ++s )
                    {
                        var actor   = ContainingWorld.GetReplicatedActor( nextSnapshot.ActorStates[s].ActorId );
                        if( actor == null )
                        {
                            continue;
                        }

                        nextSnapshot.Buffer.Position = nextSnapshot.ActorStates[s].Start;

                        // Peek the future state of the world for interpolation.
                        actor.ReadSnapshot( nextSnapshot.Buffer, true );
                    }

                    // Call post snapshot initialise.
                    for( int a = 0; a < ContainingWorld.LocalActorStart; ++a )
                    {
                        if( ContainingWorld.Actors[a] != null && !ContainingWorld.Actors[a].PostSnapshotInitialised )
                        {
                            ContainingWorld.Actors[a].OnActorInitialisePostSnapshot();
                        }
                    }
                    // NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Networking, Time.time +  " Snapshot activated " + ( CurrentClientInterpolationTime - receivedSnapshots.Snapshots[i].Timestamp ) + "ms late" );
                }
            }
        }


        public void ExecuteServerCommands()
        {
            for( int i = 0; i < serverCommandsToExecute.Count; ++i )
            {
                InvokeRPCCall( serverCommandsToExecute[i] );
            }
            serverCommandsToExecute.Clear();
        }


        public void ReadUpdate()
        {
            if( NetPeer == null )
            {
                return;
            }
            ProcessNetworkMessages();
        }


        public void SendUpdate()
        {
            if( NetPeer == null )
            {
                return;
            }

            // Copy the world to each client's snapshot.
            for( int i = 0; i < clients.Count; ++i )
            {
                clients[i].SnapshotBuffer.CurrentSnapshot.CopyClientWorldState( clients[i], ContainingWorld.LocalActorStart );
                SendSnapshot( clients[i] );
                clients[i].SnapshotBuffer.NewSnapshot( networkStep + 1 );
            }

            ++networkStep;
        }


        public NetOutgoingMessage CreateNewDataMessage( DataMessageType messageType )
        {
            Assert.IsTrue( netPeer != null && netPeer.Status == NetPeerStatus.Running );

            var message = netPeer.CreateMessage();
            message.Write( ( byte )messageType );

            return message;
        }


        public ClientState GetClientStateFromId( long id )
        {
            if( NetPeer == null )
            {
                return null;
            }
            return clients.Find( x => x.Id == id );
        }


        public double CurrentClientInterpolationTime
        {
            get
            {
                // The current time the client is running at.
                return ( NetTime.Now - ( interpolationBufferLegnth ) * NetworkSendPeriod ) - ( ServerConnection == null ? 0.0 : ServerConnection.AverageRoundtripTime / 2.0 );
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
                if( !IsServer )
                {
                    return ( ( NetClient )NetPeer ).ServerConnection;
                }
                return null;
            }
        }


        public long ServerId
        {
            get
            {
                if( NetPeer == null || ( IsClient && !IsConnected ) )
                {
                    return 0;
                }
                return IsServer ? NetPeer.UniqueIdentifier : ( ( NetClient )NetPeer ).ServerConnection.RemoteUniqueIdentifier;
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


        public bool IsConnected
        {
            get
            {
                return NetPeer.ConnectionsCount > 0;
            }
        }


        public bool IsAuthoritative
        {
            get
            {
                return NetPeer == null || NetPeer.Status != NetPeerStatus.Running || !IsConnected || NetPeer is NetServer || performingPreConnection;
            }
        }


        public bool IsServer
        {
            get
            {
                return NetPeer is NetServer;
            }
        }


        public bool IsClient
        {
            get
            {
                return !IsServer;
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


        private NetPeer                     netPeer     = null;
        private NetPeerConfiguration        netConfig   = null;

        private bool                        performingPreConnection         = false;
        private double                      networkSendRate                 = 20.0;
        private int                         interpolationBufferLegnth       = 2;

        private List<RPCCall>               bufferedRPCCalls                = new List<RPCCall>();

        private List<ClientState>           clients                         = new List<ClientState>();

        private int                         networkStep                     = 0;
        private int                         currentClientStep               = -1;

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
            // Create the new client state.
            var client = new ClientState( connection );
            clients.Add( client );

            CreateInitialSnapshot( client );
            ContainingWorld.OnClientConnected( client );
        }


        private void OnClientDisconnect( NetConnection connection )
        {
            var client = GetClientFromConnection( connection );

            for( int i = 0; i < ContainingWorld.LocalActorStart; ++i )
            {
                if( ContainingWorld.Actors[i] != null )
                {
                    RemoveRelevancy( client, ContainingWorld.Actors[i] );
                }
            }
            ContainingWorld.OnClientDisconnect( client );

            // Remove the client state. It's only valid for this session.
            clients.RemoveAll( x => x.NetConnection == connection );
        }


        private void OnPreConnectToServer( NetConnection serverConnection )
        {
            ContainingWorld.OnPreConnectToServer( serverConnection );
        }


        private void OnConnectToServer( NetConnection serverConnection )
        {
            ContainingWorld.OnConnectToServer( serverConnection );
        }


        private void OnDisconnectFromServer( NetConnection serverConnection )
        {
            ContainingWorld.OnDisconnectFromServer( serverConnection );
        }


        private void OnStartRunning( NetConnection serverConnection )
        {

        }


        private void CreateInitialSnapshot( ClientState client )
        {
            if( IsClient )
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
            //CreateRelevancy( client, ContainingWorld, RelevancyType.Active );
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
            if( IsClient )
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
                else if( IsServer && incomingMsgs[i].MessageType == NetIncomingMessageType.ConnectionApproval )
                {
                    incomingMsgs[i].SenderConnection.Approve();
                }
                else if( incomingMsgs[i].MessageType == NetIncomingMessageType.DiscoveryRequest )
                {
                    ContainingWorld.ReadNetDiscoveryRequest( incomingMsgs[i] );
                }
                else if( incomingMsgs[i].MessageType == NetIncomingMessageType.DiscoveryResponse )
                {
                    ContainingWorld.ReadNetDiscoveryResponse( incomingMsgs[i] );
                }
            }
        }


        private void ProcessStatusChangedMessage( NetIncomingMessage message )
        {
            // Read the new status for this connection.
            var status = ( NetConnectionStatus )message.ReadByte();

            if( status == NetConnectionStatus.Connected )
            {
                if( IsServer )
                {
                    OnClientConnected( message.SenderConnection );
                }
                else
                {
                    // Allow the client to respond as an authority one more time.
                    performingPreConnection = true;
                    OnPreConnectToServer( message.SenderConnection );
                    performingPreConnection = false;

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
                if( IsServer )
                {
                    OnClientDisconnect( message.SenderConnection );
                }
                else
                {
                    OnDisconnectFromServer( message.SenderConnection );
                }
            }
        }


        private void ProcessDataMessage( NetIncomingMessage msg )
        {
            // Data message, read the 8-bit sub type.
            var type = ( DataMessageType )msg.ReadByte();
            
            if( IsAuthoritative )
            {
                if( type == DataMessageType.SnapshotACK )
                {
                    var client = GetClientFromConnection( msg.SenderConnection );
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
                    var call = new RPCCall( msg );
                    serverCommandsToExecute.Add( call );
                }
            }
            if( IsClient && type == DataMessageType.Snapshot )
            {
                int num         = msg.ReadInt32();

                // Old snapshot, we don't want it.
                if( num <= lastAckdSnapshot )
                {
                    return;
                }

                int deltaFrom   = msg.ReadInt32();
                var last        = receivedSnapshots.GetSnapshotWithNum( deltaFrom );
                var newSnapshot = new Snapshot( msg, last, num );
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


        private void InvokeRPCCall( RPCCall call )
        {
            if( ContainingWorld == null )
            {
                return;
            }

            // Find the actor the call is for.
            Actor actor = null;
            actor = ContainingWorld.GetReplicatedActor( call.DestinationActorId );
            if( actor == null )
            {
                throw new InvalidRPCCallException( "Received RPC but destination actor could not be found. Id: " + call.DestinationActorId + ", Method Id: " + call.MethodId );
            }

            // Parse parameters.
            object[] parameters = new object[call.Parameters.Count];
            for( int i = 0; i < parameters.Length; ++i )
            {
                if( call.Parameters[i].ParameterId == RPCParameterId.ActorId )
                {
                    parameters[i] = ContainingWorld.GetReplicatedActor( ( UInt16 )call.Parameters[i].Data );
                    if( parameters[i] == null )
                    {
                        throw new InvalidRPCCallException( "Received RPC but an actor passed as a parameter could not be found. Actor Id: " + call.Parameters[i].Data + ", Method Id: " + call.MethodId );
                    }
                    continue;
                }
                if( call.Parameters[i].ParameterId == RPCParameterId.ComponentId )
                {
                    var ids              = ( KeyValuePair<UInt16, byte> )call.Parameters[i].Data;
                    var componentOwner   = ContainingWorld.GetReplicatedActor( ids.Key );
                    if( componentOwner == null )
                    {
                        throw new InvalidRPCCallException( "Received RPC but an actor passed as a parameter could not be found. Actor Id: " + call.Parameters[i].Data + ", Method Id: " + call.MethodId );
                    }

                    parameters[i]       = componentOwner.FindComponent( ids.Value );
                    if( parameters[i] == null )
                    {
                        throw new InvalidRPCCallException( "Received RPC but a component passed as a parameter could not be found. Ids: " + call.Parameters[i].Data + ", Method Id: " + call.MethodId );
                    }
                    continue;
                }
                parameters[i] = call.Parameters[i].Data;
            }

            // Use reflection to dynamically invoke the method.
            var method = actor.FindRPCMethodById( call.MethodId );
            if( method == null )
            {
                throw new InvalidRPCCallException( "Received RPC but destination method could not be found. Method id:" + call.MethodId );
            }
            method.MethodInfo.Invoke( method.Owner, parameters );
        }


        private ClientState GetClientFromConnection( NetConnection connection )
        {
            return clients.Find( x => x.NetConnection == connection );
        }
    }
}

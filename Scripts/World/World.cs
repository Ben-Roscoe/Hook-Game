using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lidgren.Network;
using System.Reflection;
using System;
using System.Timers;
using System.Threading;
using System.IO;
using MicroLibrary;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Nixin
{
    public abstract class World : Actor
    {



        // Public:
        

        public bool                    useTestData                  = false;

        public GameManager             TEST_managerPrefab           = null;
        public LocalGameManager        TEST_localManagerPrefab      = null;
        public GameState               TEST_statePrefab             = null;
        public StatsBase               TEST_statsPrefab             = null;
        public HudBase                 TEST_hudPrefab               = null;
        public GameModeMapExtension    TEST_extensionPrefab         = null;


        public virtual void LoadMap( GameMap next )
        {
            Assert.IsTrue( !IsLoadingMap && next.IsValid() );

            // Let's create the new resource batch.
            GameMapBatchLoader nextBatch = new GameMapBatchLoader( resourceSystem, next.Map, next.Mode );

            currentMapTransition    = new MapTransition( currentGameMap, next, mapResourceBatch, nextBatch );
            currentMapTransition.AddProgressCallback( TransitionMapLoaderProgressCallback );
            currentMapTransition.MakeTransition();

            OnMapBeginLoad( currentMapTransition ); 

            // Send load message to clients.
            if( NetworkSystem != null && NetworkSystem.IsAuthoritative )
            {
                // TODO: Make proper hash.
                latestLoadMapCall = RPC( MulticastLoadMap, RPCType.Multicast, this, next.Map.name, 
                    currentMapTransition.NextBatch.MapChunk.name,
                     next.Mode.name, currentMapTransition.NextBatch.GameModeChunk.name );
                for( int i = 0; i < NetworkSystem.Clients.Count; ++i )
                {
                    NetworkSystem.Clients[i].IsLoadingCurrentMap = true;
                    for( int r = 1; r < NetworkSystem.Clients[i].Relevancies.Count; ++r )
                    {
                        NetworkSystem.RemoveRelevancy( NetworkSystem.Clients[i].Relevancies[r].Client,
                            NetworkSystem.Clients[i].Relevancies[r].Actor );
                        --r;
                    }
                }
            }
        }


        public void RestartGameMap()
        {
            if( !NetworkSystem.IsAuthoritative || IsLoadingMap )
            {
                return;
            }
            LoadMap( currentGameMap );
        }


        public virtual void OnSnapshotArrive( Snapshot snapshot )
        {
            ExpandReplicatedActors( snapshot.ActorListSize );
        }


        public virtual void OnWorldApplicationQuit()
        {
            worldInputComponent.Uninitialise();
            if( NetworkSystem != null )
            {
                NetworkSystem.Shutdown();
            }
        }


        public virtual void OnWorldLocalisationChanged()
        {
            for( int i = 0; i < actors.Count; ++i )
            {
                if( actors[i] != null )
                {
                    actors[i].OnLocalisationChanged();
                }
            }
        }


        public virtual void ReadNetDiscoveryResponse( NetIncomingMessage msg )
        {
            if( LocalGameManager != null )
            {
                LocalGameManager.ReadNetDiscoveryResponse( msg );
            }
        }


        public virtual void ReadNetDiscoveryRequest( NetIncomingMessage msg )
        {
            if( GameManager != null )
            {
                NetBuffer          response         = NetworkSystem.NetPeer.CreateMessage( 0 );
                int                initialSize      = response.LengthBits;
                GameManager.HandleNetDiscoveryRequest( msg, response );

                // Send a response if the message is actually needed.
                if( response.LengthBits > initialSize )
                {
                    NetOutgoingMessage responseMsg = NetworkSystem.NetPeer.CreateMessage( response.LengthBytes );
                    responseMsg.Write( response.Data );
                    NetworkSystem.SendNetworkDiscoveryResponse( responseMsg, msg.SenderEndPoint );
                }
            }
        }


        public virtual void OnConnected( NetConnection connection )
        {
            if( networkSystem.CurrentNetType == NetType.Server )
            {
                ClientState client = networkSystem.GetClientStateFromConnection( connection );
                client.SnapshotBuffer.CurrentSnapshot.AddRPC( latestLoadMapCall );
            }
            else if( networkSystem.CurrentNetType == NetType.Client )
            {
                BecomeClient();
            }
        }


        public virtual void OnDisconnected( NetConnection connection )
        {
            // TODO: Stop network timer.
            if( networkSystem.CurrentNetType == NetType.Server )
            {
                ClientState client = networkSystem.GetClientStateFromConnection( connection );

                if( GameManager != null )
                {
                    GameManager.OnClientDisconnect( client );
                }
                pendingClientLoadResults.RemoveAll( x => x.Client == client );
                DestroyReplicatedActorsWithOwner( client );
            }
            else if( networkSystem.CurrentNetType == NetType.Client )
            {
                BecomeServer();
            }
        }


        public virtual void OnPreConnectToServer( NetConnection serverConnection )
        {
            if( LocalGameManager != null )
            {
                LocalGameManager.OnConnectToServer();
            }
        }


        public Actor GetReplicatedActor( UInt16 index )
        {
            return actors[index];
        }


        public GameState CreateGameState( GameState newGameState )
        {
            if( !IsAuthority )
            {
                return null;
            }

            Assert.IsTrue( gameState == null );

            gameState = ( GameState )InstantiateReplicatedActor( newGameState, Vector3.zero, Quaternion.identity, NetworkSystem.Id, true, null );
            return gameState;
        }


        public GameModeMapExtension CreateGameModeMapExtension( GameModeMapExtension newGameModeMapExtensionPrefab )
        {
            if( !IsAuthority )
            {
                return null;
            }

            Assert.IsTrue( gameModeMapExtension == null );

            gameModeMapExtension = ( GameModeMapExtension )InstantiateReplicatedActor( newGameModeMapExtensionPrefab, Vector3.zero, Quaternion.identity, NetworkSystem.Id, true, null );
            return gameModeMapExtension;
        }


        public UIActor InstantiateUIActor( UIActor prefab, UICanvasActor canvas )
        {
            return InstantiateUIActor( prefab, canvas, Vector3.zero, Quaternion.identity );
        }


        public UIActor InstantiateUIActor( UIActor prefab, UICanvasActor canvas, Vector3 position, Quaternion rotation )
        {
            var actor = ( UIActor )InstantiateActorLocally( prefab, position, rotation, false, 0 );
            if( canvas != null )
            {
                actor.transform.SetParent( canvas.transform, false );
            }
            InitialiseActorHierarchy( actor, false, NetworkSystem.Id, false, null, null, 0 );
            SetLocalTextUIActorRecursive( actor );
            return actor;
        }


        public Actor InstantiateLocalActor( Actor prefab, Controller responsibleController )
        {
            return InstantiateLocalActor( prefab, Vector3.zero, Quaternion.identity, responsibleController );
        }


        public Actor InstantiateLocalActor( Actor prefab, Vector3 position, Quaternion rotation, 
            Controller responsibleController )
        {
            Actor actor         = InstantiateActorLocally( prefab, position, rotation, false, 0 );
            InitialiseActorHierarchy( actor, false, NetworkSystem.Id, false, responsibleController,
                responsibleController == null ? null : responsibleController.Stats, 0 );
            return actor;
        }


        public Actor InstantiateReplicatedActor( Actor prefab, Vector3 position, Quaternion rotation, long owner,
            bool acceptsNewConnections, Controller responsibleController )
        {
            return InstantiateReplicatedActor( prefab, position, rotation, owner, acceptsNewConnections, NetworkSystem.Clients, responsibleController );
        }


        public Actor InstantiateReplicatedActor( Actor prefab, Vector3 position, Quaternion rotation, long owner,
            bool acceptsNewConnections, List<ClientState> relevantClients, Controller responsibleController )
        {
            Assert.IsTrue( NetworkSystem.IsAuthoritative );

            // Find a suitable index for this actor. Try and find an empty slot if possible.
            UInt16                    index       = localActorsStart;
            for( UInt16 i = 0; i < localActorsStart; ++i )
            {
                if( actors[i] == null )
                {
                    index = i;
                    break;
                }
            }

            Actor                  newActor    = InstantiateActorLocally( prefab, position, rotation, true, index );
            for( int i = 0; i < relevantClients.Count; ++i )
            {
                NetworkSystem.CreateRelevancy( relevantClients[i], newActor, 
                    relevantClients[i].IsLoadingCurrentMap ? RelevancyType.Deactive : RelevancyType.Active );
            }

            newActor.ContainingWorld = this;
            newActor.Id              = index;


            if( acceptsNewConnections )
            {
                // Should be no previous call.
                bufferedInstantiateCalls.Add( new InstantiateCall( newActor, RPC( MulticastInstantiateTargetedActor,
                    RPCType.Multicast, this, index, prefab.name, position, rotation, owner,
                    responsibleController == null ? null : responsibleController.ResponsibleStats ) ) );
            }
            else
            {
                RPC( MulticastInstantiateTargetedActor, RPCType.Multicast, this, index, prefab.name, position,
                    rotation, owner, responsibleController == null ? null : responsibleController.ResponsibleStats );
            }


            InitialiseActorHierarchy( newActor, true, owner, acceptsNewConnections, responsibleController,
                responsibleController == null ? null : responsibleController.Stats, index );

            return newActor;
        }


        public void DestroyActor( Actor actor )
        {
            if( actor == null )
            {
                return;
            }

            // This actor is pooled, delegate it's destruction to the pool.
            if( actor.Pool != null )
            {
                actor.Pool.FreeActor( actor, false );
                return;
            }

            if( !actor.Replicates )
            {
                DestroyActorLocallyRecursive( actor.transform, true );
            }
            else if( NetworkSystem.IsAuthoritative )
            {
                RPC( MulticastDestroyTargetedActor, RPCType.Multicast, this, actor );

                if( actor.AcceptsNewConnections )
                {
                    // Remove instantiate buffered call.
                    bufferedInstantiateCalls.RemoveAll( x => x.Actor == actor );
                    NetworkSystem.UnbufferRPCCallForActor( actor );
                }
                var relevacies = new List<Relevancy>( actor.Relevancies );
                relevacies.ForEach( x => NetworkSystem.RemoveRelevancy( x.Client, x.Actor ) );

                DestroyActorLocallyRecursive( actor.transform, true );
            }
        }


        public RPCCall RPC( RPCMethodDelegate rpcMethod, RPCType rpcType, Actor destination )
        {
            return ParseRPC( rpcMethod.Method, rpcType, destination, () => { rpcMethod(); } );
        }


        public RPCCall RPC<A>( RPCMethodDelegate<A> rpcMethod, RPCType rpcType, Actor destination, A a )
        {
            return ParseRPC( rpcMethod.Method, rpcType, destination, () => { rpcMethod( a ); }, a );
        }


        public RPCCall RPC<A, B>( RPCMethodDelegate<A, B> rpcMethod, RPCType rpcType, Actor destination, A a, B b )
        {
            return ParseRPC( rpcMethod.Method, rpcType, destination, () => { rpcMethod( a, b ); }, a, b );
        }


        public RPCCall RPC<A, B, C>( RPCMethodDelegate<A, B, C> rpcMethod, RPCType rpcType, Actor destination, A a, B b, C c )
        {
            return ParseRPC( rpcMethod.Method, rpcType, destination, () => { rpcMethod( a, b, c ); }, a, b, c );
        }


        public RPCCall RPC<A, B, C, D>( RPCMethodDelegate<A, B, C, D> rpcMethod, RPCType rpcType, Actor destination, A a, B b, C c, D d )
        {
            return ParseRPC( rpcMethod.Method, rpcType, destination, () => { rpcMethod( a, b, c, d ); }, a, b, c, d );
        }


        public RPCCall RPC<A, B, C, D, E>( RPCMethodDelegate<A, B, C, D, E> rpcMethod, RPCType rpcType, Actor destination, A a, B b, C c, D d, E e )
        {
            return ParseRPC( rpcMethod.Method, rpcType, destination, () => { rpcMethod( a, b, c, d, e ); }, a, b, c, d, e );
        }


        public RPCCall RPC<A, B, C, D, E, F>( RPCMethodDelegate<A, B, C, D, E, F> rpcMethod, RPCType rpcType, Actor destination, A a, B b, C c, D d, E e, F f )
        {
            return ParseRPC( rpcMethod.Method, rpcType, destination, () => { rpcMethod( a, b, c, d, e, f ); }, a, b, c, d, e, f );
        }


        public override void WriteSnapshot( NetBuffer buffer )
        {
            base.WriteSnapshot( buffer );

            buffer.WriteActor( gameState );
            buffer.WriteActor( gameModeMapExtension );
        }


        public override void ReadSnapshot( NetBuffer buffer, bool isFuture )
        {
            base.ReadSnapshot( buffer, isFuture );

            gameState            = buffer.ReadActor<GameState>( this, gameState, isFuture );
            gameModeMapExtension = buffer.ReadActor<GameModeMapExtension>( this, gameModeMapExtension, isFuture );
        }


        // Game specific values.
        public abstract string AppId                    { get; }
        public abstract string DataPath                 { get; }
        public abstract string LocalisationPath         { get; }


        public ConsoleSystem ConsoleSystem
        {
            get
            {
                return consoleSystem;
            }
        }


        public NetworkSystem NetworkSystem
        {
            get
            {
                return networkSystem;
            }
        }


        public TimerSystem TimerSystem
        {
            get
            {
                return timerSystem;
            }
        }


        public ResourceSystem ResourceSystem
        {
            get
            {
                return resourceSystem;
            }
        }


        public LocalisationSystem LocalisationSystem
        {
            get
            {
                return localisationSystem;
            }
        }


        public QuerySystem QuerySystem
        {
            get
            {
                return querySystem;
            }
        }


        public UICanvasActor DefaultCanvas
        {
            get
            {
                return defaultCanvas;
            }
        }


        public EventSystem EventSystem
        {
            get
            {
                return eventSystem;
            }
        }


        public List<Actor> Actors
        {
            get
            {
                return actors;
            }
        }


        public bool IsLoadingMap
        {
            get
            {
                return currentMapTransition != null && ( currentMapTransition.Status == MapTransitionStatus.LoadingNextBundles || currentMapTransition.Status == MapTransitionStatus.LoadingNextScene );
            }
        }


        public GameMode CurrentGameModeResource
        {
            get
            {
                return currentGameMode;
            }
        }


        public GameManager GameManager
        {
            get
            {
                return gameManager;
            }
        }


        public LocalGameManager LocalGameManager
        {
            get
            {
                return localGameManager;
            }
        }


        public GameState GameState
        {
            get
            {
                return gameState;
            }
        }


        public UpdateGroup DeltaUpdateGroup
        {
            get
            {
                return deltaUpdateGroup;
            }
        }


        public UpdateGroup FixedUpdateGroup
        {
            get
            {
                return fixedUpdateGroup;
            }
        }


        public bool IsDedicatedServer
        {
            get
            {
                return false;
            }
        }


        public InputComponent WorldInputComponent
        {
            get
            {
                return worldInputComponent;
            }
        }


        public UInt16 LocalActorStart
        {
            get
            {
                return localActorsStart;
            }
        }


        public Player FirstLocalPlayer
        {
            get
            {
                for( int i = 0; i < actors.Count; ++i )
                {
                    var player = actors[i] as Player;
                    if( player != null && player.IsLocalPlayer )
                    {
                        return player;
                    }
                }
                return null;
            }
        }


        public NixinEvent<GameState> OnGameStateCreated
        {
            get
            {
                return onGameStateCreated;
            }
        }


        public NixinEvent<bool> OnApplicationFocusChanged
        {
            get
            {
                return onApplicationFocusChanged;
            }
        }


        // Protected:


        protected virtual void WorldInitialise()
        {
            Assert.raiseExceptions = true;
            Assert.IsTrue( eventSystemPrefab != null );
            Assert.IsTrue( defaultCanvasPrefab != null );

            // World object should persist throughout the entire game.
            DontDestroyOnLoad( gameObject );

            int port = 0;
#if !NSHIPPING
            if( Application.isEditor )
            {
                port = 7000;
            }
            else
#endif
            {
                string[] args = System.Environment.GetCommandLineArgs();
                for( int i = 1; i < args.Length; i += 2 )
                {
                    if( args[i] == "+nport" )
                    {
                        port = int.Parse( args[i + 1] );
                        break;
                    }
                }
            }


            // Start systems.
            consoleSystem       = new ConsoleSystem( this );
            networkSystem       = new NetworkSystem( this, AppId, port );
            timerSystem         = new TimerSystem( this );
            resourceSystem      = new ResourceSystem( this, DataPath );
            localisationSystem  = new LocalisationSystem( this, LocalisationPath );
            querySystem         = new QuerySystem( this );

            ContainingWorld = this;
            replicates      = true;

            worldInputComponent = new InputComponent( this );

            // RPCs for instantiating and destroying remotely.
            RegisterRPC<UInt16, string, Vector3, Quaternion, long, StatsBase>( MulticastInstantiateTargetedActor );
            RegisterRPC<Actor>( MulticastDestroyTargetedActor );

            // Map loading RPCs.
            RegisterRPC<long, string, string, string, string>( ServerAcknowledgeClientLoad );
            RegisterRPC<string, string, string, string>( MulticastLoadMap );

            OnActorInitialise( replicates, 0, true, null );
            InsertReplicatedActor( this, 0 );

            if( useTestData )
            {
                currentGameMode = new GameMode( TEST_managerPrefab, TEST_localManagerPrefab, TEST_statePrefab, TEST_statsPrefab, TEST_hudPrefab, TEST_extensionPrefab );
                InitialiseMap();
            }

            worldInputComponent.BindAction( "ToggleConsole", InputState.Down, ToggleConsole, this );
            WorldInputComponent.BindAction( "DebugBreak", InputState.Down, ActivateDebugBreak, this );
        }


        protected virtual void WorldUpdate()
        {
            networkSystem.ReadUpdate();
            networkSystem.Update();

            // Do world update.
            WorldInputComponent.UpdateInput( Time.deltaTime );
            deltaUpdateGroup.Update( Time.deltaTime );
            TimerSystem.Update();

            // Cache unity values.
            for( int i = 0; i < actors.Count; ++i )
            {
                if( actors[i] == null )
                {
                    continue;
                }
                actors[i].OnActorCacheUnityValues();
            }

            // Check for a new selected UI actor.
            if( eventSystem != null && eventSystem.currentSelectedGameObject != lastFocusedUIObject )
            {
                lastFocusedUIObject = eventSystem.currentSelectedGameObject;
                if( lastFocusedUIObject != null )
                {
                    lastFocusedUIActor      = lastFocusedUIObject.GetComponent<UIActor>();
                }
            }
            if( lastFocusedUIActor != null )
            {
                lastFocusedUIActor.UpdateInput( Time.deltaTime );
            }

            NetworkSystem.TrySendUpdate( Time.deltaTime );
        }


        protected virtual void WorldFixedUpdate()
        {
            networkSystem.ReadUpdate();
            networkSystem.Update();

            // Do world fixed update.
            fixedUpdateGroup.UpdateFixed();

            // Cache unity values.
            for( int i = 0; i < actors.Count; ++i )
            {
                if( actors[i] == null )
                {
                    continue;
                }
                actors[i].OnActorCacheUnityValues();
            }
        }


        protected virtual void OnMapBeginLoad( MapTransition transition )
        {
        }


        protected virtual void OnMapLoadUpdate( MapTransition transition, float progress )
        {
        }


        protected virtual void OnMapFinishLoad( MapTransition transition )
        {
            currentGameMode = new GameMode( transition.NextGameMap.Mode, transition.NextGameMap.Map );
            currentMap      = new Map();

            currentGameMap  = transition.NextGameMap;

            mapResourceBatch = transition.NextBatch;

            InitialiseMap();

            // Send load acknowledgement.
            if( !NetworkSystem.IsAuthoritative )
            {
                SendMapLoadAcknowledgementToServer();
            }
            else
            {
                pendingClientLoadResults.ForEach( x => ProcessClientLoadResult( x ) );
                pendingClientLoadResults.Clear();
            }
        }


        protected virtual void InitialiseLocalMap()
        {
            // Set up the local game manager.
            Assert.IsTrue( currentGameMode != null );
            if( currentGameMode.LocalManagerPrefab != null )
            {
                localGameManager = ( LocalGameManager )InstantiateActorLocally( currentGameMode.LocalManagerPrefab, 
                    Vector3.zero, Quaternion.identity, false, 0 );
                InitialiseActorHierarchy( LocalGameManager, false, NetworkSystem.Id, false, null, null, 0 );
            }
        }


        protected virtual void InitialiseReplicatedMap()
        {
            Assert.IsTrue( currentGameMode != null );
            gameManager = InstantiateLocalActor( currentGameMode.ManagerPrefab, Vector3.zero, Quaternion.identity, null ) as GameManager;
            gameManager.InitialiseGameManager( currentGameMap == null ? new List<GameVar>() : currentGameMap.Vars );
        }


        protected virtual void BecomeClient()
        {
            // Destroy the game manager, since the server is now in control of the game.
            if( gameManager != null )
            {
                DestroyActor( gameManager );
            }

            DestroyAllReplicatedActors();
        }


        protected virtual void BecomeServer()
        {
            BecomeClient();
            gameState = null;
            InitialiseReplicatedMap();
        }


        // Private:


        private class ClientGameMapLoadResult
        {
            public ClientState      Client          { get; set; }
            public string           MapName         { get; set; }
            public Hash128          MapHash         { get; set; }
            public string           GameModeName    { get; set; }
            public Hash128           GameModeHash    { get; set; }       
            
            
            public ClientGameMapLoadResult( ClientState client, string mapName, Hash128 mapHash, string gameModeName, Hash128 gameModeHash )
            {
                this.Client         = client;
                this.MapName        = mapName;
                this.MapHash        = mapHash;
                this.GameModeName   = gameModeName;
                this.GameModeHash   = gameModeHash;
            }       
        }


        [SerializeField, FormerlySerializedAs( "EventSystemPrefab" )]
        private EventSystem                 eventSystemPrefab           = null;

        [SerializeField, FormerlySerializedAs( "DefaultCanvasPrefab" )]
        private UICanvasActor               defaultCanvasPrefab         = null;

        [SerializeField, FormerlySerializedAs( "ConsoleUIPrefab" )]
        private ConsoleUI                   consoleUIPrefab             = null;

        private List<Actor>                 actors                      = new List<Actor>();
        private UInt16                      localActorsStart            = 0;

        private UpdateGroup                 deltaUpdateGroup            = new UpdateGroup();
        private UpdateGroup                 fixedUpdateGroup            = new UpdateGroup();

        private GameManager                 gameManager                 = null;
        private LocalGameManager            localGameManager            = null;
        private GameState                   gameState                   = null;
        private GameModeMapExtension        gameModeMapExtension        = null;

        private UInt32                      nextActorId                 = 1;

        private ConsoleSystem               consoleSystem               = null;
        private NetworkSystem               networkSystem               = null;
        private TimerSystem                 timerSystem                 = null;
        private ResourceSystem              resourceSystem              = null;
        private LocalisationSystem          localisationSystem          = null;
        private QuerySystem                 querySystem                 = null;

        private GameMode                    currentGameMode             = null;
        private Map                         currentMap                  = null;

        private GameMap                     currentGameMap              = null;
        private MapTransition               currentMapTransition        = null;

        private GameMapBatchLoader          mapResourceBatch            = null;

        private List<ClientGameMapLoadResult>   pendingClientLoadResults = new List<ClientGameMapLoadResult>();

        private EventSystem                 eventSystem                 = null;
        private UICanvasActor               defaultCanvas               = null;
        private ConsoleUI                   consoleUI                   = null;

        private GameObject                  lastFocusedUIObject         = null;
        private UIActor                     lastFocusedUIActor          = null;

        private InputComponent              worldInputComponent         = null;

        private NixinEvent<GameState>       onGameStateCreated          = new NixinEvent<GameState>();
        private NixinEvent<bool>            onApplicationFocusChanged   = new NixinEvent<bool>();



        private void TransitionMapLoaderProgressCallback( MapTransition transition, float progress )
        {
            if( transition.Status == MapTransitionStatus.ClearingLastScene )
            {
                ClearMap();
            }
            if( transition.Status == MapTransitionStatus.Complete || transition.Status == MapTransitionStatus.Failed )
            {
                OnMapFinishLoad( transition );
                return;
            }
            OnMapLoadUpdate( transition, progress );
        }


        private RPCCall ParseRPC( MethodInfo rpcMethod, RPCType rpcType, Actor destinationActor, Action localCall, params object[] parameters )
        {
            // Check our destination object will be valid. This will be the object that receives the call.
            if( destinationActor == null )
            {
                throw new ArgumentNullException( "RPC destination object cannot be null." );
            }
            if( !destinationActor.Replicates )
            {
                throw new ArgumentException( "RPC destination object must replicate." );
            }

            RPCMethod method = destinationActor.FindRPCMethodByMethodInfo( rpcMethod );

            // Is this call valid for the type of peer we are?
            var    callType                     = RPCCall.GetCallType( rpcType, NetworkSystem.IsAuthoritative, destinationActor.IsAuthority );
            if( callType == RPCCallType.InvalidCall )
            {
                throw new InvalidRPCCallException( "Call is invalid. This could be a client attempting to multicast. Method Name: " + rpcMethod.Name );
            }

            // Create the call. Use a lambda to make the local call to avoid reflection.
            var    rpcCall    = NetworkSystem.CreateRPCCall( destinationActor, method, true, parameters );
            if( callType == RPCCallType.Local )
            {
                localCall();
            }
            else if( callType == RPCCallType.Multicast )
            {
                NetworkSystem.AddMulticastRPC( rpcCall, destinationActor );
            }
            else if( callType == RPCCallType.Server )
            {
                NetworkSystem.SendServerRPC( rpcCall, true );
            }
            return rpcCall;
        }


        private void DestroyReplicatedActorsWithOwner( ClientState owner )
        {
            DestroyReplicatedActorsWithOwner( owner.Id );
        }


        private void DestroyReplicatedActorsWithOwner( long id )
        {
            // Find every actor this connection owns and destroy it.
            for( int i = 0; i < localActorsStart; ++i )
            {
                if( actors[i] != null && actors[i].NetworkOwner == id )
                {
                    DestroyActor( actors[i] );
                }
            }
        }


        private void DestroyAllReplicatedActors()
        {
            // Start at 0 so we don't destroy the world.
            for( int i = 1; i < localActorsStart; ++i )
            {
                if( actors[i] != null )
                {
                    DestroyActor( actors[i] );
                }
            }
        }


        private void Awake()
        {
            WorldInitialise();
        }


        private void Update()
        {
            WorldUpdate();
        }


        private void FixedUpdate()
        {
            WorldFixedUpdate();
        }


        private void OnApplicationQuit()
        {
            OnWorldApplicationQuit();
        }


        private void OnApplicationFocus( bool focusStatus )
        {
            onApplicationFocusChanged.Invoke( focusStatus );
        }


        private Actor InstantiateActorLocally( Actor prefab, Vector3 position, Quaternion rotation, bool newActorReplicates, UInt16 index )
        {
            var newGameObject = GameObject.Instantiate( prefab.gameObject, position, rotation ) as GameObject;
            if( newGameObject == null )
            {
                NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Gameplay, "Failed to instantiate local actor." );
                return null;
            }

            var newActor     = newGameObject.GetComponent<Actor>();
            Assert.IsTrue( newActor != null );

            return newActor;
        }


        private void InsertReplicatedActor( Actor actor, UInt16 index )
        {
            if( index >= localActorsStart )
            {
                // Client isn't allowed to modify actor list directly.
                Assert.IsTrue( NetworkSystem.IsAuthoritative && index >= 0 );
                ExpandReplicatedActors( index );
            }

            actors[index]       = actor;
            actors[index].Id    = index;
        }


        private void ExpandReplicatedActors( UInt16 newSize )
        {
            while( newSize >= localActorsStart )
            {
                actors.Insert( localActorsStart, null );
                ++localActorsStart;

                // Update replicated actor's ids to match their new array index.
                for( int i = localActorsStart; i < actors.Count; ++i )
                {
                    if( actors[i] != null )
                    {
                        ++actors[i].Id;
                    }
                }
            }
        }


        private void InsertLocalActor( Actor actor )
        {
            for( int i = localActorsStart; i < actors.Count; ++i )
            {
                // Free space.
                if( actors[i] == null )
                {
                    actors[i] = actor;
                    actor.Id  = ( UInt16 )i;
                    return;
                }
            }

            actors.Add( actor );
            actor.Id = ( UInt16 )( actors.Count - 1 );
        }


        private void DestroyActorLocally( int index )
        {
            Assert.IsTrue( actors.Count > index && index >= 0 && actors[index] != null );
            actors[index].OnActorDestroy();
            Destroy( actors[index].gameObject );
            actors[index] = null;
        }


        private void DestroyActorLocallyRecursive( Transform transform, bool root )
        {
            var actor = transform.GetComponent<Actor>();
            if( transform == null || transform.gameObject == null )
            {
                return;
            }
            if( actor != null && ( ( !root && actor.Replicates ) || actors[actor.Id] == null ) )
            {
                return;
            }

            int count = transform.childCount;
            for( int i = 0; i < count; ++i )
            {
                DestroyActorLocallyRecursive( transform.GetChild( i ), false );
            }

            if( actor != null )
            {
                var index = actor.Id;
                Assert.IsTrue( actors.Count > index && index >= 0 && actors[index] != null );

                actors[index].OnActorDestroy();
                if( root )
                {
                    Destroy( actors[index].gameObject );
                }
                actors[index] = null;
            }
        }


        private void ProcessClientLoadResult( ClientGameMapLoadResult result )
        {
            result.Client.IsLoadingCurrentMap = false;
            for( int i = 0; i < bufferedInstantiateCalls.Count; ++i )
            {
                result.Client.SnapshotBuffer.CurrentSnapshot.AddRPC( bufferedInstantiateCalls[i].Call );
            }

            // Activate all relevancies.
            for( int i = 0; i < result.Client.Relevancies.Count; ++i )
            {
                result.Client.Relevancies[i].Type = RelevancyType.Active;
            }

            if( gameManager != null )
            {
                gameManager.OnClientConnect( result.Client );
            }
        }


        private void InitialiseMap()
        {
            RegisterExistingSceneActors();
            if( NetworkSystem.IsAuthoritative )
            {
                InitialiseReplicatedMap();
            }
            InitialiseLocalMap();
        }


        private void ClearMap()
        {
            // Don't destroy the world.
            for( int i = 1; i < actors.Count; ++i )
            {
                if( actors[i] != null && actors[i] != this && actors[i].DestroyOnLoad )
                {
                    DestroyActorLocally( i );
                }
            }
            bufferedInstantiateCalls.Clear();
        }


        private void RegisterExistingSceneActors()
        {
            // Register all objects at the beginning of the scene.
            var sceneActors = FindObjectsOfType<Actor>();
            for( int i = 0; i < sceneActors.Length; ++i )
            {
                if( sceneActors[i] != this )
                {
                    InitialiseActor( sceneActors[i], false, NetworkSystem.Id, false, null, null, 0 );
                }
            }
            for( int i = 0; i < sceneActors.Length; ++i )
            {
                if( sceneActors[i] != this )
                {
                    RegisterExistingActorComponents( sceneActors[i] );
                }
            }
            for( int i = 0; i < sceneActors.Length; ++i )
            {
                if( sceneActors[i] != this )
                {
                    PostHierarchyInitialiseActor( sceneActors[i] );
                }
            }
            var eventSystemComponent = FindObjectOfType<EventSystem>();
            if( eventSystemComponent != null )
            {
                eventSystem = eventSystemComponent;
            }

            // Create the event system if none was found.
            if( eventSystem == null )
            {
                var eventSystemGameObject = Instantiate<GameObject>( eventSystemPrefab.gameObject );
                eventSystem = eventSystemGameObject.GetComponent<EventSystem>();
            }
            if( defaultCanvasPrefab != null && defaultCanvas == null )
            {
                defaultCanvas = ( UICanvasActor )InstantiateUIActor( defaultCanvasPrefab, null );
                defaultCanvas.CanvasComponent.sortingOrder = 9999;
                defaultCanvas.EnableDontDestroyOnLoad();
            }
            if( consoleUIPrefab != null && consoleUI == null )
            {
                consoleUI    =  ( ConsoleUI )InstantiateUIActor( consoleUIPrefab, DefaultCanvas );
                consoleUI.EnableDontDestroyOnLoad();
            }
        }


        private void InitialiseActorHierarchy( Actor actor, bool replicates, long networkOwner,
            bool acceptsNewConnections, Controller responsibleController, StatsBase responsibleStats,
            UInt16 givenId )
        {
            InitialiseActorRecursive( actor.transform, replicates, networkOwner, acceptsNewConnections, responsibleController,
                responsibleStats, givenId );
            RegisterExistingActorComponentsRecursive( actor.transform );
            PostHierarchyInitialiseActorRecursive( actor.transform );
        }


        private void InitialiseActorRecursive( Transform transform, bool replicates, long networkOwner,
            bool acceptsNewConnections, Controller responsibleController, StatsBase responsibleStats,
            UInt16 givenId )
        {
            var actor = transform.GetComponent<Actor>();

            int count = transform.childCount;
            for( int i = 0; i < count; ++i )
            {
                InitialiseActorRecursive( transform.GetChild( i ), false, NetworkSystem.Id, false, 
                    responsibleController, responsibleStats, 0 );
            }

            if( actor != null )
            {
                InitialiseActor( actor, replicates, networkOwner, acceptsNewConnections, responsibleController,
                    responsibleStats, givenId );
            }
        }


        private void InitialiseActor( Actor actor, bool replicates, long networkOwner, bool acceptsNewConnections,
            Controller responsibleController, StatsBase responsibleStats, UInt16 givenId )
        {
            actor.ContainingWorld = this;
            actor.ResponsibleStats = responsibleStats;

            if( replicates )
            {
                InsertReplicatedActor( actor, givenId );
            }
            else
            {
                InsertLocalActor( actor );
            }

            
            actor.OnActorInitialise( replicates, networkOwner, acceptsNewConnections,
                    responsibleController );
        }


        private void RegisterExistingActorComponentsRecursive( Transform transform )
        {
            var actor = transform.GetComponent<Actor>();

            if( actor != null )
            {
                RegisterExistingActorComponents( actor );
            }

            int count = transform.childCount;
            for( int i = 0; i < count; ++i )
            {
                RegisterExistingActorComponentsRecursive( transform.GetChild( i ) );
            }
        }


        private void RegisterExistingActorComponents( Actor actor )
        {
            actor.RegisterExistingComponents();
        }


        private void PostHierarchyInitialiseActorRecursive( Transform transform )
        {
            var actor = transform.GetComponent<Actor>();

            if( actor != null )
            {
                PostHierarchyInitialiseActor( actor );
            }

            int count = transform.childCount;
            for( int i = 0; i < count; ++i )
            {
                PostHierarchyInitialiseActorRecursive( transform.GetChild( i ) );
            }
        }


        private void PostHierarchyInitialiseActor( Actor actor )
        {
            actor.OnPostHierarchyInitialise();
        }


        private void SetLocalTextUIActorRecursive( UIActor actor )
        {
            SetLocalTextUIActor( actor );

            int count = actor.transform.childCount;
            for( int i = 0; i < count; ++i )
            {
                var childActor = Actor.GetActorFromTransform<UIActor>( actor.transform.GetChild( i ) );
                if( childActor == null )
                {
                    continue;
                }
                SetLocalTextUIActorRecursive( childActor );
            }
        }


        private void SetLocalTextUIActor( UIActor actor )
        {
            actor.SetLocalText();
        }


        private void ToggleConsole()
        {
            if( consoleUI != null )
            {
                if( consoleUI.IsVisible )
                {
                    var player = FirstLocalPlayer;
                    if( player != null )
                    {
                        player.EnableLocalPlayerInput();
                    }
                    consoleUI.Hide();
                }
                else
                {
                    var player = FirstLocalPlayer;
                    if( player != null )
                    {
                        player.DisableLocalPlayerInput();
                    }
                    consoleUI.Show();
                }
            }
        }


        private void ActivateDebugBreak()
        {
            Debug.Break();
        }


        // RPCs


        private class InstantiateCall
        {
            public Actor    Actor { get; set; }
            public RPCCall  Call  { get; set; }


            public InstantiateCall( Actor actor, RPCCall call )
            {
                this.Actor  = actor;
                this.Call   = call;
            }
        }

        private List<InstantiateCall>      bufferedInstantiateCalls = new List<InstantiateCall>();
        private RPCCall                    latestLoadMapCall        = null;


        private void ServerAcknowledgeClientLoad( long clientId, string mapName, string mapHash, string gameModeName, string gameModeHash )
        {
            // We haven't loaded a map, invalid message.
            if( currentMapTransition == null )
            {
                NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Networking, "Received map load result from client before any map was loaded." );
                return;
            }

            // Do we know this client?
            var client = NetworkSystem.GetClientStateFromId( clientId );
            if( client == null )
            {
                NDebug.PrintSubsystemDebug( NDebug.DebugSubsystem.Networking, "Received map load result from client that does not exist." );
                return;
            }

            var mapHash128              = Hash128.Parse( mapHash );
            var gameModeHash128         = Hash128.Parse( gameModeHash );

            var result                  = new ClientGameMapLoadResult( client, mapName, mapHash128, gameModeName, gameModeHash128 );

            if( currentMapTransition.Status != MapTransitionStatus.Complete )
            {
                // We'll deal with this load result later.
                pendingClientLoadResults.Add( result );
                return;
            }

            ProcessClientLoadResult( result );
        }


        private void MulticastInstantiateTargetedActor( UInt16 index, string prefabName, Vector3 position, 
            Quaternion rotation, long owner, StatsBase responsibleStats )
        {
            var actorPrefab                 = resourceSystem.GetLoadedActorResourceWithName( prefabName );
            Assert.IsTrue( actorPrefab != null );

            var actor                       = InstantiateActorLocally( actorPrefab, position, rotation, true, index );
            InitialiseActorHierarchy( actor, true, owner, false, null, responsibleStats, index );
        }


        private void MulticastDestroyTargetedActor( Actor actor )
        {
            DestroyActorLocallyRecursive( actor.transform, true );
        }


        private void MulticastLoadMap( string mapName, string mapHash, string gameModeName, string gameModeHash )
        {
            // At this point the server is not sending any more data to us until we reply with an acknowledgement
            // that we have finished loading the map.

            var mapHash128          = Hash128.Parse( mapHash );
            var gameModeHash128     = Hash128.Parse( gameModeHash );

            var mapMeta             = resourceSystem.GetMapChunk( mapName );
            var gameModeMeta        = resourceSystem.GetGameModeChunk( gameModeName );

            if( mapMeta == null )
            {
                // TODO: Report unfound map.
            }


            if( gameModeMeta == null )
            {
                // TODO: Report unfound game mode.
            }

            if( currentGameMap == null || currentGameMap.Map.name != mapName 
                || currentGameMap.Mode.name != gameModeName )
            {
                var gameMap = new GameMap( mapMeta, gameModeMeta );
                LoadMap( gameMap );
            }
            else
            {
                RPC( ServerAcknowledgeClientLoad, RPCType.Server, this, NetworkSystem.Id, 
                    currentMapTransition.NextGameMap.Map.name, 
                    currentMapTransition.NextBatch.MapChunk.name,
                        currentMapTransition.NextGameMap.Mode.name, 
                        currentMapTransition.NextBatch.GameModeChunk.name );
            }
        }


        private void SendMapLoadAcknowledgementToServer()
        {
            RPC( ServerAcknowledgeClientLoad, RPCType.Server, this, 
                NetworkSystem.Id, currentMapTransition.NextGameMap.Map.name, 
                currentMapTransition.NextBatch.MapChunk.name,
                        currentMapTransition.NextGameMap.Mode.name, 
                        currentMapTransition.NextBatch.GameModeChunk.name );
        }
    }


    public class WorldConsoleCommandDefs : ConsoleCommandDefs
    {

        [ConsoleCommandDef( "RestartGameMap" )]
        public static void Cmd_RestartGameMap( World world )
        {
            world.RestartGameMap();
        }


        [ConsoleCommandDef( "GetActorUnderCursor" )]
        public static Actor Cmd_GetActorUnderCursor( World world )
        {
            var localPlayer     = world.FirstLocalPlayer;

            Camera camera       = null;
            if( localPlayer != null && localPlayer.CameraActor != null )
            {
                camera = localPlayer.CameraActor.CameraComponent;
            }
            else
            {
                camera = Camera.main;
            }

            if( camera == null )
            {
                world.ConsoleSystem.SendTextInput( "GetActorUnderCursor: Could not find camera." );
                return null;
            }
 
            var input                   = world.WorldInputComponent;
            var mousePositionNullable   = input.MousePosition;
            if( !mousePositionNullable.HasValue )
            {
                return null;
            }

            var ray                     = camera.ScreenPointToRay( mousePositionNullable.Value );

            RaycastHit hit;
            if( Physics.Raycast( ray, out hit ) )
            {
                var actor = hit.collider.gameObject.GetComponent<Actor>();
                if( actor != null )
                {
                    return actor;
                }
            }

            return null;
        }


        [ConsoleCommandDef( "DestroyActor" )]
        public static void Cmd_DestroyActor( World world, Actor actor )
        {
            if( actor == null )
            {
                world.ConsoleSystem.SendTextInput( "Could not destroy actor because it was null." );
                return;
            }
            world.DestroyActor( actor );
        }


        [ConsoleCommandDef( "PrintActorName" )]
        public static void Cmd_PrintActorName( World world, Actor actor )
        {
            if( actor == null )
            {
                world.ConsoleSystem.SendTextInput( "null" );
                return;
            }
            world.ConsoleSystem.SendTextInput( actor.gameObject.name );
        }
    }
}
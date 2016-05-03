using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lidgren.Network;
using System;
using UnityEngine.Assertions;

namespace Nixin
{
    public class GameManager : Actor
    {


        // Public:


        public virtual void InitialiseGameManager( List<GameVar> gameVars )
        {
            // Create game state.
            Assert.IsTrue( gameStatePrefab != null );
            ContainingWorld.CreateGameState( gameStatePrefab );

            // Might not have a map extension. Create it if we do.
            if( gameModeMapExtensionPrefab != null )
            {
                gameModeMapExtension = ContainingWorld.CreateGameModeMapExtension( gameModeMapExtensionPrefab );
            }

            this.gameVars = gameVars;
        }


        public virtual void HandleNetDiscoveryRequest( NetIncomingMessage msg, NetOutgoingMessage response )
        {
        }


        public virtual void OnClientConnect( ClientState client )
        {

        }


        public virtual void OnClientDisconnect( ClientState client )
        {

        }


        public virtual void SetUpHud()
        {

        }


        public virtual Controller AddController( ClientState client, JoinType type )
        {
            var relevancies = new List<ClientState>();

            // If client is null, we're adding a listen player.
            if( client != null )
            {
                relevancies.Add( client );
            }

            var id = client == null ? ContainingWorld.NetworkSystem.Id : client.Id;

            Actor prefab = null;
            if( type == JoinType.Player )
            {
                prefab = playerPrefab;
            }
            else if( type == JoinType.Spectator )
            {
                prefab = spectatorPrefab;
            }
            else if( type == JoinType.AI )
            {
                prefab = AIControllerPrefab;
            }

            Controller controller = ( Controller )ContainingWorld.InstantiateReplicatedActor( prefab, Vector3.zero, Quaternion.identity, id, false, relevancies, null );
            Controllers.Add( controller );

            return controller;
        }


        public virtual void InitialiseStats( Controller controller, StatsBase stats )
        {

        }


        public GameVar FindGameVarByName( string name )
        {
            for( int i = 0; i < gameVars.Count; ++i )
            {
                if( gameVars[i].Decl.Name == name )
                {
                    return gameVars[i];
                }
            }
            return null;
        }


        public List<Controller> Controllers
        {
            get
            {
                return controllers;
            }
        }


        public GameModeMapExtension GameModeMapExtensionPrefab
        {
            get
            {
                return gameModeMapExtensionPrefab;
            }
        }


        public GameModeMapExtension GameModeMapExtension
        {
            get
            {
                return gameModeMapExtension;
            }
        }


        // Private:


        private List<Controller>        controllers             = new List<Controller>();
        private List<GameVar>           gameVars                = new List<GameVar>();
        private GameModeMapExtension    gameModeMapExtension    = null;

        [SerializeField]
        private GameState       gameStatePrefab         = null;

        [SerializeField]
        private Player          playerPrefab            = null;

        [SerializeField]
        private Player          spectatorPrefab         = null;

        [SerializeField]
        private AIController    AIControllerPrefab      = null;

        [SerializeField]
        private GameModeMapExtension gameModeMapExtensionPrefab = null;
    }

    [System.Serializable]
    public class SubClassOfGameManager : SubClassOf<GameManager>
    {
    }

    [System.Serializable]
    public class GameManagerWeakReference : WeakUnityReference<GameManager>
    {
    }


    public enum JoinType
    {
        Player,
        Spectator,
        AI,
    }
    

    public class GameManagerConsoleCommandDefs : ConsoleCommandDefs
    {

        [ConsoleCommandDef( "SpawnAI" )]
        public static Controller SpawnAI( World world )
        {
            if( !world.NetworkSystem.IsAuthoritative )
            {
                world.ConsoleSystem.SendTextInput( "You do not have permission to spawn AI." );
                return null;
            }

            var manager = world.GameManager;
            if( manager == null )
            {
                world.ConsoleSystem.SendTextInput( "Could not spawn AI because the game manager does not exist." );
                return null;
            }
            return manager.AddController( null, JoinType.AI );
        }
    }
}
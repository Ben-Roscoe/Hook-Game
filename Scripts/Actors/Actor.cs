using UnityEngine;
using System.Collections.Generic;
using Lidgren.Network;
using System;
using UnityEngine.Serialization;

namespace Nixin
{
    public class Actor : NixinBehaviour, IUpdatable
    {


        public static T GetActorFromTransform<T>( Transform transform ) where T : Actor
        {
            if( transform == null )
            {
                return null;
            }
            T actor = transform.GetComponent<T>();
            return actor;
        }


        // Public:


        [SerializeField, HideInInspector]
        public List<Component>          componentsToDestroy                = new List<Component>();

        [SerializeField, HideInInspector]
        public List<Component>          createdComponents                  = new List<Component>();

        [SerializeField, HideInInspector]
        public List<Component>          currentCreatedComponents           = new List<Component>();

        public World                    ContainingWorld                     { get; set; }


        public override void EditorConstruct()
        {
            base.EditorConstruct();
            currentCreatedComponents.Clear();
        }


        public virtual void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            this.replicates                 = replicates;
            this.networkOwner               = networkOwner;
            this.responsibleController      = responsibleController;

            if( IsAuthority )
            {
                this.responsibleStats           = ResponsibleController == null ? null : ResponsibleController.Stats;
            }

            AcceptsNewConnections = acceptsNewConnections;

            UpdateComponent.Initialise( this );

            if( IsAuthority )
            {
                OnActorInitialisePostSnapshot();
            }
        }


        public void RegisterExistingComponents()
        {
            var nixinComponents = GetComponents<NixinComponent>();
            for( int i = 0; i < nixinComponents.Length; ++i )
            {
                RegisterComponent( nixinComponents[i] );
            }

            for( int i = 0; i < components.Count; ++i )
            {
                components[i].OnOwningActorInitialised();
            }
        }


        public virtual void OnPostHierarchyInitialise()
        {
        }


        public virtual void OnActorInitialisePostSnapshot()
        {
            postSnapshotInitialised = true;
        }


        public virtual void OnActorCacheUnityValues()
        {
            position    = transform.position;
            rotation    = transform.rotation;
        }


        public virtual void OnUpdate( float deltaTime )
        {
            for( int i = 0; i < components.Count; ++i )
            {
                components[i].OnOwningActorUpdate();
            }
        }


        public virtual void OnFixedUpdate()
        {
            for( int i = 0; i < components.Count; ++i )
            {
                components[i].OnOwningActorFixedUpdate();
            }
        }


        public virtual void OnActorDestroy()
        {
            OnDestroyed.Invoke( this );
            OnDestroyed.RemoveAll();

            UnregisterAllComponents();
            UpdateComponent.IsEnabled = false;
        }


        public virtual void OnLocalisationChanged()
        {
        }


        public virtual void EnableDontDestroyOnLoad()
        {
            if( transform.parent == null )
            {
                DontDestroyOnLoad( gameObject );
            }
            destroyOnLoad = false;
        }


        public virtual void DisableDontDestroyOnLoad()
        {
            destroyOnLoad = true;
        }


        public virtual void OnPoolAllocate()
        {
            gameObject.SetActive( true );
        }


        public virtual void OnPoolDeallocate()
        {
            gameObject.SetActive( false );
        }


        public void ModifyStat( Stat stat, StatModifier modifier, bool baseModifier, Actor instigator )
        {
            if( stat.Owner != this || !CanModifyStat( stat, modifier, instigator ) )
            {
                return;
            }

            PreModifyStat( stat, modifier, baseModifier, instigator );
            stat.AddModifier( modifier, baseModifier );
            PostModifyStat( stat, modifier, baseModifier, instigator );
        }


        public virtual bool IsStatModifiable( Stat stat, Actor instigator )
        {
            return stat.Owner == this;
        }


        public virtual bool CanModifyStat( Stat stat, StatModifier modifier, Actor instigator )
        {
            return stat.Owner == this;
        }


        public virtual void PreModifyStat( Stat stat, StatModifier modifier, bool baseModifier, Actor instigator )
        {
        }


        public virtual void PostModifyStat( Stat stat, StatModifier modifer, bool baseModifier, Actor instigator )
        {
        }


        public virtual void WriteSnapshot( NetBuffer buffer )
        {
            buffer.WriteActor( responsibleStats );
            foreach( var stat in stats )
            {
                stat.Value.WriteSnapshot( buffer );
            }
            for( int i = 0; i < components.Count; ++i )
            {
                components[i].WriteSnapshot( buffer );
            }
        }


        public virtual void ReadSnapshot( NetBuffer buffer, bool isFuture )
        {
            responsibleStats = buffer.ReadActor<StatsBase>( ContainingWorld, responsibleStats, isFuture );
            foreach( var stat in stats )
            {
                stat.Value.ReadSnapshot( buffer, isFuture );
            }
            for( int i = 0; i < components.Count; ++i )
            {
                components[i].ReadSnapshot( buffer, isFuture );
            }
        }


        public virtual Stat GetStat( int nameHash, bool includeChildren )
        {
            Stat outStat = null;
            if( stats.TryGetValue( nameHash, out outStat ) )
            {
                return outStat;
            }
            return null;
        }


#if UNITY_EDITOR
        public void CheckForComponentRemoval()
        {
            // Find a removal.
            for( int i = 0; i < createdComponents.Count; ++i )
            {
                // Don't remove if it was added this round.
                if( currentCreatedComponents.Contains( createdComponents[i] ) )
                {
                    continue;
                }

                if( createdComponents[i] != null )
                {
                    createdComponents[i].hideFlags = ( HideFlags )int.MaxValue;
                    componentsToDestroy.Add( createdComponents[i] );
                }
                createdComponents.RemoveAt( i );
                --i;
            }
        }
#endif


        public void RegisterComponent( NixinComponent component )
        {
            components.Add( component );
            component.Owner = this;
            component.OnRegistered( this, GetNextComponentId() );
        }


        public void UnregisterComponent( NixinComponent component )
        {
            if( components.Remove( component ) )
            {
                component.OnUnregistered();
                component.Owner = null;
            }
        }


        public T GetNixinComponent<T>() where T : NixinComponent
        {
            for( int i = 0; i < components.Count; ++i )
            {
                T componentAsType = components[i] as T;
                if( componentAsType != null )
                {
                    return componentAsType;
                }
            }
            return null;
        }


        public NixinComponent FindComponent( byte id )
        {
            for( int i = 0; i < components.Count; ++i )
            {
                if( components[i].ComponentId == id )
                {
                    return components[i];
                }
            }
            return null;
        }


        public bool DestroyOnLoad
        {
            get
            {
                return destroyOnLoad;
            }
        }


        public List<Relevancy> Relevancies
        {
            get
            {
                return relevancies;
            }
        }


        public bool Replicates
        {
            get
            {
                return replicates;
            }
        }


        public bool IsAuthority
        {
            get
            {
                return ContainingWorld.NetworkSystem.IsAuthoritative || !Replicates;
            }
        }


        public UInt16 Id
        {
            get
            {
                return actorId;
            }
            set
            {
                actorId = value;
            }
        }


        public long NetworkOwner
        {
            get
            {
                return networkOwner;
            }
            set
            {
                networkOwner = value;
            }
        }


        public bool AcceptsNewConnections
        {
            get
            {
                return acceptsNewConnections;
            }
            set
            {
                acceptsNewConnections = value;
            }
        }


        public NixinEvent<Stat, StatModifier> OnStatModifiedEvent
        {
            get
            {
                return onStatModifiedEvent;
            }
        }


        public NixinEvent<Stat> OnStatAdded
        {
            get
            {
                return onStatAddedEvent;
            }
        }


        public NixinEvent<Actor> OnDestroyed
        {
            get
            {
                return onDestroyed;
            }
        }


        public Controller ResponsibleController
        {
            get
            {
                return responsibleController;
            }
        }


        public virtual StatsBase ResponsibleStats
        {
            get
            {
                return responsibleStats;
            }
            set
            {
                responsibleStats = value;
            }
        }


        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }


        public Quaternion Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
            }
        }


        public IActorPool Pool
        {
            get
            {
                return pool;
            }
            set
            {
                pool = value;
            }
        }


        public UpdateComponent UpdateComponent
        {
            get
            {
                return updateComponent;
            }
        }


        public bool PostSnapshotInitialised
        {
            get
            {
                return postSnapshotInitialised;
            }
        }


        public bool IsInitialsed
        {
            get
            {
                return isInitialised;
            }
            set
            {
                isInitialised = value;
            }
        }


        public bool IsPostHierarchyInitialised
        {
            get
            {
                return isPostHierarchyInitialised;
            }
            set
            {
                isPostHierarchyInitialised = value;
            }
        }


        // Protected:


        // This is automatically set when spawned at runtime. It is used for to determine if existing scene
        // objects should be replicated.
        [SerializeField]
        protected bool replicates;


        protected Stat AddStat( NixinName statName )
        {
            var stat = new Stat( this, statName );
            stats.Add( statName.Id, stat );
            onStatAddedEvent.Invoke( stat );
            return stat;
        }


        // Private:


        private List<Relevancy>             relevancies                     = new List<Relevancy>();

        private bool                        acceptsNewConnections           = false;

        private List<NixinComponent>        components                      = new List<NixinComponent>();
        private byte                        nextComponentId                 = 1;


        protected long                      networkOwner                    = -1;

        [SerializeField]
        private UInt16                      actorId                         = 0;

        // The actor's update component. Defines if the actor updates fixed or at a specific rate based on delta time.
        [SerializeField]
        private UpdateComponent                 updateComponent;

        private Controller                  responsibleController   = null;
        private StatsBase                   responsibleStats        = null;

        private Dictionary<int,Stat>            stats               = new Dictionary<int, Stat>();

        private NixinEvent<Stat, StatModifier>  onStatModifiedEvent = new NixinEvent<Stat, StatModifier>();
        private NixinEvent<Stat>                onStatAddedEvent    = new NixinEvent<Stat>();
        private NixinEvent<Actor>               onDestroyed         = new NixinEvent<Actor>();

        // Cache position and rotation on the actor to avoid overhead, and allow them to be
        // accessed by other threads.
        private Vector3                         position            = Vector3.zero;
        private Quaternion                      rotation            = Quaternion.identity;

        // The pool the actor belongs to. null if none.
        private IActorPool                      pool                = null;

        private bool                            destroyOnLoad       = true;
        private bool                            postSnapshotInitialised = false;

        private bool                            isInitialised                   = false;
        private bool                            isPostHierarchyInitialised      = false;


        private void UnregisterAllComponents()
        {
            for( int i = 0; i < components.Count; )
            {
                if( components[i] != null )
                {
                    UnregisterComponent( components[i] );
                }
            }
        }


        private byte GetNextComponentId()
        {
            return nextComponentId++;
        }
    }


    public enum ActorHierarchyReplication
    {
        Local,
        Replicated,
        InheritParent,
    }
}
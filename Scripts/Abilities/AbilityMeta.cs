using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Nixin
{
    public abstract class AbilityMeta : ScriptableObject
    {


        // Public:


        
        public void DestroyInstance( AbilityInstance instance )
        {
            instance.Uninitialise();
        }


        public string InternalName
        {
            get
            {
                return internalName;
            }
        }


        public string NameToken
        {
            get
            {
                return nameToken;
            }
        }


        public int MaxLevel
        {
            get
            {
                return maxLevel;
            }
        }


        public Sprite Icon
        {
            get
            {
                return icon;
            }
        }


        public AbilityCategory Category
        {
            get
            {
                return category;
            }
        }


        public float BaseCooldownTime
        {
            get
            {
                return baseCooldownTime;
            }
        }


        public abstract AbilityType         Type             { get; }
        public abstract AbilityTargetType   TargetType       { get; }
        public abstract float               BaseAreaOfEffect { get; }


        // Private:


        [SerializeField, FormerlySerializedAs( "InternalName" )]
        private string          internalName;

        [SerializeField, FormerlySerializedAs( "LocalNameId" )]
        private string          nameToken;

        // -1 for infinite.
        [SerializeField, FormerlySerializedAs( "MaxLevel" )]
        private int             maxLevel;

        [SerializeField, FormerlySerializedAs( "Icon" )]
        private Sprite          icon;

        [SerializeField, FormerlySerializedAs( "Category" )]
        private AbilityCategory category;

        [SerializeField, FormerlySerializedAs( "baseCooldownTime" )]
        private float           baseCooldownTime;
    }


    public abstract class AbilityInstance : IUpdatable
    {


        // Public:


        public AbilityInstance( Actor owner, Actor instigator, NixinName id,
            AbilityMeta meta )
        {
            Assert.IsTrue( owner != null, "Invalid owner." );

            this.owner           = owner;
            this.instigator      = instigator;
            this.id              = id;
            this.meta            = meta;
            this.containingWorld = owner.ContainingWorld;

            owner.OnDestroyed.AddHandler( OnOwnerDestroyed );
        }


        public virtual void Uninitialise()
        {
            if( owner != null )
            {
                owner.OnDestroyed.RemoveHandler( OnOwnerDestroyed );
            }

            updateComponent.UpdateGroupType = UpdateGroupType.None;
            owner                          = null;
            meta                            = null;
            containingWorld                 = null;
        }


        public virtual void OnLevelChanged()
        {
        }


        public virtual void OnUpdate( float deltaTime )
        {
        }


        public virtual void OnFixedUpdate()
        {
        }


        public int Level
        {
            get
            {
                return level;
            }
            set
            {
                if( level != value )
                {
                    OnLevelChanged();
                }
                level = value;
            }
        }


        public Actor Owner
        {
            get
            {
                return owner;
            }
        }


        public Actor Instigator
        {
            get
            {
                return instigator;
            }
        }


        public NixinName Id
        {
            get
            {
                return id;
            }
        }


        public AbilityMeta Meta
        {
            get
            {
                return meta;
            }
        }


        public virtual float AreaOfEffect
        {
            get
            {
                return -1;
            }
        }


        public World ContainingWorld
        {
            get
            {
                return containingWorld;
            }
        }


        public UpdateComponent UpdateComponent
        {
            get
            {
                return updateComponent;
            }
        }


        // Private:


        // This ability gets removed with it's target. So, if something should outlive it's target,
        // make it the world.
        private Actor           owner       = null;
        private Actor           instigator  = null;
        private NixinName       id;
        private AbilityMeta     meta        = null;

        private int             level       = -1;

        private World           containingWorld = null;
        private UpdateComponent updateComponent = new UpdateComponent();


        private void OnOwnerDestroyed( Actor actor )
        {
            Uninitialise();
        }
    }


    public enum AbilityCategory
    {
        WarriorSkill,
        ArcaneSkill,
        EngineerSkill,
        Item,
        Other,
    }


    public enum AbilityType
    {
        Passive,
        Target,
        Activate,
    }


    public enum AbilityTargetType
    {
        None,
        Actor,
        Location,
    }
}

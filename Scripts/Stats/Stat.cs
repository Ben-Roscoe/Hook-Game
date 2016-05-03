using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class Stat
    {


        // Public:


        public Stat( Actor owner, NixinName statName, Stat statMax = null, float hardMax = -1.0f )
        {
            this.owner      = owner;
            this.hardMax    = hardMax;
            this.statMax    = statMax;
            this.statName   = statName;
        }


        public void WriteSnapshot( NetBuffer buffer )
        {
            buffer.Write( baseValue );
            buffer.Write( ModifiedValue );
            buffer.Write( BaseModifiedValue );
        }


        public void ReadSnapshot( NetBuffer buffer, bool isFuture )
        {
            baseValue                       = buffer.ReadFloat( baseValue, isFuture );
            clientCachedModifiedValue       = buffer.ReadFloat( clientCachedModifiedValue, isFuture );
            clientCachedBaseModifiedValue   = buffer.ReadFloat( clientCachedBaseModifiedValue, isFuture );
        }


        public void AddModifier( StatModifier modifier, bool baseModifier )
        {
            if( owner == null || !owner.IsAuthority )
            {
                return;
            }

            var old = ModifiedValue;

            if( modifier.IsPermanent )
            {
                BaseValue += modifier.GetModifiedValue( baseValue );
            }
            else
            {
                if( baseModifier )
                {
                    baseModifiers.Add( modifier );
                }
                else
                {
                    modifiers.Add( modifier );
                }
            }

            // Has this stat changed as a result of the modifier?
            var current = ModifiedValue;
            var delta   = current - old;
            if( delta > float.Epsilon || delta < -float.Epsilon )
            {
                valueChanged.Invoke( this, old, current );
            }
        }


        public void RemoveModifier( StatModifier modifier, bool baseModifier )
        {
            if( owner == null || !owner.IsAuthority )
            {
                return;
            }

            if( baseModifier )
            {
                baseModifiers.Remove( modifier );
            }
            else
            {
                modifiers.Remove( modifier );
            }
        }


        public float BaseValue
        {
            get
            {
                return baseValue;
            }
            set
            {
                baseValue = value;
                if( hardMax > 0.0f )
                {
                    baseValue = Mathf.Clamp( baseValue, hardMin, hardMax );
                }
                if( statMax != null )
                {
                    baseValue = Mathf.Clamp( baseValue, hardMin, statMax.ModifiedValue );
                }
            }
        }


        public float BaseModifiedValue
        {
            get
            {
                if( owner == null || !owner.IsAuthority )
                {
                    return clientCachedBaseModifiedValue;
                }

                float baseModifiedValue = baseValue;
                for( int i = 0; i < baseModifiers.Count; ++i )
                {
                    baseModifiedValue += modifiers[i].GetModifiedValue( baseValue );
                }
                if( hardMax > 0.0f )
                {
                    baseModifiedValue = Mathf.Clamp( baseModifiedValue, hardMin, hardMax );
                }
                if( statMax != null )
                {
                    baseModifiedValue = Mathf.Clamp( baseModifiedValue, hardMin, statMax.ModifiedValue );
                }
                return baseModifiedValue;
            }
        }


        public float ModifiedValue
        {
            get
            {
                if( owner == null || !owner.IsAuthority )
                {
                    return clientCachedModifiedValue;
                }

                float baseModifiedValue     = BaseModifiedValue;
                float modifiedValue         = baseModifiedValue;
                for( int i = 0; i < modifiers.Count; ++i )
                {
                    modifiedValue += modifiers[i].GetModifiedValue( baseModifiedValue );
                }
                if( hardMax > 0.0f )
                {
                    modifiedValue = Mathf.Clamp( modifiedValue, hardMin, hardMax );
                }
                if( statMax != null )
                {
                    modifiedValue = Mathf.Clamp( modifiedValue, hardMin, statMax.ModifiedValue );
                }
                return modifiedValue;
            }
        }


        public Stat StatMax
        {
            get
            {
                return statMax;
            }
            set
            {
                statMax = value;
            }
        }


        public float HardMax
        {
            get
            {
                return hardMax;
            }
            set
            {
                hardMax = value;
            }
        }


        public Actor Owner
        {
            get
            {
                return owner;
            }
        }


        public NixinName StatName
        {
            get
            {
                return statName;
            }
        }


        public NixinEvent<Stat, float, float> ValueChanged
        {
            get
            {
                return valueChanged;
            }
        }


        // Private:


        private const float                 hardMin         = 0.0f;

        private float                       baseValue       = 0.0f;
        private List<StatModifier>          modifiers       = new List<StatModifier>();

        // Modifiers that should be considered part of the base for stuff like percentage modifiers.
        private List<StatModifier>          baseModifiers   = new List<StatModifier>();
        private Actor                       owner           = null;
        private NixinName                   statName;

        // We'll send the current modified value and cache it here for clients. This means they don't
        // have to know about all stat modifiers added to this stat.
        private float                       clientCachedModifiedValue       = 0.0f;
        private float                       clientCachedBaseModifiedValue   = 0.0f;

        private float                       hardMax         = -1;
        private Stat                        statMax         = null;

        // Stat, old, current.
        private NixinEvent<Stat, float, float> valueChanged = new NixinEvent<Stat, float, float>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nixin
{
    public enum UpdateGroupType
    {
        None,
        Update,
        Fixed,
        UpdateAndFixed,
    }

    [System.Serializable]
    public class UpdateComponent
    {


        // Public:


        public void Initialise( IUpdatable owner )
        {
            Assert.IsNotNull( owner );

            this.owner = owner;
            AddToUpdateGroups();
        }


        public void TryUpdate( float deltaTime )
        {
            Assert.IsNotNull( Owner );

            if( UpdateRate <= 0.0f )
            {
                Owner.OnUpdate( deltaTime );
                return;
            }

            timeSinceLastUpdate += deltaTime;
            if( timeSinceLastUpdate >= UpdateRate )
            {
                Owner.OnUpdate( timeSinceLastUpdate );
                timeSinceLastUpdate = 0.0f;
            }
        }


        public void UpdateFixed()
        {
            Assert.IsNotNull( Owner );

            Owner.OnFixedUpdate();
        }

        
        public UpdateGroupType UpdateGroupType
        {
            get
            {
                return updateGroupType;
            }
            set
            {
                if( owner != null )
                {
                    RemoveFromUpdateGroups();
                }
                updateGroupType = value;
                if( owner != null )
                {
                    AddToUpdateGroups();
                }
            }
        }


        public float UpdateRate
        {
            get
            {
                return updateRate;
            }
            set
            {
                updateRate = value;
            }
        }


        public bool UseActorDefaultValues
        {
            get
            {
                return useActorDefaultValues;
            }
            set
            {
                useActorDefaultValues = value;
            }
        }


        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                if( IsEnabled == value )
                {
                    return;
                }

                isEnabled = value;
                if( Owner != null )
                {
                    if( !IsEnabled )
                    {
                        RemoveFromUpdateGroups();
                    }
                    else
                    {
                        AddToUpdateGroups();
                    }
                }
            }
        }


        public IUpdatable Owner
        {
            get
            {
                return owner;
            }
        }


        // Private:


        [SerializeField]
        private bool                useActorDefaultValues = true;

        [SerializeField]
        private UpdateGroupType     updateGroupType     = UpdateGroupType.None;

        [SerializeField]
        private float               updateRate          = 0.0f;

        private IUpdatable          owner               = null;
        private float               timeSinceLastUpdate = 0.0f;
        private bool                isEnabled           = true;


        private void AddToUpdateGroups()
        {
            if( owner.ContainingWorld == null )
            {
                return;
            }

            if( updateGroupType == UpdateGroupType.Update || updateGroupType == UpdateGroupType.UpdateAndFixed )
            {
                owner.ContainingWorld.DeltaUpdateGroup.AddUpdatable( owner );
            }
            if( updateGroupType == UpdateGroupType.Fixed || updateGroupType == UpdateGroupType.UpdateAndFixed )
            {
                owner.ContainingWorld.FixedUpdateGroup.AddUpdatable( owner );
            }
        }


        private void RemoveFromUpdateGroups()
        {
            if( owner.ContainingWorld == null )
            {
                return;
            }

            if( updateGroupType == UpdateGroupType.Update || updateGroupType == UpdateGroupType.UpdateAndFixed )
            {
                owner.ContainingWorld.DeltaUpdateGroup.RemoveUpdatable( owner );
            }
            if( updateGroupType == UpdateGroupType.Fixed || updateGroupType == UpdateGroupType.UpdateAndFixed )
            {
                owner.ContainingWorld.FixedUpdateGroup.RemoveUpdatable( owner );
            }
        }
    }
}

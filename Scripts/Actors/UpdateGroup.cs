using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class UpdateGroup
    {


        // Public:

        
        public void Update( float deltaTime )
        {
            if( !IsEnabled )
            {
                return;
            }

            for( int i = 0; i < updatables.Count; ++i )
            {
                updatables[i].UpdateComponent.TryUpdate( deltaTime );
            }
        }


        public void UpdateFixed()
        {
            if( !IsEnabled )
            {
                return;
            }

            for( int i = 0; i < updatables.Count; ++i )
            {
                updatables[i].UpdateComponent.UpdateFixed();
            }
        }


        public void AddUpdatable( IUpdatable updatable )
        {
            updatables.Add( updatable );
        }


        public void RemoveUpdatable( IUpdatable updatable )
        {
            updatables.Remove( updatable );
        }


        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                isEnabled = value;
            }
        }


        // Private:


        private List<IUpdatable>     updatables          = new List<IUpdatable>();
        private bool                 isEnabled           = true;
    }
}

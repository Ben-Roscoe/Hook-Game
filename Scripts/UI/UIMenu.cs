using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class UIMenu : UIActorGroup
    {


        // Public:


        public List<UITransition> Transitions
        {
            get
            {
                return transitions;
            }
        }


        public override void OnActorDestroy()
        {
            base.OnActorDestroy();
            ClearTransitions();
        }


        // Protected:


        protected void AddTransition( UITransition transition )
        {
            transitions.Add( transition );
        }


        // Private:


        private List<UITransition>            transitions = new List<UITransition>();


        private void ClearTransitions()
        {
            for( int i = 0; i < transitions.Count; ++i )
            {
                transitions[i].Clear();
            }
            transitions.Clear();
        }
    }
}

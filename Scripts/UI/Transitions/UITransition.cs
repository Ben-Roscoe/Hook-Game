using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public abstract class UITransition
    {


        // Public:


        public UITransition( UIActor from, UIActor to )
        {
            this.from   = from;
            this.to     = to;
        }


        public virtual void Clear()
        {
        }


        public UIActor To
        {
            get
            {
                return to;
            }
        }


        public UIActor From
        {
            get
            {
                return from;
            }
        }


        // Protected:


        protected void MakeTransition()
        {
            from.Hide();
            to.Show();
        }


        // Private:


        private UIActor         from        = null;
        private UIActor         to          = null;
    }
}

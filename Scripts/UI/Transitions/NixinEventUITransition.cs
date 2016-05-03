using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class NixinEventUITransition : UITransition
    {


        // Public:


        public NixinEventUITransition( UIActor from, UIActor to, NixinEvent instigator ) : base( from, to )
        {
            this.instigator = instigator;
            instigator.AddHandler( MakeTransition );
        }


        public override void Clear()
        {
            base.Clear();
            instigator.RemoveHandler( MakeTransition );
        }


        public NixinEvent Instigator
        {
            get
            {
                return instigator;
            }
        }


        // Private:


        private NixinEvent          instigator = null;
    }
}

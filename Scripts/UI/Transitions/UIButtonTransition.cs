using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace Nixin
{
    public class UIButtonTransition : UITransition
    {


        // Public:


        public UIButtonTransition( UIActor from, UIActor to, Button instigator ) : base( from, to )
        {
            this.instigator         = instigator;
            instigator.onClick.AddListener( MakeTransition );
        }


        public override void Clear()
        {
            base.Clear();
            instigator.onClick.RemoveListener( MakeTransition );
        }


        public Button Instigator
        {
            get
            {
                return instigator;
            }
        }


        // Private:


        private Button      instigator = null;
    }
}

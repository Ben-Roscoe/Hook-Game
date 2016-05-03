using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class DefaultUIShowHideEffect : UIShowHideEffect
    {


        // Public:


        public DefaultUIShowHideEffect( UIActor uiActor ) : base( uiActor )
        {
        }


        public override void Show()
        {
            base.Show();
            UIActor.gameObject.SetActive( true );
            EndTransition( true );
        }


        public override void Hide()
        {
            base.Hide();
            UIActor.gameObject.SetActive( false );
            EndTransition( false );
        }


        // Private:


    }
}

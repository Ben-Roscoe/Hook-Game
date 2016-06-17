using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class FogOfWarBlockerComponent : NixinComponent
    {


        // Public:


        public override void OnRegistered( Actor owner, byte id )
        {
            base.OnRegistered( owner, id );
            FogOfWarMapComponent map = GetComponent<FogOfWarMapComponent>();
            map.RegisterBlocker( this );
        }


        // Private:
    }
}

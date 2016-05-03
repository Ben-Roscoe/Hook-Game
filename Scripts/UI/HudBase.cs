using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public class HudBase : UICanvasActor
    {


        // Public:


        public virtual void SetUp( Player localPlayer )
        {
            this.localPlayer = localPlayer;
        }


        public Player LocalPlayer
        {
            get
            {
                return localPlayer;
            }
        }


        // Private:


        private Player          localPlayer = null;
    }


    [System.Serializable]
    public class HudBaseWeakReference : WeakUnityReference<HudBase>
    {
    }
}

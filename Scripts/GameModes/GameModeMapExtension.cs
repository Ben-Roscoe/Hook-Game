using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nixin
{
    public class GameModeMapExtension : Actor
    {
    }


    [System.Serializable]
    public class GameModeMapExtensionWeakReference : WeakUnityReference<GameModeMapExtension>
    {
    }
}

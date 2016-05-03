using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public interface IPullable
    {
        // Needs a transform that can be pulled.
        Transform       transform { get; }
        Vector3         AttachOffset { get; }
        float           AttachMinimum { get; }

        bool Attach( HookActor hook );
        void Detach( HookActor hook );

        bool CanMove { get; }
    }
}

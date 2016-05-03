using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public interface IUpdatable
    {
        World               ContainingWorld { get; }
        UpdateComponent     UpdateComponent { get; }

        void OnUpdate( float deltaTime );
        void OnFixedUpdate();
    }
}

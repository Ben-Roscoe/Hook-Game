using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public interface IActorPool
    {
        void ClearPool( bool destroyActors );
        void FreeActor( Actor actor, bool destroyActor );
    }
}

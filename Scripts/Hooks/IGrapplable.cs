using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public interface IGrapplable
    {
        bool StartGrapple( HookActor hook );
        void EndGrapple( HookActor hook );
    }
}

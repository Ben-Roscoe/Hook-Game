using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using UnityEngine;

namespace Nixin
{
    public class StatsBase : Actor
    {


        // Public:


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            if( IsAuthority )
            {
                Controller  = responsibleController;
                repIsActive = true;
            }
        }


        public override void WriteSnapshot( NetBuffer buffer )
        {
            base.WriteSnapshot( buffer );

            buffer.Write( repIsActive );
            buffer.WriteActor( controller );
        }


        public override void ReadSnapshot( NetBuffer buffer, bool isFuture )
        {
            base.ReadSnapshot( buffer, isFuture );

            repIsActive = buffer.ReadBoolean( repIsActive, isFuture );
            controller  = buffer.ReadActor<Controller>( ContainingWorld, controller, isFuture );
        }


        public override Stat GetStat( int nameHash, bool includeChildren )
        {
            // Make sure we check the controller as well.
            var ret = base.GetStat( nameHash, includeChildren );
            if( ret != null || controller == null || !includeChildren )
            {
                return ret;
            }
            return controller.GetStat( nameHash, includeChildren );
        }


        public virtual Controller Controller
        {
            get
            {
                return controller;
            }
            set
            {
                if( IsAuthority )
                {
                    controller = value;
                }
            }
        }


        public bool IsActive
        {
            get
            {
                return repIsActive;
            }
            set
            {
                if( !IsAuthority )
                {
                    return;
                }
                repIsActive = value;
            }
        }


        public override StatsBase ResponsibleStats
        {
            get
            {
                return this;
            }
        }


        // Private:


        private Controller                      controller      = null;
        private bool                            repIsActive     = false;
    }


    [System.Serializable]
    public class StatsBaseWeakReferenece : WeakUnityReference<StatsBase>
    {
    }
}

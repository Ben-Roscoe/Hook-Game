using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nixin
{
    public class UIActorGroup : UIActor
    {


        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

            canvasGroupComponent        = ConstructDefaultStaticComponent<CanvasGroup>( this, "CanvasGroupComponent", canvasGroupComponent );
        }


        public CanvasGroup CanvasGroupComponent
        {
            get
            {
                return canvasGroupComponent;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "CanvasGroupComponent" ), HideInInspector]
        private CanvasGroup                 canvasGroupComponent = null;
    }
}

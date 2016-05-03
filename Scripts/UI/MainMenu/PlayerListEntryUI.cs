using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nixin
{
    public class PlayerListEntryUI : UIActor
    {


        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

            layoutElementComponent = ConstructDefaultComponent<LayoutElement>( this, "LayoutElementComponent", layoutElementComponent );
        }


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
                UpdateComponent.UpdateRate      = 0.1f;
            }
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );
            SetText();
        }


        public PlayerLobbyMetaData PlayerMetaData
        {
            get
            {
                return playerMetaData;
            }
            set
            {
                playerMetaData = value;
                SetText();
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "LayoutElementComponent" ), HideInInspector]
        private LayoutElement               layoutElementComponent = null;

        [SerializeField, FormerlySerializedAs( "NameText" )]
        private Text                        nameText        = null;

        [SerializeField, FormerlySerializedAs( "PingText" )]
        private Text                        pingText        = null;

        private PlayerLobbyMetaData         playerMetaData  = null;


        private void SetText()
        {
            if( playerMetaData == null )
            {
                nameText.text       = "-";
                pingText.text       = "-";
                return;
            }

            nameText.text   = PlayerMetaData.Name;
            pingText.text   = PlayerMetaData.Ping.ToString();
        }
    }
}

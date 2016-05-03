using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class HudMessageSpawnerComponent : NixinComponent
    {


        // Public:


        public override void OnOwningActorInitialised()
        {
            base.OnOwningActorInitialised();

            if( Owner != null )
            {
                hudMessagePool = new UIActorPool<HudMessageUIActor>( Owner.ContainingWorld, hudMessagePrefab );
            }
        }


        public override void OnUnregistered()
        {
            base.OnUnregistered();

            if( hudMessagePool != null )
            {
                hudMessagePool.ClearPool( true );
                hudMessagePool = null;
            }
        }


        public void DisplayHudMessage( string msg, HudMessageType type )
        {
            if( Owner == null || Owner.ContainingWorld == null )
            {
                return;
            }
            SpawnHudMessage( msg, type );
        }
 

        public Transform MessageOrigin
        {
            get
            {
                return messageOrigin;
            }
        }


        // Private:


        [SerializeField]
        private HudMessageUIActor           hudMessagePrefab    = null;

        [SerializeField]
        private Transform                   messageOrigin       = null;

        private UIActorPool<HudMessageUIActor> hudMessagePool   = null;


        private void SpawnHudMessage( string msg, HudMessageType msgType )
        {
            if( Owner == null || Owner.ContainingWorld == null || hudMessagePool == null )
            {
                return;
            }

            var localPlayer = Owner.ContainingWorld.FirstLocalPlayer;
            if( localPlayer == null || localPlayer.CameraActor == null )
            {
                return;
            }

            var colour      = GetColorFromMessageType( msgType );
            var position    = localPlayer.CameraActor.CameraComponent.WorldToScreenPoint( messageOrigin.position );

            var newMessage  = hudMessagePool.GetOrInstantiate( localPlayer.Hud, position, Quaternion.identity );
            if( newMessage == null )
            {
                return;
            }

            newMessage.SetUpHudMessage( msg, colour, this, localPlayer );
        }


        private Color GetColorFromMessageType( HudMessageType type )
        {
            switch( type )
            {
                case HudMessageType.Positive:
                    return Color.green;
                case HudMessageType.Negative:
                    return Color.red;
                case HudMessageType.Passive:
                    return Color.white;
            }
            return Color.white;
        }
    }



    public enum HudMessageType
    {
        Positive = 0,
        Negative = 1,
        Passive = 2,
    }
}

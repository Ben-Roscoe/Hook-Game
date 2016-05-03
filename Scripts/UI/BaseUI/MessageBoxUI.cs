using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nixin
{
    public class MessageBoxUI : UIActor
    {


        // Public:



        public override void SetLocalText()
        {
            base.SetLocalText();

            if( titleTextId != null )
            {
                titleText.text = ContainingWorld.LocalisationSystem.GetLocalString( titleTextId );
            }
            if( bodyTextId != null )
            {
                bodyText.text = ContainingWorld.LocalisationSystem.GetLocalString( bodyTextId );
            }
            if( okayButtonTextId != null )
            {
                var okayButtonText = okayButton.GetComponentInAllChildren<Text>();
                if( okayButtonText != null )
                {
                    okayButtonText.text = ContainingWorld.LocalisationSystem.GetLocalString( okayButtonTextId );
                }
            }
        }


        public void SetLocalIds( string titleTextId, string bodyTextId, string okayButtonTextId )
        {
            this.titleTextId            = titleTextId;
            this.bodyTextId             = bodyTextId;
            this.okayButtonTextId       = okayButtonTextId;

            SetLocalText();
        }


        public Button OkayButton
        {
            get
            {
                return okayButton;
            }

        }


        public string TitleTextId
        {
            get
            {
                return titleTextId;
            }
        }


        public string BodyTextId
        {
            get
            {
                return bodyTextId;
            }
        }


        public string OkayButtonTextId
        {
            get
            {
                return okayButtonTextId;
            }
        }


        // Private:


        [SerializeField, FormerlySerializedAs( "TitleText" )]
        private Text        titleText   = null;

        [SerializeField, FormerlySerializedAs( "BodyText" )]
        private Text        bodyText    = null;

        [SerializeField, FormerlySerializedAs( "OkayButton" )]
        private Button      okayButton  = null;

        private string      titleTextId             = null;
        private string      bodyTextId              = null;
        private string      okayButtonTextId        = null;
    }
}

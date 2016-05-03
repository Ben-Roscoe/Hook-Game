using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nixin
{
    public abstract class GameVarUI : UIActor
    {


        // Public:


        public override void EditorConstruct()
        {
            base.EditorConstruct();

            layoutElementComponent = ConstructDefaultComponent<LayoutElement>( this, "LayoutElementComponent", layoutElementComponent );
        }


        public override void OnPostHierarchyInitialise()
        {
            base.OnPostHierarchyInitialise();

            resetToDefaultButton.onClick.AddListener( OnResetToDefaultButtonPressed );
            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
                UpdateComponent.UpdateRate      = 0.1f;
            }

            if( !ContainingWorld.NetworkSystem.IsAuthoritative )
            {
                SetInteractable( false );
            }
        }


        public override void SetInteractable( bool interactable )
        {
            base.SetInteractable( interactable );

            resetToDefaultButton.interactable = interactable;
        }


        public override void SetLocalText()
        {
            base.SetLocalText();


            if( gameVarDecl != null )
            {
                nameText.text = ContainingWorld.LocalisationSystem.GetLocalString( gameVarDecl.NameToken );
            }

            var resetToDefaultText = resetToDefaultButton.GetComponentInAllChildren<Text>();
            if( resetToDefaultText != null )
            {
                resetToDefaultText.text = ContainingWorld.LocalisationSystem.GetLocalString( LocalisationIds.MainMenu.ResetToDefault );
            }
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );
            if( GameVar != null )
            {
                UpdateGameVarValue();
            }
        }


        public void SetGameVarData( GameVarDeclAttribute gameVarDecl, GameVar gameVar )
        {
            this.gameVarDecl    = gameVarDecl;
            this.gameVar        = gameVar;

            if( GameVarDecl != null && GameVar != null )
            {
                OnGameVarDeclChanged();
                SetLocalText();
                ResetToDefault();
            }
        }


        public GameVarDeclAttribute GameVarDecl
        {
            get
            {
                return gameVarDecl;
            }
        }


        public GameVar GameVar
        {
            get
            {
                return gameVar;
            }
        }


        // Protected:


        protected virtual void ResetToDefault()
        {
        }


        protected virtual void OnGameVarDeclChanged()
        {
        }


        protected virtual void UpdateGameVarValue()
        {

        }


        // Private:


        [SerializeField, FormerlySerializedAs( "LayoutElementComponent" ), HideInInspector]
        private LayoutElement   layoutElementComponent = null;

        [SerializeField, FormerlySerializedAs( "NameText" )]
        private Text           nameText         = null;

        [SerializeField, FormerlySerializedAs( "ResetToDefaultButton" )]
        private Button         resetToDefaultButton = null;

        private GameVarDeclAttribute    gameVarDecl          = null;
        private GameVar        gameVar              = null;


        private void OnResetToDefaultButtonPressed()
        {
            ResetToDefault();
        }
    }
}

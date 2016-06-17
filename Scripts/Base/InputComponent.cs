using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System.Runtime.InteropServices;

#if !NSHIPPING  && UNITY_EDITOR
using UnityEditor;
#endif

namespace Nixin
{
    public enum InputState
    {
        Down,
        Release,
    }

    public class ActionBinding
    {
        public string InputName
        {
            get
            {
                return inputName;
            }
        }
        public Action Action
        {
            get
            {
                return action;
            }
        }
        public InputState InputState
        {
            get
            {
                return inputState;
            }
        }
        public NixinBehaviour Owner
        {
            get
            {
                return owner;
            }
        }

        public ActionBinding( string inputName, InputState inputState, Action action, NixinBehaviour owner )
        {
            this.inputName = inputName;
            this.action = action;
            this.inputState = inputState;
            this.owner = owner;
        }


        private string          inputName;
        private Action          action;
        private InputState      inputState;
        private NixinBehaviour  owner;
    }

    public class AxisBinding
    {
        public string AxisName
        {
            get
            {
                return axisName;
            }
        }
        public Action<float, float> AxisCallback
        {
            get
            {
                return axisCallback;
            }
        }
        public NixinBehaviour Owner
        {
            get
            {
                return owner;
            }
        }

        public AxisBinding( string axisName, Action<float, float> axisCallback, NixinBehaviour owner )
        {
            this.axisName = axisName;
            this.axisCallback = axisCallback;
            this.owner = owner;
        }

        private string          axisName;
        private Action<float, float>   axisCallback;
        private NixinBehaviour  owner;
    }

    public class TouchBinding
    {
        public Action Callback
        {
            get
            {
                return callback;
            }
        }
        public NixinBehaviour Owner
        {
            get
            {
                return owner;
            }
        }

        public TouchBinding( Action callback, NixinBehaviour owner )
        {
            this.callback = callback;
            this.owner = owner;
        }

        private Action                              callback;
        private NixinBehaviour                      owner;
    }

    public class InputComponent
    {


        // Public:


        public InputComponent( World world )
        {
            Assert.IsTrue( world != null, "Input component must be given a valid world." );
            this.world = world;
            this.world.OnApplicationFocusChanged.AddHandler( OnApplicationFocus );
        }


        public void Uninitialise()
        {
            if( world == null )
            {
                return;
            }
            world.OnApplicationFocusChanged.RemoveHandler( OnApplicationFocus );
        }


        public void ToggleLocallyEnabled( bool enable )
        {
            isLocallyEnabled = enable;
        }


        public void ToggleRemotelyEnabled( bool enabled )
        {
            isRemotelyEnabled = enabled;
        }


        public void UpdateInput( float deltaTime )
        {
            if( !IsEnabled )
            {
                return;
            }

            for( int i = 0; i < actionBindings.Count; ++i )
            {
                switch( actionBindings[i].InputState )
                {
                    case InputState.Down:
                        {
                            if( !Input.GetButtonDown( actionBindings[i].InputName ) )
                            {
                                continue;
                            }
                            actionBindings[i].Action.Invoke();
                            break;
                        }
                    case InputState.Release:
                        {
                            if( !Input.GetButtonUp( actionBindings[i].InputName ) )
                            {
                                continue;
                            }
                            actionBindings[i].Action.Invoke();
                            break;
                        }
                }
            }
            for( int i = 0; i < axisBindings.Count; ++i )
            {
                axisBindings[i].AxisCallback.Invoke( Input.GetAxis( axisBindings[i].AxisName ), deltaTime );
            }
            if( Input.touchCount > 0 )
            {
                for( int i = 0; i < touchBindings.Count; ++i )
                {
                    touchBindings[i].Callback.Invoke();
                }
            }
        }


        public void BindAction( string inputName, InputState inputState, Action action, NixinBehaviour owner )
        {
            actionBindings.Add( new ActionBinding( inputName, inputState, action, owner ) );
        }


        public void BindAxis( string axisName, Action<float, float> axisCallback, NixinBehaviour owner )
        {
            axisBindings.Add( new AxisBinding( axisName, axisCallback, owner ) );
        }


        public void BindTouch( Action callback, NixinBehaviour owner )
        {
            touchBindings.Add( new TouchBinding( callback, owner ) );
        }


        public void RemoveAction( string inputName, InputState inputState, Action action, NixinBehaviour owner )
        {
            actionBindings.RemoveAll( ( x ) => { return x.Action == action && x.InputName == inputName && x.InputState == inputState && x.Owner == owner; } );
        }


        public void RemoveAxis( string axisName, Action<float, float> axisCallback, NixinBehaviour owner )
        {
            axisBindings.RemoveAll( ( x ) => { return x.AxisName == axisName && x.AxisCallback == axisCallback 
                && x.Owner == owner; } );
        }


        public void RemoveTouch( Action callback, NixinBehaviour owner )
        {
            touchBindings.RemoveAll( ( x ) => { return x.Callback == callback && x.Owner == owner; } );
        }


        public void RemoveAllWithOwner( NixinBehaviour owner )
        {
            actionBindings.RemoveAll( ( x ) => { return x.Owner == owner; } );
            axisBindings.RemoveAll( ( x ) => { return x.Owner == owner; } );
            touchBindings.RemoveAll( ( x ) => { return x.Owner == owner; } );
        }


        public Vector3? MousePosition
        {
            get
            {
                return ( ApplicationHasFocus ) ? Input.mousePosition : new Vector3?();
            }
        }


        public bool IsLocallyEnabled
        {
            get
            {
                return isLocallyEnabled;
            }
        }


        public bool IsRemotelyEnabled
        {
            get
            {
                return isRemotelyEnabled;
            }
        }


        public bool IsEnabled
        {
            get
            {
                return IsLocallyEnabled && IsRemotelyEnabled;
            }
        }


        public bool ApplicationHasFocus
        {
            get
            {
#if !NSHIPPING && UNITY_EDITOR
                if( world == null )
                {
                    return applicationHasFocus && EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.titleContent.text == gameViewWindowTitle;
                }
                else
#endif
                {
                    return applicationHasFocus;
                }
            }
        }


        // Private:


        // Make sure we have the game view focusd in editor before doing any input.
#if !NSHIPPING
        private const string                        gameViewWindowTitle  = "Game";
#endif

        private List<ActionBinding>                 actionBindings  = new List<ActionBinding>();
        private List<AxisBinding>                   axisBindings    = new List<AxisBinding>();
        private List<TouchBinding>                  touchBindings   = new List<TouchBinding>();

        private bool                                isLocallyEnabled    = true;
        private bool                                isRemotelyEnabled   = true;
        private bool                                applicationHasFocus         = true;

        private World                               world                       = null;


        private void OnApplicationFocus( bool focusStatus )
        {
            applicationHasFocus = focusStatus;
        }
    }
}
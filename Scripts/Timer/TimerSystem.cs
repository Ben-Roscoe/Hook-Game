using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nixin
{
    public delegate void TimerDelegate();

    public class TimerSystem : NixinSystem
    {


        // Public:


        public TimerSystem( World containingWorld ) : base( containingWorld )
        {
        }


        public TimerHandle SetTimerHandle( object timerOwner, TimerDelegate method, int repeatAmount, float length )
        {
            var timerHandle = new TimerHandle( timerOwner, method, repeatAmount, length );
            timerHandles.Add( timerHandle );
            return timerHandle;
        }


        public void RemoveTimerHandle( TimerHandle timerHandle )
        {
            if( timerHandles.Contains( timerHandle ) )
            {
                timerHandle.Completed = true;
            }
        }


        public void RemoveTimerHandlesWithOwner( object owner )
        {
            for( int i = 0; i < timerHandles.Count; ++i )
            {
                if( timerHandles[i].TimerOwner == owner )
                {
                    timerHandles[i].Completed = true;
                }
            }
        }


        public void Update()
        {
            for( int i = 0; i < timerHandles.Count; ++i )
            {
                if( timerHandles[i].Completed )
                {
                    timerHandles.RemoveAt( i );
                    --i;
                    continue;
                }
                timerHandles[i].Update();
            }
        }


        // Private:


        private List<TimerHandle> timerHandles = new List<TimerHandle>();
    }


    public class TimerHandle
    {


        // Public:


        public TimerHandle( object timerOwner, TimerDelegate method, int repeatAmount, float length )
        {
            this.timerOwner         = timerOwner;
            this.method             = method;
            this.repeatAmount       = repeatAmount;
            this.length             = length;

            this.currentTime        = 0.0f;
            this.invokedAmount      = 0;
            this.completed          = false;

            Assert.IsTrue( method != null );

            if( timerOwner is UnityEngine.Object )
            {
                isUnityObject   = true;
                unityObject     = timerOwner as UnityEngine.Object;
            }
        }


        public void Update()
        {
            // Unity object has been destroyed. Remove the timer.
            if( isUnityObject && unityObject == null )
            {
                completed = true;
                return;
            }

            currentTime += Time.deltaTime;
            if( currentTime < length )
            {
                return;
            }

            method();

            // If we're done, flag the completed timer. Otherwise reset it.
            ++invokedAmount;
            if( repeatAmount >= 0 && invokedAmount > repeatAmount )
            {
                completed = true;
            }
            else
            {
                currentTime = 0.0f;
            }
        }


        public bool Completed
        {
            get
            {
                return completed;
            }
            set
            {
                completed = value;
            }
        }


        public double TimeRemaining
        {
            get
            {
                return length - currentTime;
            }
        }


        public double TimePassed
        {
            get
            {
                return currentTime;
            }
        }


        public object TimerOwner
        {
            get
            {
                return timerOwner;
            }
        }


        public int RepeatAmount
        {
            get
            {
                return repeatAmount;
            }
        }


        public double Length
        {
            get
            {
                return length;
            }
        }


        // Private:


        private object          timerOwner      = null;
        private TimerDelegate   method          = null;

        // -1 for infinite repeating.
        private int             repeatAmount    = -1;
        private int             invokedAmount   = 0;

        private float           currentTime     = 0.0f;
        private float           length          = 0.0f;

        private bool            completed       = false;

        private UnityEngine.Object unityObject  = null;
        private bool            isUnityObject   = false;
    }


    public class TimerDelegateException : Exception
    {


        public TimerDelegateException()
        {

        }


        public TimerDelegateException( string message ) : base( message )
        {

        }
    }
}

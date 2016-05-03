using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nixin
{
    public class AnimatorParameter
    {


        // Public:


        public AnimatorParameter( string parameterName, Animator animator )
        {
            this.parameterName = parameterName;
            this.animator      = animator;
            this.parameterId   = Animator.StringToHash( parameterName );
        }
        

        public void SetInt( int v )
        {
            animator.SetInteger( parameterId, v );
        }


        public void SetBool( bool v )
        {
            animator.SetBool( parameterId, v );
        }


        public void SetFloat( float v )
        {
            animator.SetFloat( parameterId, v );
        }


        public void SetTrigger()
        {
            animator.SetTrigger( parameterId );
        }


        public int GetInt()
        {
            return animator.GetInteger( parameterId );
        }


        public bool GetBool()
        {
            return animator.GetBool( parameterId );
        }


        public float GetFloat()
        {
            return animator.GetFloat( parameterId );
        }


        public string ParameterName
        {
            get
            {
                return parameterName;
            }
        }


        public int ParameterId
        {
            get
            {
                return parameterId;
            }
        }


        public Animator Animator
        {
            get
            {
                return animator;
            }
        }


        // Private:


        private string      parameterName = "";
        private int         parameterId   = -1;
        private Animator    animator      = null;
    }
}

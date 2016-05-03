using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public enum GameVarDataType
    {
        Int,
        Bool,
        Float,
    }
    public class GameVar
    {


        // Public:


        public GameVar( GameVarDeclAttribute decl )
        {
            this.decl = decl;
            type = GameVarDataType.Bool;
            if( decl is FloatGameVarDeclAttribute )
            {
                type = GameVarDataType.Float;
            }
            else if( decl is IntGameVarDeclAttribute )
            {
                type = GameVarDataType.Int;
            }
        }


        public void Write( NetBuffer buffer )
        {
            switch( type )
            {
                case GameVarDataType.Bool: buffer.Write( GetBool() ); break;
                case GameVarDataType.Float: buffer.Write( GetFloat() ); break;
                case GameVarDataType.Int: buffer.Write( GetInt() ); break;
            }
        }


        public void Read( NetBuffer buffer, bool isFuture )
        {
            switch( type )
            {
                case GameVarDataType.Bool: SetBool( buffer.ReadBoolean( GetBool(), isFuture ) ); break;
                case GameVarDataType.Float: SetFloat( buffer.ReadFloat( GetFloat(), isFuture ) ); break;
                case GameVarDataType.Int: SetInt( buffer.ReadInt32( GetInt(), isFuture ) ); break;
            }
        }


        public void SetInt( int value )
        {
            intValue = value;
        }


        public void SetFloat( float value )
        {
            floatValue = value;
        }


        public void SetBool( bool value )
        {
            boolValue = value;
        }


        public int GetInt()
        {
            return intValue;
        }


        public float GetFloat()
        {
            return floatValue;
        }


        public bool GetBool()
        {
            return boolValue;
        }


        public GameVarDeclAttribute Decl
        {
            get
            {
                return decl;
            }
        }


        public GameVarDataType Type
        {
            get
            {
                return type;
            }
        }


        // Private:


        private GameVarDeclAttribute        decl        = null;
        private GameVarDataType             type        = GameVarDataType.Bool;

        private int                         intValue    = 0;
        private float                       floatValue  = 0.0f;
        private bool                        boolValue   = false;

    }
}

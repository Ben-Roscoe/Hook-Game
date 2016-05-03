
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nixin
{
    public delegate void RPCMethodDelegate();
    public delegate void RPCMethodDelegate<A>( A a );
    public delegate void RPCMethodDelegate<A, B>( A a, B b );
    public delegate void RPCMethodDelegate<A, B, C>( A a, B b, C c );
    public delegate void RPCMethodDelegate<A, B, C, D>( A a, B b, C c, D d );
    public delegate void RPCMethodDelegate<a, B, C, D, E>( a a, B b, C c, D d, E e );
    public delegate void RPCMethodDelegate<a, B, C, D, E, F>( a a, B b, C c, D d, E e, F f );


    public enum RPCParameterId
    {
        Invalid     = 0,
        Byte        = 1,
        Int16       = 2,
        Int32       = 3,
        Int64       = 4,
        UInt16      = 5,
        UInt32      = 6,
        UInt64      = 7,
        Float       = 8,
        String      = 9,
        Vector3     = 10,
        Quaternion  = 11,
        ActorId     = 12,
        NullId      = 13,
        ComponentId = 14,
        Bool        = 16,
    }


    public enum RPCCallType
    {
        Server,
        Multicast,
        Local,
        InvalidCall,
    }


    public enum RPCType
    {
        Multicast,
        Server,
    }


    public class RPCMethod
    {
        public MethodInfo           MethodInfo      { get; set; }
        public object               Owner           { get; set; }
        public byte                 Id              { get; set; }


        public RPCMethod( MethodInfo methodInfo, object owner, byte id )
        {
            this.MethodInfo     = methodInfo;
            this.Owner          = owner;
            this.Id             = id;
        }
    }


    public class RPCParameter
    {
        public object               Data            { get; set; }
        public RPCParameterId       ParameterId     { get; set; }

        public RPCParameter( object data, RPCParameterId parameterId )
        {
            Data            = data;
            ParameterId     = parameterId;
        }
    }


    public class RPCSenderData
    {
        public bool                 Reliable        { get; set; }
        public bool                 IsRelevantToAll { get; set; }
        public int                  NetworkStep     { get; set; }


        public RPCSenderData( bool reliable, bool isRelevantToAll, int networkStep )
        {
            this.Reliable           = reliable;
            this.IsRelevantToAll    = isRelevantToAll;
            this.NetworkStep        = networkStep;
        }
    }


    public class RPCCall
    {


        // Public:


        public static RPCCallType GetCallType( RPCType rpcType, bool isServer, bool isAuthorityOfDestinationBehaviour )
        {
            if( rpcType == RPCType.Server )
            {
                if( isServer )
                {
                    return RPCCallType.Local;
                }
                else if( isAuthorityOfDestinationBehaviour )
                {
                    return RPCCallType.Local;
                }
                return RPCCallType.Server;
            }
            if( rpcType == RPCType.Multicast )
            {
                if( !isServer )
                {
                    return RPCCallType.InvalidCall;
                }
                return RPCCallType.Multicast;
            }
            return RPCCallType.InvalidCall;
        }


        public static RPCParameterId GetParameterIdFromData( object parameter )
        {
            if( parameter is byte )
            {
                return RPCParameterId.Byte;
            }
            if( parameter is short )
            {
                return RPCParameterId.Int16;
            }
            if( parameter is int )
            {
                return RPCParameterId.Int32;
            }
            if( parameter is bool )
            {
                return RPCParameterId.Bool;
            }
            if( parameter is long )
            {
                return RPCParameterId.Int64;
            }
            if( parameter is UInt16 )
            {
                return RPCParameterId.UInt16;
            }
            if( parameter is UInt32 )
            {
                return RPCParameterId.UInt32;
            }
            if( parameter is UInt64 )
            {
                return RPCParameterId.UInt64;
            }
            if( parameter is float )
            {
                return RPCParameterId.Float;
            }
            if( parameter is string )
            {
                return RPCParameterId.String;
            }
            if( parameter is Vector3 )
            {
                return RPCParameterId.Vector3;
            }
            if( parameter is Quaternion )
            {
                return RPCParameterId.Quaternion;
            }
            if( parameter is NixinComponent )
            {
                return RPCParameterId.ComponentId;
            }
            if( parameter is Actor )
            {
                return RPCParameterId.ActorId;
            }
            if( parameter == null )
            {
                return RPCParameterId.NullId;
            }
            return RPCParameterId.Invalid;
        }


        // Message must be pointing to the top of the RPC data.
        public RPCCall( NetIncomingMessage message )
        {
            ReadRPC( message );
        }


        public RPCCall( UInt16 destinationActorId, byte methodId, List<RPCParameter> parameters, bool reliable, bool isRelevantToAll, int networkStep )
        {
            this.destinationActorId     = destinationActorId;
            this.methodId               = methodId;
            this.parameters             = parameters;

            this.senderData             = new RPCSenderData( reliable, isRelevantToAll, networkStep );
        }


        public void ReadRPC( NetIncomingMessage msg )
        {
            destinationActorId              = msg.ReadUInt16();
            methodId                        = msg.ReadByte();

            int parameterCount = msg.ReadByte();
            for( int i = 0; i < parameterCount; ++i )
            {
                var         parameterId         = ( RPCParameterId )msg.ReadByte();
                object      parameterData       = ReadParameter( msg, parameterId );
                parameters.Add( new RPCParameter( parameterData, parameterId ) );
            }
        }


        public void WriteRPC( NetOutgoingMessage msg )
        {
            msg.Write( ( UInt16 )destinationActorId );
            msg.Write( ( byte )methodId );
            
            msg.Write( ( byte )parameters.Count );
            for( int i = 0; i < parameters.Count; ++i )
            {
                WriteParameter( msg, parameters[i] );
            }
        }


        public List<RPCParameter> Parameters
        {
            get
            {
                return parameters;
            }
        }


        public UInt16 DestinationActorId
        {
            get
            {
                return destinationActorId;
            }
            set
            {
                destinationActorId = value;
            }
        }


        public byte MethodId
        {
            get
            {
                return methodId;
            }
            set
            {
                methodId = value;
            }
        }


        public RPCSenderData SenderData
        {
            get
            {
                return senderData;
            }
        }


        // Private:


        private byte                        methodId                = 0;
        private UInt16                      destinationActorId      = 0;
        private List<RPCParameter>          parameters              = new List<RPCParameter>();

        private RPCSenderData               senderData              = null;


        private void WriteParameter( NetOutgoingMessage message, RPCParameter parameter )
        {
            message.Write( ( byte )parameter.ParameterId );
            switch( parameter.ParameterId )
            {
                case RPCParameterId.Byte:
                {
                    message.Write( ( byte )parameter.Data );
                    return;
                }
                case RPCParameterId.Bool:
                {
                    message.Write( ( bool )parameter.Data );
                    return;
                }
                case RPCParameterId.Int16:
                {
                    message.Write( ( short )parameter.Data );
                    return;
                }
                case RPCParameterId.Int32:
                {
                    message.Write( ( int )parameter.Data );
                    return;
                }
                case RPCParameterId.Int64:
                {
                    message.Write( ( long )parameter.Data );
                    return;
                }
                case RPCParameterId.UInt16:
                { 
                    message.Write( ( UInt16 )parameter.Data );
                    return;
                }
                case RPCParameterId.UInt32:
                {
                    message.Write( ( UInt32 )parameter.Data );
                    return;
                }
                case RPCParameterId.UInt64:
                {
                    message.Write( ( UInt64 )parameter.Data );
                    return;
                }
                case RPCParameterId.Float:
                {
                    message.Write( ( float )parameter.Data );
                    return;
                }
                case RPCParameterId.String:
                {
                    message.Write( ( string )parameter.Data );
                    return;
                }
                case RPCParameterId.Vector3:
                {
                    Vector3 vector = ( Vector3 )parameter.Data;
                    message.Write( vector.x );
                    message.Write( vector.y );
                    message.Write( vector.z );
                    return;
                }
                case RPCParameterId.Quaternion:
                {
                    Quaternion quaternion = ( Quaternion )parameter.Data;
                    message.Write( quaternion.x );
                    message.Write( quaternion.y );
                    message.Write( quaternion.z );
                    message.Write( quaternion.w );
                    return;
                }
                case RPCParameterId.ActorId:
                {
                    message.Write( ( ( Actor )parameter.Data ).Id );
                    return;
                }
                case RPCParameterId.ComponentId:
                {
                    message.Write( ( ( NixinComponent )parameter.Data ).Owner.Id );
                    message.Write( ( ( NixinComponent )parameter.Data ).ComponentId );
                    return;
                }
                case RPCParameterId.NullId:
                {
                    return;
                }
            }
            throw new InvalidRPCParameterIdException( "Could not write the given RPC parameter id: " + parameter.ParameterId );
        }


        private object ReadParameter( NetIncomingMessage message, RPCParameterId id )
        {
            switch( id )
            {
                case RPCParameterId.Byte:
                {
                    return message.ReadByte();
                }
                case RPCParameterId.Bool:
                {
                    return message.ReadBoolean();
                }
                case RPCParameterId.Int16:
                {
                    return message.ReadInt16();
                }
                case RPCParameterId.Int32:
                {
                    return message.ReadInt32();
                }
                case RPCParameterId.Int64:
                {
                    return message.ReadInt64();
                }
                case RPCParameterId.UInt16:
                {
                    return message.ReadUInt16();
                }
                case RPCParameterId.UInt32:
                {
                    return message.ReadUInt32();
                }
                case RPCParameterId.UInt64:
                {
                    return message.ReadUInt64();
                }
                case RPCParameterId.Float:
                {
                    return message.ReadFloat();
                }
                case RPCParameterId.String:
                {
                    return message.ReadString();
                }
                case RPCParameterId.Vector3:
                {
                    Vector3 vector = Vector3.zero;
                    vector.x       = message.ReadFloat();
                    vector.y       = message.ReadFloat();
                    vector.z       = message.ReadFloat();
                    return vector;
                }
                case RPCParameterId.Quaternion:
                {
                    Quaternion quaternion = Quaternion.identity;
                    quaternion.x          = message.ReadFloat();
                    quaternion.y          = message.ReadFloat();
                    quaternion.z          = message.ReadFloat();
                    quaternion.w          = message.ReadFloat();
                    return quaternion;
                }
                case RPCParameterId.ActorId:
                {
                    return message.ReadUInt16();
                }
                case RPCParameterId.ComponentId:
                {
                    return new KeyValuePair<UInt16, byte>( message.ReadUInt16(), message.ReadByte() );
                }
                case RPCParameterId.NullId:
                {
                    return null;
                }
            }
            throw new InvalidRPCParameterIdException( "Could not read the given RPC parameter id: " + id );
        }
    }


    public class InvalidRPCParameterIdException : System.Exception
    {
        public InvalidRPCParameterIdException()
        {

        }


        public InvalidRPCParameterIdException( string message ) : base( message )
        {

        }
    }


    public class InvalidRPCCallException : System.Exception
    {
        public InvalidRPCCallException()
        {

        }


        public InvalidRPCCallException( string message ) : base( message )
        {

        }
    }
}
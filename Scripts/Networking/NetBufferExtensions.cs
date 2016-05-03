using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public static class NetBufferExtensions
    {
        public static void WriteActor( this NetBuffer buffer, Actor actor )
        {
            buffer.Write( actor != null ? ( UInt16 )actor.Id : ( UInt16 )0 );
        }


        public static T ReadActor<T>( this NetBuffer buffer, World world ) where T : Actor
        {
            var id = buffer.ReadUInt16();
            return world.GetReplicatedActor( id ) as T;
        }


        public static T ReadActor<T>( this NetBuffer buffer, World world, T current, bool isFuture ) where T : Actor
        {
            var id      = buffer.ReadUInt16();

            if( !isFuture )
            {
                var actor   = world.GetReplicatedActor( id ) as T;
                return actor;
            }
            return current;
        }


        public static Int32 ReadInt32( this NetBuffer buffer, Int32 current, bool isFuture )
        {
            Int32 ret = buffer.ReadInt32();
            return isFuture ? current : ret;
        }


        public static Int16 ReadInt16( this NetBuffer buffer, Int16 current, bool isFuture )
        {
            Int16 ret = buffer.ReadInt16();
            return isFuture ? current : ret;
        }


        public static float ReadFloat( this NetBuffer buffer, float current, bool isFuture )
        {
            float ret = buffer.ReadFloat();
            return isFuture ? current : ret;
        }


        public static double ReadDouble( this NetBuffer buffer, double current, bool isFuture )
        {
            double ret = buffer.ReadDouble();
            return isFuture ? current : ret;
        }


        public static byte ReadByte( this NetBuffer buffer, byte current, bool isFuture )
        {
            byte ret = buffer.ReadByte();
            return isFuture ? current : ret;
        }


        public static bool ReadBoolean( this NetBuffer buffer, bool current, bool isFuture )
        {
            bool ret = buffer.ReadBoolean();
            return isFuture ? current : ret;
        }


        public static string ReadString( this NetBuffer buffer, string current, bool isFuture )
        {
            string ret = buffer.ReadString();
            return isFuture ? current : ret;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using UnityEngine;

namespace Nixin
{
    public class MovementComponent : NixinComponent
    {


        // Public:



        public override void OnRegistered( Actor owner, byte id )
        {
            base.OnRegistered( owner, id );

            interpolatedPosition.From = transform.position;
            interpolatedPosition.To   = transform.position;

            interpolatedRotation.From = transform.rotation;
            interpolatedRotation.To   = transform.rotation;

            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
            }
        }


        public override void WriteSnapshot( NetBuffer buffer )
        {
            base.WriteSnapshot( buffer );

            buffer.Write( Owner.Position.x );
            buffer.Write( Owner.Position.y );
            buffer.Write( Owner.Position.z );

            buffer.Write( Owner.Rotation.x );
            buffer.Write( Owner.Rotation.y );
            buffer.Write( Owner.Rotation.z );
            buffer.Write( Owner.Rotation.w );
        }


        public override void ReadSnapshot( NetBuffer buffer, bool isFuture )
        {
            base.ReadSnapshot( buffer, isFuture );

            if( !isFuture )
            {
                interpolatedPosition.From   = new Vector3( buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat() );
                interpolatedRotation.From   = new Quaternion( buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat() );
                //interpolatedPosition.From = transform.position;
            }
            else
            {
                interpolatedPosition.To     = new Vector3( buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat() );
                interpolatedRotation.To     = new Quaternion( buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat() );
            }
        }


        public override void OnUpdate( float deltaTime )
        {
            base.OnUpdate( deltaTime );

            if( Owner != null && !Owner.IsAuthority )
            {
                transform.position = interpolatedPosition.GetWorldNetworkInterpolatedVector( Owner.ContainingWorld );
                transform.rotation = interpolatedRotation.GetWorldNetworkInterpolatedQuaternion( Owner.ContainingWorld );
            }
        }


        public override void PreSendSnapshot( int networkStep )
        {
            base.PreSendSnapshot( networkStep );

            if( Owner == null || !Owner.IsAuthority )
            {
                return;
            }
        }


        public void MoveDirection( Vector3 direction, float amount )
        {
            if( Owner != null && Owner.IsAuthority )
            {
                transform.Translate( direction * amount, Space.World );
            }
        }


        public void MoveDirectionSelf( Vector3 direction, float amount )
        {
            if( Owner != null && Owner.IsAuthority )
            {
                transform.Translate( direction * amount, Space.Self );
            }
        }


        public void MoveToPosition( Vector3 position )
        {
            if( Owner != null && Owner.IsAuthority )
            {
                transform.position = position;
            }
        }


        public void Extrapolate( Vector3 to )
        {
            if( Owner != null && Owner.IsAuthority )
            {
                transform.position = Vector3.Lerp( transform.position, to, 0.5f );
            }
        }


        public Vector3 Position
        {
            get
            {
                return transform.position;
            }
        }


        // Private:


        NetInterpolatedVector       interpolatedPosition    = new NetInterpolatedVector();
        NetInterpolatedQuaternion   interpolatedRotation    = new NetInterpolatedQuaternion();
    }



}

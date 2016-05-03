using UnityEngine;
using System.Collections;

namespace Nixin
{
	public class SparkCollisionActor : Actor
	{
		public override void EditorConstruct ()
		{
			base.EditorConstruct ();
			sparkParticle 	= ConstructDefaultComponent<ParticleSystem> ( this, "SparkParticle", sparkParticle );
			sparkLight 		= ConstructDefaultComponent<Light> ( this, "sparkLight", sparkLight );
        }

		public override void OnActorInitialise ( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
		{
			base.OnActorInitialise ( replicates, networkOwner, acceptsNewConnections, responsibleController );
			ContainingWorld.TimerSystem.SetTimerHandle ( this, EndLifeTime, 0, sparkParticle.duration );

            if( UpdateComponent.UseActorDefaultValues )
            {
                UpdateComponent.UpdateGroupType = UpdateGroupType.Update;
                UpdateComponent.UpdateRate = 0.0f;
            }
        }

		public override void OnUpdate( float deltaTime )
		{
			base.OnUpdate( deltaTime );
			sparkLight.intensity =  lightPowerCurve.Evaluate( sparkParticle.time / sparkParticle.duration) * lightMax;
		}
		private const float lightMax = 8.0f;

		[HideInInspector, SerializeField]
		private ParticleSystem sparkParticle = null;

		[HideInInspector, SerializeField]
		private Light sparkLight = null;

		[SerializeField]
		private AnimationCurve lightPowerCurve = null;

		private void EndLifeTime()
		{
			ContainingWorld.DestroyActor( this );
		}
	}
}
using UnityEngine;
using System.Collections;

public class ParticleLight : MonoBehaviour 
{
	public bool timeWithBurst 	= false;
	public ParticleSystem referenceParticle;
	public AnimationCurve lifeTime = null;
	
	private float currentTime = 0;

	// Use this for initialization
	void Start () 
	{
		gameObject.GetComponent<Light>().range = lifeTime.Evaluate( 0 );
	}
	
	// Update is called once per frame
	void Update () 
	{
		currentTime += Time.deltaTime;

		gameObject.GetComponent<Light>().range = lifeTime.Evaluate( currentTime / referenceParticle.startLifetime );
	}
}

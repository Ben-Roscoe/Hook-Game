using UnityEngine;
using System.Collections;

public class ChangeAmbientLightingOnPosition : MonoBehaviour {
	
	public float transitionRate = 10;

	public Color colour1 = Color.red;
	public Color colour2 = Color.blue;
	public Color midColour = Color.grey;

	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
		if ( this.transform.position.x < 0 )
		{
			RenderSettings.ambientLight = Color.Lerp( midColour, colour1, Mathf.Abs( this.transform.position.x ) / transitionRate );
		}
		else if ( this.transform.position.x > 0 )
		{
			RenderSettings.ambientLight = Color.Lerp( midColour, colour2, Mathf.Abs( this.transform.position.x ) / transitionRate );
		}
		else
		{
			RenderSettings.ambientLight = midColour;
		}
	}
}

using UnityEngine;
using System.Collections;

public class AutoRotation : MonoBehaviour 
{
	public float rotationSpeed = 0;

	public bool x = false;
	public bool y = false;
	public bool z = false;
	// Update is called once per frame
	void Update () 
	{
		this.transform.Rotate ( new Vector3( x ? 1.0f : 0.0f, y ? 1.0f : 0.0f, z ? 1.0f : 0.0f ) , rotationSpeed );
	}
}

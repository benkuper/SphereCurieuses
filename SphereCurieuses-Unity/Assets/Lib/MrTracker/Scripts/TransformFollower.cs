using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformFollower : MonoBehaviour {

	public Transform source;
	public Vector3 offset;

	//public Vector3 rotationOffset;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (source == null)
			return;

		transform.position = source.position + offset;
		transform.rotation = source.rotation;
	}
}

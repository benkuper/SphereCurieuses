using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraProjectorSync : MonoBehaviour {

	private Camera sourceCam;
	Projector projector;
	// Use this for initialization
	void Start () {
		if (sourceCam == null) sourceCam = transform.parent.GetComponent<Camera> ();
		projector = GetComponent<Projector> ();
	}

	
	// Update is called once per frame
	void Update () {
		if (sourceCam == null)
			return;

		projector.fieldOfView = sourceCam.fieldOfView;
		if (sourceCam.targetTexture != null) {
			projector.aspectRatio = sourceCam.targetTexture.width*1.0f / sourceCam.targetTexture.height;
		}

	}
}

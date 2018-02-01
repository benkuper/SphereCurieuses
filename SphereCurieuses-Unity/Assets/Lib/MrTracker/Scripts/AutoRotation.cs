using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotation : MonoBehaviour {

    public Vector3 speed;
    public bool random;

    public int seed;
    Vector3 rand3;
	// Use this for initialization
	void Start () {
        Random.InitState(seed);
        rand3 = Random.insideUnitSphere;
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(speed,Space.World);
        if(random) transform.Rotate(Vector3.Scale(rand3,speed) * .17f, Space.Self);
	}
}

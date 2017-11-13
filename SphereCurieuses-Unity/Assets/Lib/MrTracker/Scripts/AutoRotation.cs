using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotation : MonoBehaviour {

    [Range(0,10)]
    public float speed;

    public int seed;
    Vector3 rand3;
	// Use this for initialization
	void Start () {
        Random.InitState(seed);
        rand3 = Random.insideUnitSphere;
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(rand3 * speed,Space.World);
        transform.Rotate(rand3 * speed * .17f, Space.Self);
	}
}

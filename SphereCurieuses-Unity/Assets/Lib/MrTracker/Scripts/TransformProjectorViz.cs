using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TransformProjectorViz : MonoBehaviour {

    public Transform target;

    Transform xTrans;
    Transform yTrans;
    Transform zTrans;

	// Use this for initialization
	void Start () {
        xTrans = transform.Find("XProj");
        yTrans = transform.Find("YProj");
        zTrans = transform.Find("ZProj");
    }
	
	// Update is called once per frame
	void Update () {
        if (!target) return;
        Vector3 origin = transform.TransformPoint(Vector3.zero);
        xTrans.position = new Vector3(origin.x, target.position.y, target.position.z);
        yTrans.position = new Vector3(target.position.x, origin.y, target.position.z);
        zTrans.position = new Vector3(target.position.x, target.position.y, origin.z);
    }
}

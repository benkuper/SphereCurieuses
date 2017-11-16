using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneLookAt : MonoBehaviour {

    public static DroneLookAt instance;

    public SCController[] controllers;
	
    // Use this for initialization
	void Awake () {
        instance = this;    
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 target = Vector3.zero;
        foreach (SCController sc in controllers)
        {
            target += sc.transform.position;
            Debug.DrawLine(transform.position, sc.transform.position, Color.grey);
        }

        target /= controllers.Length;
        transform.position = target;
	}

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, .1f);
    }
}

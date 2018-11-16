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
        int goodTargets = 0;
        foreach (SCController sc in controllers)
        {
            if (sc.trackableID == -1) continue;
            target += sc.transform.position;
            goodTargets++;
            Debug.DrawLine(transform.position, sc.transform.position, Color.grey);
        }

        if(goodTargets > 0) target /= goodTargets;
        transform.position = target;
	}

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, .1f);
    }
}

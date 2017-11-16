using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TrackableObject : MonoBehaviour
{
    public int trackableID;
    public Trackable trackable;


	// Update is called once per frame
	public virtual void Update () {
        if (trackable == null) return;

        //Debug.Log(trackable.position + "/" + trackable.rotation + "/" + trackable.size);
        if (float.IsInfinity(trackable.rotation.x) || float.IsNaN(trackable.rotation.x)) return;


        transform.localPosition = new Vector3(trackable.position.x, trackable.position.y, trackable.position.z);
        transform.localRotation = trackable.rotation;
        if(trackable.sourceType == 1) //1 == MrTrackerClient.SourceType.AUGMENTA
            transform.localScale = trackable.size;
	}

    public virtual void setTrackable(Trackable t)
    {
        trackable = t;
        trackableID = t.id;
        gameObject.name = "Trackable " + t.id;
    }

    public virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.TransformPoint(Vector3.right*.5f));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.TransformPoint(Vector3.up * .5f));
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.TransformPoint(Vector3.forward * .5f));
    }

}

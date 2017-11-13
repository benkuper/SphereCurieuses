using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TrackableAutoManager : MonoBehaviour
{
    public MrTrackerClient client;
    public MrTrackerClient.ViveTypeFilter filter;


    public GameObject trackableObjectPrefab;
    public List<TrackableObject> trackableObjects;


    // Use this for initialization
    void Start()
    {
        trackableObjects = new List<TrackableObject>();

        client.trackableAdded += trackableAdded;
        client.trackableRemoved += trackableRemoved;
    }

    // Update is called once per frame
    void Update()
    {
   
    }

    void trackableAdded(Trackable t)
    {
        //Debug.Log("Trackable added ! currentTrackIndex = "+currentTrackIndex+" / Type " + t.type);

        //if (trackableObjects.Length <= currentTrackIndex) return;
        
        

        if (filter != MrTrackerClient.ViveTypeFilter.ALL)
        {
            switch (t.type)
            {
                case 1:
                    if (filter != MrTrackerClient.ViveTypeFilter.HMD) return;
                    break;

                case 2:
                    if (filter != MrTrackerClient.ViveTypeFilter.CONTROLLER) return;
                    break;

                case 3:
                    if (filter != MrTrackerClient.ViveTypeFilter.LIGHTHOUSE) return;
                    break;

                case 4:
                    if (filter != MrTrackerClient.ViveTypeFilter.TRACKER) return;
                    break;
            }
        }

        TrackableObject o = getObjectForTrackable(t.id);
        if(o == null)
        {
            o = Instantiate(trackableObjectPrefab).GetComponent<TrackableObject>();
        }
        o.transform.parent = transform;
        o.setTrackable(t);
        trackableObjects.Add(o);

        //currentTrackIndex++;
        //objects.Add(t, o);
    }

    void trackableRemoved(Trackable t)
    {
        TrackableObject to = getObjectForTrackable(t.id);
        if (to != null)
        {
            trackableObjects.Remove(to);
            Destroy(to.gameObject);
            Debug.Log("Remove trackable object " + t.id);
        }else
        {
            Debug.Log("Remove trackable not found : " + t.id);
        }
        
        /*
        if (currentTrackIndex > 0)
        {
            currentTrackIndex--;
            //TrackableObject o = trackableObjects[currentTrackIndex];
        }
        Debug.Log("Trackable removed ! New trackindex " + currentTrackIndex);
        */
    }

    TrackableObject getObjectForTrackable(int id)
    {
        foreach(TrackableObject to in trackableObjects)
        {
            if (to.trackable.id == id) return to;
        }

        return null;
    }
}
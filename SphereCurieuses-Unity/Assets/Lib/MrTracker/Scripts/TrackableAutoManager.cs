using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TrackableAutoManager : Controllable
{
    public MrTrackerClient client;
    public MrTrackerClient.ViveTypeFilter filter;


    public GameObject trackableObjectPrefab;
    public List<TrackableObject> trackableObjects;


    public bool vibrateOnAdded;
    public bool vibrateOnLost;
    public int numVibrations = 6;

    public bool dynamicTrackerMode;

    // Use this for initialization
    void Start()
    {
        if(dynamicTrackerMode) trackableObjects = new List<TrackableObject>();

        client.trackableAdded += trackableAdded;
        client.trackableRemoved += trackableRemoved;
    }
    

    void trackableAdded(Trackable t)
    {
        //Debug.Log("Trackable added ! currentTrackIndex = "+t.id+" / Type " + t.type);

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
            if (dynamicTrackerMode)
            {
                o = Instantiate(trackableObjectPrefab).GetComponent<TrackableObject>();
                o.transform.parent = transform;
                trackableObjects.Add(o);
            }
            else
            {
                return;
            }
        }

        if(o != null)
        {
            Debug.Log("Set Trackable "+t.id);
            o.setTrackable(t);
            if(vibrateOnAdded)
            {
                TrackableObject[] tos = GetComponentsInChildren<TrackableObject>();
                foreach (TrackableObject to in tos)
                {
                    if (to.trackable == null) continue;
                    if (to.trackable.type == (int)MrTrackerClient.ViveTypeFilter.CONTROLLER) MrTrackerClient.instance.sendMultiVibrate(to.trackableID, numVibrations, .5f, 1, .25f);
                }
            }
        }
        //currentTrackIndex++;
        //objects.Add(t, o);
    }

    void trackableRemoved(Trackable t)
    {
        if (!dynamicTrackerMode) return;
        
        TrackableObject o = getObjectForTrackable(t.id);
        if (o != null)
        {
            trackableObjects.Remove(o);
            Destroy(o.gameObject);
           // Debug.Log("Remove trackable object " + t.id);
        }else
        {
          //  Debug.Log("Remove trackable not found : " + t.id);
        }

        if (vibrateOnLost)
        {
            TrackableObject[] tos = GetComponentsInChildren<TrackableObject>();
            foreach (TrackableObject to in tos)
            {
                if (to.trackable == null) continue;
                if(to.trackable.type == (int)MrTrackerClient.ViveTypeFilter.CONTROLLER) MrTrackerClient.instance.sendMultiVibrate(to.trackableID, numVibrations, .5f,1,.25f);
            }
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
        //Debug.Log("****** Get for id " + id);
        foreach (TrackableObject to in trackableObjects)
        {
            if (to.trackableID == id || to.trackableID == -1) return to; //-1 is "Free trackable"
        }

        //Debug.Log("Not found for id " + id);
        return null;
    }
}
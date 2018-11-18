using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[SerializeField]
public class ViveConfigData
{
    public Vector3 position;
    public Quaternion rotation;
    public int rightControllerID;
}

public class CalibrateAutoVive : Controllable {

    public bool loadAtStart;
    public int rightHandID;

    public TrackableObject trackable;
    public Transform calibTransform;

    public int firstLHId;
    public Vector3 originLHAbsolutePos;
    Quaternion originLHRotation;

    override public void Awake()
    {
        TargetScript = this;
        base.Awake();
    }

    // Use this for initialization
    void Start () {
        if (loadAtStart) loadConfig();
	}

    override public void Update()
    {
        base.Update();

        TrackableObject originLH = getTrackableWithId(firstLHId);
        if (originLH != null && originLH.transform.position != originLHAbsolutePos)
        {
            Debug.Log("LIGHTHOUSE CHANGED !!!");
            recalibrateLighthouses();
        }
    }



    [OSCMethod]
    public void calibrate()
    {
        if(trackable == null)
        {
            Debug.LogWarning("Trackable null, not calibrating");
            SCController[] controllers = GetComponentsInChildren<SCController>();
            foreach (SCController sc in controllers) if (sc.specktrHand == SpecktrOSC.Hand.Right) trackable = sc;
            return;
        }

        Transform tt = trackable.transform;

        transform.position = Vector3.up * transform.position.y;
        transform.rotation = Quaternion.identity;

        transform.localRotation = Quaternion.Euler(0, -tt.eulerAngles.y, 0) * calibTransform.localRotation;
        transform.position = new Vector3(-tt.position.x, -tt.localPosition.y, -tt.position.z) + calibTransform.transform.position;


        MrTrackerClient.instance.sendMultiVibrate(trackable.trackableID, 3, .2f, .6f, .1f);
    }

    [OSCMethod]
    public void recalibrateLighthouses()
    {
        TrackableObject originLH = getTrackableWithId(firstLHId);
        if (originLH == null) return;

        Transform initParent = transform.parent;

        bool initAlways = originLH.alwaysUpdate;
        originLH.alwaysUpdate = false;

        originLH.transform.parent = null;//, true);
        transform.parent = originLH.transform;
        originLH.transform.position = originLHAbsolutePos;
        originLH.transform.rotation = originLHRotation;

        transform.parent = null;
        originLH.transform.parent = transform;
        transform.parent = initParent;

        originLH.alwaysUpdate = initAlways;

        setOriginLightHouse();
    }

    TrackableObject getTrackableWithId(int id)
    {
        TrackableObject[] tos = GetComponentsInChildren<TrackableObject>();

        foreach (TrackableObject to in tos)
        {
            if (to.trackable != null && to.trackable.id == id) return to;
        }

        return null;
    }

    void setOriginLightHouse()
    {
        TrackableObject[] tos = GetComponentsInChildren<TrackableObject>();

        foreach (TrackableObject to in tos)
        {
            if (to.trackable != null && to.trackable.type == (int)MrTrackerClient.ViveTypeFilter.LIGHTHOUSE)
            {
                firstLHId = to.trackable.id;
                originLHAbsolutePos = to.transform.position;
                originLHRotation = to.transform.rotation;
                break;
            }
        }
    }


    public void loadConfig()
    {
        string path = Application.dataPath + "/viveConfig.json";
        string s = File.ReadAllText(path);
        ViveConfigData vc = JsonUtility.FromJson<ViveConfigData>(s);
        transform.position = vc.position;
        transform.rotation = vc.rotation;
        rightHandID = vc.rightControllerID;

        Debug.Log("Vive config loaded");        
    }

    public void saveConfig()
    {
        ViveConfigData vc = new ViveConfigData();
        vc.position = transform.position;
        vc.rotation = transform.rotation;
        if (trackable != null) vc.rightControllerID = trackable.GetComponent<SCController>().trackableID;


        string s = JsonUtility.ToJson(vc);
        string path = Application.dataPath + "/viveConfig.json";

        File.WriteAllText(path, s);

        Debug.Log("Vive config saved to " + path + "\n" + s);

        setOriginLightHouse();
    }
}

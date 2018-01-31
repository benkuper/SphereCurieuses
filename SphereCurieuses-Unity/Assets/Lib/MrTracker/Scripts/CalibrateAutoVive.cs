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

public class CalibrateAutoVive : OSCControllable {

    public bool loadAtStart;
    public int rightHandID;

    public Transform trackable;
    public Transform calibTransform; 

	// Use this for initialization
	void Start () {
        if (loadAtStart) loadConfig();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    [OSCMethod("calibrate")]
    public void calibrate()
    {
        if(trackable == null)
        {
            Debug.LogWarning("Trackable null, not calibrating");
            SCController[] controllers = GetComponentsInChildren<SCController>();
            foreach (SCController sc in controllers) if (sc.specktrHand == SpecktrOSC.Hand.Right) trackable = sc.transform;
            return;
        }

        transform.position = Vector3.up * transform.position.y;
        transform.rotation = Quaternion.identity;

        transform.localRotation = Quaternion.Euler(0, -trackable.eulerAngles.y, 0);
        transform.position = new Vector3(-trackable.position.x, -trackable.localPosition.y, -trackable.position.z) + calibTransform.transform.position;
    }
    
    public void loadConfig()
    {
        string path = Application.dataPath + "/viveConfig.json";
        string s = File.ReadAllText(path);
        ViveConfigData vc = JsonUtility.FromJson<ViveConfigData>(s);
        transform.position = vc.position;
        transform.rotation = vc.rotation;
        rightHandID = vc.rightControllerID;
        
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
    }
}

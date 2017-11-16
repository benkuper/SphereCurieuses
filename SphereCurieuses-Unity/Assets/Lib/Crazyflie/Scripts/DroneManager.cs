using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneManager : OSCControllable {

    public static DroneManager instance;

    public delegate void DroneSetupEvent();
    public event DroneSetupEvent droneSetup;

    public delegate void DroneStateUpdate(Drone d);
    public event DroneStateUpdate droneStateUpdate;

   

    public List<Drone> drones;

    [Header("Setup")]
    public GameObject dronePrefab;

    [Header("Global settings")]
    public float selectionRadius;

    [Header("Testing")]
    public bool testMode;

    private void Awake()
    {
        instance = this;
        drones = new List<Drone>();
    }
    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    [OSCMethod("setup",packInArray =true)]
    public void setup(object[] data)
    {
        clean();
        for(int i=0;i<data.Length;i++)
        {
            addDrone((string)data[i]);
        }

        if (droneSetup != null) droneSetup();
    }

    public void resetAllKalman()
    {
        foreach (Drone d in drones) d.resetKalman();
    }

    public void stopAll()
    {
        foreach (Drone d in drones) d.stop();
    }

    public void launchAll()
    {
        foreach (Drone d in drones) d.launch();
    }

    public void clean()
    {

        foreach(Drone d in drones) Destroy(d.gameObject);
        drones = new List<Drone>();
    }

    public void addDrone(string droneName)
    {
        Drone d = Instantiate(dronePrefab).GetComponent<Drone>();
        d.setName(droneName);
        d.transform.SetParent(transform, true);
        if (testMode) d.transform.position = new Vector3(Random.Range(2.0f, 5.0f), 0, Random.Range(2.0f, 4.0f));
        else d.transform.position = Vector3.right * drones.Count * .2f;
        drones.Add(d);
        d.stateUpdate += stateUpdateHandler;
        d.testMode = testMode;
    }

    public void stateUpdateHandler(Drone d)
    {
        if (droneStateUpdate != null) droneStateUpdate(d);
    }
}

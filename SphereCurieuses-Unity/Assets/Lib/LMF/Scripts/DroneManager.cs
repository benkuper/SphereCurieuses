using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class DroneManager : Controllable {

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

    [Header("Simulation")]
    [OSCProperty]
    public bool simulationMode;


    [OSCProperty]
    public bool sendLights = true;

    [Header("Conversion")]
    public bool convertsFromFeet;

    public override void Awake()
    {
        usePresets = false;
        base.Awake();

        instance = this;
        drones = new List<Drone>();

    }

    private void Start()
    {
        sendSetupRequest();
    }

    [OSCMethod]
    public void sendSetupRequest()
    {
        OSCMessage m = new OSCMessage("/setup");
        LMFClient.sendMessage(m);
    }


    [OSCMethod(packInArray=true)]
    public void setup(object[] data)
    {
        clean();
        for(int i=0;i<data.Length;i++)
        {
            addDrone((string)data[i]);
        }

        for (int i = 0; i < drones.Count; i++) drones[i].updateLabel();

        if (droneSetup != null) droneSetup();
    }

    [OSCMethod]
    public void calibrateAll(bool onlyOnGround)
    {
        foreach (Drone d in drones)
        {
            if (!onlyOnGround || !d.isFlying()) d.calibrate();
        }
    }

    [OSCMethod]
    public void launchAll()
    {
        foreach(Drone d in drones) d.launch();
    }

    [OSCMethod]
    public void landAll()
    {
        foreach (Drone d in drones) d.land();
    }

    [OSCMethod]
    public void stopAll()
    {
        foreach (Drone d in drones) d.stop();
    }

    public void clean()
    {

        foreach(Drone d in drones) Destroy(d.gameObject);
        drones = new List<Drone>();
    }

    public void addDrone(string droneName)
    {
        Drone d = Instantiate(dronePrefab).GetComponent<Drone>();
        d.setId(droneName);
        d.transform.SetParent(transform, true);
        //if (testMode) d.transform.position = new Vector3(Random.Range(2.0f, 5.0f), 0, Random.Range(2.0f, 4.0f));
        //else d.transform.position = Vector3.right * drones.Count * .2f;
        drones.Add(d);
        d.stateUpdatedEvent += stateUpdateHandler;
    }

    public void stateUpdateHandler(Drone d)
    {
        if (droneStateUpdate != null) droneStateUpdate(d);
    }


    //External commands
    [OSCMethod]
    public void launchDrone(int index)
    {
        if (index == -1) launchAll();
        else if (index >= 0 && index < drones.Count) drones[index].launch();
    }

    [OSCMethod]
    public void landDrone(int index)
    {
        if (index == -1) landAll();
        else if (index >= 0 && index < drones.Count) drones[index].land();
    }

    [OSCMethod]
    public void stopDrone(int index)
    {
        if (index == -1) stopAll();
        if (index >= 0 && index < drones.Count) drones[index].stop();
    }

    [OSCMethod]
    public void setDronePosition(int index, float time, Vector3 position)
    {
        position = getConvertedPosition(position);
        if (index >= 0 && index < drones.Count) drones[index].moveToPosition(position, time);
    }

    [OSCMethod(packInArray =true)] // values : time (float), 0x, 0y, 0z, 1x, 1y, 1z, ..
    public void setAllDronesPositions(object[] values)
    {
        float time = (float)values[0];
        int numPositions = Mathf.FloorToInt((values.Length-1) / 3.0f);
        for(int i = 0;i<numPositions;i++)
        {
            int index = 1 + i * 3;
            Vector3 p = new Vector3((float)values[index], (float)values[index + 1], (float)values[index + 2]);
            if(p != Vector3.zero) setDronePosition(i, time, p);
        }
    }

    [OSCMethod(packInArray = true)] // values : drone indices (1,3,5,..) time (float), 0x, 0y, 0z, 1x, 1y, 1z, ..
    public void setSelectDronesPositions(object[] values)
    {
        string[] iSplit = ((string)values[0]).Split(new char[] { ',' });
        float time = (float)values[1];

        int numPositions = Mathf.FloorToInt((values.Length - 2) / 3.0f);

        for (int i = 0; i < numPositions && i < iSplit.Length; i++)
        {
            int droneIndex = -1;
            if(int.TryParse(iSplit[i], out droneIndex))
            {
                int vIndex = 2 + i * 3;
                Vector3 p = new Vector3((float)values[vIndex], (float)values[vIndex + 1], (float)values[vIndex + 2]);
                setDronePosition(droneIndex, time, p);
            }
            
        }
    }

    [OSCMethod]
    public void setSelectDronesPositionX(string indexes, float time, float value)
    {
        string[] iSplit =indexes.Split(new char[] { ',' });
        foreach(string si in iSplit)
        {
            int index = -1;
            if (int.TryParse(si, out index))
            {
                setDronePositionX(index, time, value);
            }
        }
    }

    [OSCMethod]
    public void setSelectDronesPositionY(string indexes, float time, float value)
    {
        string[] iSplit = indexes.Split(new char[] { ',' });
        foreach (string si in iSplit)
        {
            int index = -1;
            if (int.TryParse(si, out index))
            {
                setDronePositionY(index, time, value);
            }
        }
    }

    [OSCMethod]
    public void setSelectDronesPositionZ(string indexes, float time, float value)
    {
        string[] iSplit = indexes.Split(new char[] { ',' });
        foreach (string si in iSplit)
        {
            int index = -1;
            if (int.TryParse(si, out index))
            {
                setDronePositionZ(index, time, value);
            }
        }
    }

    [OSCMethod]
    public void setDronePositionX(int index, float time, float posX)
    {
        posX = getConvertedValue(posX);

        if (index >= 0 && index < drones.Count) drones[index].moveToPosition(new Vector3(posX, drones[index].transform.position.y, drones[index].transform.position.z), time);
        else if (index == -1)
        {
            foreach (Drone d in drones) d.moveToPosition(new Vector3(posX, d.transform.position.y, d.transform.position.z), time);
        }
    }

    [OSCMethod]
    public void setDronePositionY(int index, float time, float posY)
    {
        posY = getConvertedValue(posY);

        if (index >= 0 && index < drones.Count) drones[index].moveToPosition(new Vector3(drones[index].transform.position.x, posY, drones[index].transform.position.z), time);
        else if (index == -1)
        {
            foreach (Drone d in drones) d.moveToPosition(new Vector3(d.transform.position.x, posY, d.transform.position.z), time);
        }
    }

    [OSCMethod]
    public void setDronePositionZ(int index, float time, float posZ)
    {
        posZ = getConvertedValue(posZ);

        if (index >= 0 && index < drones.Count) drones[index].moveToPosition(new Vector3(drones[index].transform.position.x, drones[index].transform.position.y, posZ), time);
        else if (index == -1)
        {
            foreach (Drone d in drones) d.moveToPosition(new Vector3(d.transform.position.x, d.transform.position.y, posZ), time);
        }
    }

    [OSCMethod]
    public void setDroneYaw(int index, float time, float yaw)
    {
        if (index >= 0 && index < drones.Count)
        {
            drones[index].yawTo(yaw, time);
        }
    }

    [OSCMethod]
    public void setDroneColor(int index, float time, Color color)
    {
        if (index >= 0 && index < drones.Count)
        {
            drones[index].colorTo(color, time);
        }else if(index == -1)
        {
            foreach (Drone d in drones) d.colorTo(color, time);
        }
    }

    [OSCMethod]
    public void setDroneheadlight(int index, bool value)
    {
        if (index >= 0 && index < drones.Count)
        {
            drones[index].setHeadlight(value);
        }else if(index == -1)
        {
            foreach (Drone d in drones) d.setHeadlight(value);
        }
    }

    [OSCMethod]
    public void reorderX()
    {
        drones.Sort(SortByPositionX);
        for (int i = 0; i < drones.Count; i++)
        {
            drones[i].transform.SetSiblingIndex(i);
            drones[i].updateLabel();
        }

    }


    [OSCMethod]
    public void reorderZ()
    {
        drones.Sort(SortByPositionZ);
        for (int i = 0; i < drones.Count; i++)  drones[i].transform.SetSiblingIndex(i);
        for (int i = 0; i < drones.Count; i++) drones[i].updateLabel();
    }


    [OSCMethod(packInArray = true)]
    public void reassign(object[] newIds)
    {

        Drone[] newDroneList = new Drone[drones.Count];
        for(int i=0;i<newIds.Length;i++)
        {
            Debug.Log("Setting new id : " + (int)(newIds[i])+" to drone "+i);
            if ((int)newIds[i] < 0 || (int)newIds[i] >= newDroneList.Length) continue;
            newDroneList[(int)newIds[i]] = drones[i];
        }

        for(int i=newIds.Length;i<drones.Count;i++)
        {
            for (int j = 0; j < newDroneList.Length; j++)
            {
                if (newDroneList[j] == null)
                {
                    newDroneList[j] = drones[i];
                    break;
                }
            }
        }

        drones = new List<Drone>(newDroneList);

        for (int i = 0; i < drones.Count; i++) drones[i].transform.SetSiblingIndex(i);
        for (int i = 0; i < drones.Count; i++) drones[i].updateLabel();

    }

    public int SortByPositionZ(Drone d1, Drone d2)
    {
        //if (d1.transform.position.y <= 0) return 1;
        //if (d2.transform.position.y <= 0) return -1;
        return d1.transform.position.z.CompareTo(d2.transform.position.z);
    }

    public int SortByPositionX(Drone d1, Drone d2)
    {
        //if (d1.transform.position.y <= 0) return 1;
        //if (d2.transform.position.y <= 0) return -1;
        return d1.transform.position.x.CompareTo(d2.transform.position.x); //Inverse order, first in list will be the greatest X
    }

    //Util
    public float getConvertedValue(float val)
    {
        if (!convertsFromFeet) return val;
        else return val * 0.3048f;
    }
    public Vector3 getConvertedPosition(Vector3 pos)
    {
        if (!convertsFromFeet) return pos;
        else return pos * 0.3048f;
    }
}

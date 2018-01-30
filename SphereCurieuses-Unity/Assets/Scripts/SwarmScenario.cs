using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmScenario : MonoBehaviour, IDroneLocker {

    
    public string scenarioName;
    public bool isCurrent;

    public float startTime;
    public float endTime;
    protected float timeAtStart;
    protected float timeAtEnd;
    public bool isStarting { get { return isCurrent && Time.time < timeAtStart + startTime; } }
    public bool isEnding { get { return Time.time < timeAtEnd + endTime; } }

    public Dictionary<Drone, bool> droneLocks;

    public virtual void Awake()
    {
        isCurrent = false;
        droneLocks = new Dictionary<Drone, bool>();
        
    }

    public virtual void Start() {

    }
    
    public virtual void Update() {
        if (isCurrent && !isStarting && !isEnding) updateScenario();
    }

    public virtual void startScenario() { }
    public virtual void updateScenario() { }
    public virtual void endScenario() { }

    public void setCurrent(bool value)
    {
        Debug.Log("Set current " + scenarioName);
        if (isCurrent == value) return;
        isCurrent = value;

        if(isCurrent)
        {
            //start
            timeAtStart = Time.time;
            startScenario();
        }
        else
        {
            //end
            timeAtEnd = Time.time;
            endScenario();

        }
    }

    public bool lockDrone(Drone d)
    {
        if (d.isLocked()) return false;
        d.setLocker(this);
        if (!droneLocks.ContainsKey(d)) droneLocks.Add(d, true);
        else droneLocks[d] = true;
        return true;

    }

    public void releaseDrone(Drone d)
    {
        if ((Object)d.locker != this) return;

        d.setLocker(null);

        if (!droneLocks.ContainsKey(d))
        {
            Debug.Log("Should not be here");
            return;
        }
        droneLocks[d] = false;
    }

    //Interaction

    public virtual void buttonStateUpdate(DroneController controller, int buttonID, bool value, DroneController.ButtonState state) { }

    public virtual void triggerShortPress(DroneController controller, int buttonID, DroneController.ButtonState state) { }

    public virtual void triggerLongPress(DroneController controller, int buttonID, DroneController.ButtonState state) { }

    public virtual void overDroneUpdate(DroneController controller, Drone d) { }
}

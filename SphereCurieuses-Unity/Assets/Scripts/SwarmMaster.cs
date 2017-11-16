using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmMaster : MonoBehaviour {

    public static SwarmMaster instance;

    public SwarmScenario currentScenario;
    public List<SwarmScenario> scenarios;

    //Helper to know which controller is over which drone
    public Dictionary<DroneController, Drone> overDrones;

    void Awake()
    {
        overDrones = new Dictionary<DroneController, Drone>();
        instance = this;
    }

    private void droneSetupCallback()
    {
        
        setCurrentScenario(null);
        if (currentScenario == null && scenarios.Count > 0) setCurrentScenario(scenarios[0]);
        
    }

    void Start () {
        DroneManager.instance.droneSetup += droneSetupCallback;
        scenarios = new List<SwarmScenario>(GetComponents<SwarmScenario>());
        setCurrentScenario(null);
       // setCurrentScenario(scenarios[0]);
    }

    void Update () {
        
    }

    public void setCurrentScenario(SwarmScenario s)
    {
        if (currentScenario == s) return;
        if(currentScenario != null)
        {
            currentScenario.setCurrent(false);
        }

        currentScenario = s;

        if (currentScenario != null)
        {
            currentScenario.setCurrent(true);
        }else
        {
            foreach (Drone d in DroneManager.instance.drones) d.colorTo(Color.black, 0);
        }
    }



    public void nextScenario()
    {
        if (scenarios.Count == 0) return;
        if (currentScenario == null) setCurrentScenario(scenarios[0]);
        else setCurrentScenario(scenarios[(scenarios.IndexOf(currentScenario)+1) % scenarios.Count]);
    }

    public void prevScenario()
    {
        if (scenarios.Count == 0) return;
        if (currentScenario == null) setCurrentScenario(scenarios[0]);
        else setCurrentScenario(scenarios[(scenarios.IndexOf(currentScenario) + scenarios.Count - 1) % scenarios.Count]);
    }


    //Global functions

    public List<Drone> getAvailableDrones(bool includeFlying, bool includeOnGround) {
        return getAvailableDrones(includeFlying, includeOnGround, DroneManager.instance.drones.Count);
    }
    public List<Drone> getAvailableDrones(bool includeFlying, bool includeOnGround, int maxNum)
    {
        List<Drone> result = new List<Drone>();
        for(int i=0; i< DroneManager.instance.drones.Count && result.Count < maxNum; i++)
        {
            Drone d = DroneManager.instance.drones[i];
            if(d.canFly(true))
            {
                if (d.isFlying()) { if (includeFlying) result.Add(d); }
                else if (includeOnGround) result.Add(d);

            }
        }
        return result;
    }

    public void stopAllDrones()
    {
        foreach (Drone d in DroneManager.instance.drones) d.stop();
    }

    public void homeAllDrones()
    {
        foreach (Drone d in DroneManager.instance.drones) d.goHome();
    }


    //Interaction

    public void setOverDrone(DroneController dc, Drone d)
    {
        if (overDrones.ContainsKey(dc))
        {
            if (overDrones[dc] == d) return;
            overDrones[dc].isOver = false;
            if (currentScenario != null) currentScenario.overDroneUpdate(dc, overDrones[dc]);
            overDrones.Remove(dc);
        }

        if (d != null)
        {
            //Debug.Log("Set over drone " + dc.id + " > " + d);

            d.isOver = true;
            overDrones.Add(dc, d);
            if (currentScenario != null) currentScenario.overDroneUpdate(dc, d);
        }        
    }


    public void buttonStateUpdate(DroneController controller, int buttonID, bool value)
    {
        if (currentScenario != null) currentScenario.buttonStateUpdate(controller, buttonID, value);

    }

    public void triggerShortPress(DroneController controller, int buttonID)
    {
        if (currentScenario != null) currentScenario.triggerShortPress(controller, buttonID);
    }

    public void triggerLongPress(DroneController controller, int buttonID)
    {
        if (currentScenario != null) currentScenario.triggerLongPress(controller, buttonID);
    }

}

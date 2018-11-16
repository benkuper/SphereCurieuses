using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmMaster : Controllable {

    public static SwarmMaster instance;

    public SwarmScenario currentScenario;
    public List<SwarmScenario> scenarios;

    public Dictionary<DroneController, Drone> overDrones;

    const int SHAPE_BT = 6;
    const int TRAIL_BT = 7;
    const int BOX_BT = 5;

    public enum HitMode { COLLIDER, SHORTEST_DISTANCE }
    public HitMode hitMode;
    public float maxSelectionDistance;

    override public void Awake()
    {
        overDrones = new Dictionary<DroneController, Drone>();
        instance = this;
        TargetScript = this;
        base.Awake();
    }

    private void droneSetupCallback()
    {
        
        setCurrentScenario(null);
        if (currentScenario == null && scenarios.Count > 0) setCurrentScenario(scenarios[0]);
        
    }

    void Start () {
        DroneManager.instance.droneSetup += droneSetupCallback;
        scenarios = new List<SwarmScenario>(GetComponents<SwarmScenario>());
        //setCurrentScenario(null);
        setCurrentScenario(scenarios[0]);
    }

   
    public void setScenario(int index)
    {
        if (index < 0 || index >= scenarios.Count) return;
        setCurrentScenario(scenarios[index]);
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
            DroneController[] controllers = FindObjectsOfType<DroneController>();
            foreach(DroneController dc in controllers) MrTrackerClient.instance.sendMultiVibrate(dc.id, 5, .1f, .4f, 0.05f);
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

    [OSCMethod]
    public void stopScenario()
    {
        setCurrentScenario(null);
    }

    [OSCMethod]
    public void setTrailScenario()
    {
        foreach (SwarmScenario s in scenarios)
        {
            if (s is SwarmTrailScenario)
            {
                setCurrentScenario(s);
                return;
            }
        }
    }

    [OSCMethod]
    public void setBoxScenario()
    {
        foreach (SwarmScenario s in scenarios)
        {
            if (s is SwarmEndBoxScenario)
            {
                setCurrentScenario(s);
                return;
            }
        }
    }

    //Global functions

    public List<Drone> getZOrderedAvailableDrones(bool includeFlying, bool includeOnGround)
    {
        List<Drone> dList = getAvailableDrones(includeFlying, includeOnGround, DroneManager.instance.drones.Count);
        dList.Sort(SortByPositionZ);
        return dList;
    }

    public List<Drone> getXOrderedAvailableDrones(bool includeFlying, bool includeOnGround)
    {
        List<Drone> dList = getAvailableDrones(includeFlying, includeOnGround, DroneManager.instance.drones.Count);
        dList.Sort(SortByPositionX);
        return dList;
    }

    public int SortByPositionZ(Drone d1, Drone d2)
    {
        return d1.realPosition.z.CompareTo(d2.realPosition.z);
    }

    public int SortByPositionX(Drone d1, Drone d2)
    {
        return d2.realPosition.x.CompareTo(d1.realPosition.x); //Inverse order, first in list will be the greatest X
    }

    public List<Drone> getAvailableDrones(bool includeFlying, bool includeOnGround) {
        return getAvailableDrones(includeFlying, includeOnGround, DroneManager.instance.drones.Count);
    }

    public List<Drone> getAvailableDrones(bool includeFlying, bool includeOnGround, int maxNum)
    {
        List<Drone> result = new List<Drone>();
        for(int i=0; i< DroneManager.instance.drones.Count && result.Count < maxNum; i++)
        {
            Drone d = DroneManager.instance.drones[i];

            if (d.lowBattery) continue;

            //Debug.Log(" Check drone : " + d.droneName + " " + d.isReady() + " / " + d.isFlying());

            bool good = false;
            if(includeOnGround) good |= d.isReady();
            if (includeFlying) good |= d.isFlying();
            if(good) result.Add(d);
            
        }
        return result;
    }

    public void stopAllDrones()
    {
        foreach (Drone d in DroneManager.instance.drones) d.stop();
    }

    public void landAllDrones()
    {
        foreach (Drone d in DroneManager.instance.drones) d.land();
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


    public void buttonStateUpdate(DroneController controller, int buttonID, bool value, DroneController.ButtonState state)
    {
        if (currentScenario != null) currentScenario.buttonStateUpdate(controller, buttonID, value,state);

    }

    public void triggerShortPress(DroneController controller, int buttonID, DroneController.ButtonState state)
    {
        switch (buttonID)
        {
           
            case TRAIL_BT:
                setCurrentScenario(scenarios[0]);
                break;

            default:
                if (currentScenario != null) currentScenario.triggerShortPress(controller, buttonID, state);
                break;

        }

        
    }

    public void triggerLongPress(DroneController controller, int buttonID,DroneController.ButtonState state)
    {
        
        if(buttonID == BOX_BT && state == DroneController.ButtonState.Down)
        {
             setCurrentScenario(scenarios[scenarios.Count-1]);
            return;
        }
               
        if(buttonID == 3 && state == DroneController.ButtonState.Down)
        {
            DroneManager.instance.calibrateAll(true);
            return;
        }

        switch(buttonID)
        {
             case SHAPE_BT:
                if (state != DroneController.ButtonState.Off && currentScenario != scenarios[scenarios.Count-1]) //Only if not the last scenario
                {
                    int offset = 1 + (int)state;
                    setCurrentScenario(scenarios[offset]);
                }
                break;

            default:
                if (currentScenario != null) currentScenario.triggerLongPress(controller, buttonID, state);
                break;

        }

        
    }

}

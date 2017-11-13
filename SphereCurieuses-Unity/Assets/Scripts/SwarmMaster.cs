using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SwarmMaster : MonoBehaviour {

    public static SwarmMaster instance;

    public SwarmScenario currentScenario;
    public List<SwarmScenario> scenarios;

    void Awake()
    {
        instance = this;
    }

	void Start () {
        if(GetComponents<SwarmScenario>().Length != scenarios.Count) scenarios = new List<SwarmScenario>(GetComponents<SwarmScenario>());
        if (currentScenario == null && scenarios.Count > 0) setCurrentScenario(scenarios[0]);
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
    public void launchAllDrones()
    {

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

    }

    public void homeAllDrones()
    {
        foreach (Drone d in DroneManager.instance.drones) d.stop();
    }
}

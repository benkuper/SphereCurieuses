﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class SwarmEndBoxScenario : SwarmScenario
{
    public Transform dropTransform;
    public float stagger;

    [OSCProperty]
    public float dropX;
    [OSCProperty]
    public float dropY;
    [OSCProperty]
    public float dropZ;


    public override void Start()
    {
        base.Start();

        loadPositions();
    }

    [OSCMethod]
    public void savePositions()
    {
        SaveData data = new SaveData();
        data["x"] = dropX;
        data["y"] = dropY;
        data["z"] = dropZ;
        //Save the data
        data.Save(Application.dataPath + "/endBoxPositions.uml");

    }

    [OSCMethod]
    public void loadPositions()
    {

        SaveData data = SaveData.Load(Application.dataPath + "/endBoxPositions.uml");
        if (data == null) return;
        dropX = data.GetValue<float>("x");
        dropY = data.GetValue<float>("y");
        dropZ = data.GetValue<float>("z");

    }

    public override void startScenario()
    {
        base.startScenario();
        
        List<Drone> drones = SwarmMaster.instance.getZOrderedAvailableDrones(true, false);
        for(int i=0;i<drones.Count;i++)
        {
            StartCoroutine(dropDrone(drones[i], (drones.Count-i-1)*stagger));
        }

        
    }

    public override void Update()
    {
        base.Update();
        dropTransform.position = new Vector3(dropX, dropY, dropZ);
    }

    override public void updateScenario()
    {

    }

    public IEnumerator dropDrone(Drone d, float delay)
    {
        yield return new WaitForSeconds(delay);
        float moveTime = stagger;
        d.moveToPosition(dropTransform.position, moveTime);
        yield return new WaitForSeconds(moveTime+2);
        //Vector3 drop = new Vector3(dropTransform.position.x,0, dropTransform.position.z);
        d.stop();
    }
}

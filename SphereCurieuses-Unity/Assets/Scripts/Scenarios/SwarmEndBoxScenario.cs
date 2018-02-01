using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class SwarmEndBoxScenario : SwarmScenario
{
    public Transform dropTransform;
    public float stagger;

    public override void startScenario()
    {
        List<Drone> drones = SwarmMaster.instance.getXOrderedAvailableDrones(true, false);
        for(int i=0;i<drones.Count;i++)
        {
            StartCoroutine(dropDrone(drones[i], i*stagger));
        }
    }

    override public void updateScenario()
    {

    }

    public IEnumerator dropDrone(Drone d, float delay)
    {
        yield return new WaitForSeconds(delay);
        float moveTime = stagger;
        d.moveToPosition(dropTransform.position, moveTime);
        yield return new WaitForSeconds(moveTime);
        Vector3 drop = new Vector3(dropTransform.position.x,0, dropTransform.position.z);
        d.moveToPosition(drop, 0);
    }
}

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SwarmShapeScenario : SwarmScenario {

    public Transform target;
    public bool showGizmo;
    public Vector3[] positions;
    public List<Vector3> zOrderedPositions;

    public List<Drone> drones;


    override public void Start()
    {
        base.Start();

       
    }

    public override void startScenario()
    {
        orderPositions();

        drones = SwarmMaster.instance.getZOrderedAvailableDrones(true, false);
        if(drones.Count < positions.Length) drones = SwarmMaster.instance.getZOrderedAvailableDrones(true, true);

        //FLy off all unnecessary drones
        while (drones.Count > zOrderedPositions.Count)
        {
            Drone d = drones[drones.Count - 1];
            d.stop();
            drones.Remove(d);
        }

        updateDronesPositions(2);
        foreach(Drone d in drones) lockDrone(d);
    }

    public void orderPositions()
    {
        zOrderedPositions = new List<Vector3>();
        foreach (Vector3 p in positions) zOrderedPositions.Add(p);
        zOrderedPositions.Sort(sortPositionByZ);

    }

    public int sortPositionByZ(Vector3 p1, Vector3 p2)
    {
        return getPosition(p1).z.CompareTo(getPosition(p2).z);
    }

    override public void updateScenario()
    {
        updateDronesPositions(0);
    }

    public override void endScenario()
    {
        foreach (Drone d in drones) releaseDrone(d);
    }

    public void updateDronesPositions(float time)
    {
        for (int i = 0; i < drones.Count; i++)
        {
            drones[i].moveToPosition(getPosition(i),time);
        }
    }

    public Vector3 getPosition(int index)
    {
        if (index < 0 || index > positions.Length) return Vector3.zero;
        if (target == null) return Vector3.zero;
        return getPosition(zOrderedPositions[index]);
    }

    public Vector3 getPosition(Vector3 relativePos)
    {
        return target.TransformPoint(relativePos);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!isCurrent && !showGizmo) return;
        Gizmos.color = Selection.activeGameObject == gameObject ? Color.yellow : Color.grey;

        Gizmos.DrawWireSphere(target.position, .2f);

        if(!Application.isPlaying)  orderPositions();

        for (int i = 0; i < zOrderedPositions.Count; i++)
        {
            Vector3 p = getPosition(i);
            Gizmos.DrawLine(target.position, p);
            Handles.Label(p+Vector3.down*.4f, i.ToString());
            Gizmos.DrawWireCube(p, Vector3.one * .2f);
        }
        
    }
#endif
}


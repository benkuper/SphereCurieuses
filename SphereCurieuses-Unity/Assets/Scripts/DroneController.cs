using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneController : MonoBehaviour
{

    public enum ButtonState { Up, Side, Down, Off }

    public int id;
    public int numButtons;

    public bool[] buttonStates;
    public Vector3 aimDirection;

    float[] buttonTouchTimes;
    const float longPressTime = .7f;
    Coroutine[] longPressChecks;

    public bool debugSelection;
    Vector3 debugProj;

    [Range(-1, 1)]
    public float pitch;

    void Start()
    {
        buttonStates = new bool[numButtons];
        buttonTouchTimes = new float[numButtons];
        longPressChecks = new Coroutine[numButtons];
    }

    void Update()
    {
        pitch = (transform.TransformPoint(Vector3.up) - transform.position).y;
        checkForOverDrones();
    }

    void checkForOverDrones()
    {

        Color c = Color.grey;
        bool found = false;

        Ray r = new Ray(transform.position, transform.TransformDirection(aimDirection));

        if (SwarmMaster.instance.hitMode == SwarmMaster.HitMode.COLLIDER)
        {
            RaycastHit hit;

            if (Physics.Raycast(r, out hit, 100f))
            {
                Drone d = hit.collider.GetComponent<Drone>();
                //Debug.Log("Hit ! " + (d != null ? d.droneName : hit.collider.gameObject.name));
                if (d != null)
                {
                    SwarmMaster.instance.setOverDrone(this, d);
                    found = true;

                    c = Color.green;
                }
            }
        }
        else
        {
            List<Drone> drones = SwarmMaster.instance.getAvailableDrones(true, true);
            float minDist = 10;
            Drone minDrone = null;

            Vector3 proj = Vector3.zero;
            //if (GetComponent<SCController>().specktrHand == SpecktrOSC.Hand.Right) Debug.Log("Search for closest drone");
            foreach (Drone d in drones)
            {
                bool pointIsBackwards;
                proj = NearestPointOnLine(r.origin, r.direction, d.transform.position, out pointIsBackwards);// Vector3.Cross(r.direction, d.transform.position - r.origin);

                if (pointIsBackwards) continue;

                float dist = Vector3.Distance(d.transform.position, proj);

                if (debugSelection) Debug.DrawLine(d.transform.position, proj, Color.grey);

                //if (GetComponent<SCController>().specktrHand == SpecktrOSC.Hand.Right) Debug.Log(d.droneName + " > " + dist);


                if (dist < minDist)
                {
                    minDist = dist;
                    minDrone = d;
                }
            }

            if (minDist < SwarmMaster.instance.maxSelectionDistance)
            {
                //if (GetComponent<SCController>().specktrHand == SpecktrOSC.Hand.Right) Debug.Log("found : " + minDrone);
                SwarmMaster.instance.setOverDrone(this, minDrone);
                if (debugSelection) debugProj = proj;
                found = true;
            }
        }


        /// Debug.DrawRay(r.origin, r.direction * 10, c);
        if (SwarmMaster.instance == null) return;
        if (!found)
        {
            SwarmMaster.instance.setOverDrone(this, null);
            debugProj = Vector3.zero;
        }
    }

    public void setButtonState(int buttonID, bool value, ButtonState buttonState)
    {

        if (buttonStates[buttonID] == value) return;
        buttonStates[buttonID] = value;

        //Debug.Log("Set button state " + buttonID + " : " + value);
        if (value)
        {
            buttonTouchTimes[buttonID] = Time.time;
            longPressChecks[buttonID] = StartCoroutine(checkLongPress(buttonID, buttonState));
        }
        else
        {
            if (longPressChecks[buttonID] != null)
            {
                StopCoroutine(longPressChecks[buttonID]);
                longPressChecks[buttonID] = null;
            }

            if (Time.time - buttonTouchTimes[buttonID] < longPressTime) triggerShortPress(buttonID, buttonState);
        }

        SwarmMaster.instance.buttonStateUpdate(this, buttonID, value, buttonState);
    }

    IEnumerator checkLongPress(int buttonID, ButtonState state)
    {
        yield return new WaitForSeconds(longPressTime);
        triggerLongPress(buttonID, state);
    }

    void triggerShortPress(int buttonID, ButtonState state)
    {
        SwarmMaster.instance.triggerShortPress(this, buttonID, state);
    }

    void triggerLongPress(int buttonID, ButtonState state)
    {
        SwarmMaster.instance.triggerLongPress(this, buttonID, state);
    }

    public Vector3 NearestPointOnLine(Vector3 origin, Vector3 lineDir, Vector3 pointToCheck, out bool pointIsBackwards)
    {
        lineDir.Normalize();//this needs to be a unit vector
        var v = pointToCheck - origin;
        var d = Vector3.Dot(v, lineDir);
        pointIsBackwards = d < 0;
        return origin + lineDir * d;
    }

    public void OnDrawGizmos()
    {
        if (!debugSelection) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.TransformPoint(aimDirection * 8));
        Gizmos.DrawWireSphere(debugProj, .2f);
    }
}

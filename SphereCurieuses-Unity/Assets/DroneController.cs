using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneController : MonoBehaviour
{
    public int id;

    public int numButtons;

    public bool[] buttonStates;
    public Vector3 aimDirection;

    float[] buttonTouchTimes;
    const float longPressTime = .7f;
    Coroutine[] longPressChecks;

    void Start()
    {
        buttonStates = new bool[numButtons];
        buttonTouchTimes = new float[numButtons];
        longPressChecks = new Coroutine[numButtons];
    }

    void Update()
    {
        checkForOverDrones();
    }

    void checkForOverDrones()
    {
        Color c = Color.grey;

        Ray r = new Ray(transform.position, transform.TransformDirection(aimDirection));
        RaycastHit hit;

        bool found = false;
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

        Debug.DrawRay(r.origin, r.direction * 10, c);
        if (!found) SwarmMaster.instance.setOverDrone(this, null);
    }

    public void setButtonState(int buttonID, bool value)
    {
        if (buttonStates[buttonID] == value) return;
        buttonStates[buttonID] = value;

        if(value)
        {
            buttonTouchTimes[buttonID] = Time.time;
            longPressChecks[buttonID] = StartCoroutine(checkLongPress(buttonID));             
        }
        else
        {
            if(longPressChecks[buttonID] != null)
            {
                StopCoroutine(longPressChecks[buttonID]);
                longPressChecks[buttonID] = null;
            }

            if (Time.time - buttonTouchTimes[buttonID] < longPressTime) triggerShortPress(buttonID);
        }

        SwarmMaster.instance.buttonStateUpdate(this, buttonID, value);
    }

    IEnumerator checkLongPress(int buttonID)
    {
        yield return new WaitForSeconds(longPressTime);
        triggerLongPress(buttonID);
    }

    void triggerShortPress(int buttonID)
    {
        SwarmMaster.instance.triggerShortPress(this, buttonID);
    }

    void triggerLongPress(int buttonID)
    {
        SwarmMaster.instance.triggerLongPress(this, buttonID);
    }
}

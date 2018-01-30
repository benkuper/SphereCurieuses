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

       /// Debug.DrawRay(r.origin, r.direction * 10, c);
        if (SwarmMaster.instance == null) return;
        if (!found) SwarmMaster.instance.setOverDrone(this, null);
    }

    public void setButtonState(int buttonID, bool value, ButtonState buttonState)
    {

        if (buttonStates[buttonID] == value) return;
        buttonStates[buttonID] = value;

        //Debug.Log("Set button state " + buttonID + " : " + value);
        if(value)
        {
            buttonTouchTimes[buttonID] = Time.time;
            longPressChecks[buttonID] = StartCoroutine(checkLongPress(buttonID, buttonState));             
        }
        else
        {
            if(longPressChecks[buttonID] != null)
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
        triggerLongPress(buttonID,state);
    }

    void triggerShortPress(int buttonID, ButtonState state)
    {
        SwarmMaster.instance.triggerShortPress(this, buttonID, state);
    }

    void triggerLongPress(int buttonID, ButtonState state)
    {
        SwarmMaster.instance.triggerLongPress(this, buttonID, state);
    }
}

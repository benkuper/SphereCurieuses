using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityOSC;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Drone : OSCControllable {

    public string droneName;

    public enum DroneState { Disconnected, Connecting, Stabilizing, Ready, Error };
    public enum LightMode { Off, WhiteSpinner, ColorSpinner, TiltEffect, Brightness, ColorSpinner2, DoubleSpinner, SolidColor, FactoryTest, BatteryStatus, BoatLights, Alert, Gravity };


    [OSCProperty("state")]
    public DroneState state;
    [OSCProperty("realPosition")]
    public Vector3 realPosition;

    [Header("Power")]
    [OSCProperty("voltage")]
    public float voltage;
    [OSCProperty("isCharging")]
    public bool isCharging;
    [OSCProperty("lowBattery")]
    public bool lowBattery;

    [Header("Lighting")]
    [OSCProperty("lightMode")]
    public LightMode lightMode;
    [OSCProperty("lightColor")]
    public Color color;

    [Header("Testing")]
    public bool testMode;



    //home position
    Vector3 homePosition;

    //Check last values
    DroneState lastState;
    Vector3 lastPosition;
    bool lastLowBattery;



    void Start () {
        lastState = DroneState.Disconnected;
        
	}
	
	void Update () {

        if(lastState != state)
        {
            if (state == DroneState.Ready && (lastState == DroneState.Stabilizing ||lastState == DroneState.Disconnected))
            {
                lastState = state;
                transform.position = new Vector3(realPosition.x, 0, realPosition.z);
                homePosition = transform.position;
            }
            lastState = state;
        }

        if (state == DroneState.Ready)
        {
            if (lastPosition != transform.position)
            {
                sendTargetPosition();
                lastPosition = transform.position;
            }
        }
        else if(state == DroneState.Stabilizing)
        {
            transform.position = new Vector3(realPosition.x, 0, realPosition.z);
        }

        if(lowBattery != lastLowBattery)
        {
            lastLowBattery = lowBattery;
            if (lowBattery) goHome();
        }
	}

    public void setName(string n)
    {
        oscName = "drone-" + n;
        droneName = n;
        gameObject.name = "Drone " + n;

        MoucheManager.sendMessage(getDroneMessage("setup"));
    }

   
    public void OnSceneGUI()
    {
        
    }

    public void OnDisable()
    {
        stop();
    }

    public void OnApplicationQuit()
    {
        stop();
    }



    public void launch()
    {
        moveToPosition(realPosition + Vector3.up,2);
    }

    public void goHome()
    {
        moveToPosition(homePosition, 3, true);
    }

    public void stop()
    {
        moveToPosition(new Vector3(realPosition.x, 0, realPosition.z), 3,true);
    }

    
    public void moveToPosition(Vector3 pos, float time = 0)
    {
        moveToPosition(pos, time, false);
    }

    void moveToPosition(Vector3 pos, float time, bool ignoreLowBattery)
    {
        if (!testMode && (lowBattery && !ignoreLowBattery)) return;
        transform.DOKill();
        transform.DOMove(pos, time).SetEase(Ease.InOutSine);
    }



    public void resetKalman()
    {
        OSCMessage m = getDroneMessage("resetKalmanEstimation");
        MoucheManager.sendMessage(m);
    }
    
    public void sendTargetPosition()
    {
        if (!canFly(false)) return;
        OSCMessage m = getDroneMessage("target");
        m.Append(transform.position.x);
        m.Append(transform.position.y);
        m.Append(transform.position.z);
        MoucheManager.sendMessage(m);
    }

    public bool canFly(bool includeLowBatteryInCheck)
    {
        if (isCharging) return false;
        if (!testMode && state != DroneState.Ready) return false;
        if (includeLowBatteryInCheck && lowBattery) return false;
        return true;
    }

    public bool isFlying()
    {
        return transform.position.y > 0;
    }


    OSCMessage getDroneMessage(string addressRest)
    {
        return new OSCMessage("/" + oscName + "/"+addressRest);
    }

    Color getColorForCurrentState()
    {
        switch (state)
        {
            case DroneState.Connecting: return Color.cyan;
            case DroneState.Stabilizing:  return new Color(1,.3f,1);
            case DroneState.Ready:   return Color.green;
            case DroneState.Error:    return Color.red;
            case DroneState.Disconnected:   return Color.grey;
        }

        return Color.black;
       
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Color c = getColorForCurrentState();

        GUIStyle style = new GUIStyle();
        style.normal.textColor = c;
        style.alignment = TextAnchor.MiddleCenter;
        
        Vector3 position = transform.position + Vector3.up * .08f;

        
        Handles.Label(position, droneName, style);

        Gizmos.color = c;
        Gizmos.DrawWireSphere(transform.position,DroneManager.instance.selectionRadius);

        Gizmos.color = new Color(1, 1, 1, .4f);
        Handles.DrawLine(transform.position, realPosition);
        Gizmos.DrawWireSphere(realPosition, .1f);
    }

#endif



}

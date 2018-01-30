using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityOSC;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IDroneLocker
{
    void releaseDrone(Drone d);
}

public class Drone : OSCControllable {

    public string droneName;

    public enum DroneState { Disconnected, Connecting, Stabilizing, Ready, Error };
    public enum LightMode { Off, WhiteSpinner, ColorSpinner, TiltEffect, Brightness, ColorSpinner2, DoubleSpinner, SolidColor, FactoryTest, BatteryStatus, BoatLights, Alert, Gravity };

    public delegate void DroneStateUpdate(Drone d);
    public event DroneStateUpdate stateUpdate;

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
    //[OSCProperty("headlight")]
    public bool headLight;
   // [OSCProperty("lightMode")]
    public LightMode lightMode;
    //[OSCProperty("lightColor")]
    public Color color;
   

    [Header("Testing")]
    public bool testMode;

    [Header("Selection")]
    public float colliderRealPosLerp;

    //home position
    Vector3 homePosition;

    //Check last values
    DroneState lastState;
    Vector3 lastPosition;
    bool lastLowBattery;

    bool lastHeadlight;
    LightMode lastLightMode;
    Color lastColor;

    public float yaw;
    float lastYaw;

    public IDroneLocker locker;

    public bool isOver;

    //for transition between transition to ground lowBattery lock 
    bool stoppingLowBattery;

    void Start () {
        locker = null;
        lastState = DroneState.Disconnected;

        lastHeadlight = headLight;
        lastLightMode = lightMode;
        lastColor = color;
	}

    void Update () {

        if(lastState != state)
        {
            if (state == DroneState.Ready && (lastState == DroneState.Stabilizing ||lastState == DroneState.Disconnected))
            {
                color = Color.black;
                
                lastState = state;
                resyncPosition();
                
            }

            updateColliderEnable();

            lastState = state;
            if (stateUpdate != null) stateUpdate(this);
        }

        if (state == DroneState.Ready)
        {
            if (lastPosition != transform.position)
            {
                sendTargetPosition();
                lastPosition = transform.position;
            }

            if (transform.position.y <= 0) resyncPosition();
        }
        else if(state == DroneState.Stabilizing || state == DroneState.Connecting)
        {
            resyncPosition();
        }


        if(state == DroneState.Ready)
        {
            if (lowBattery != lastLowBattery)
            {
                lastLowBattery = lowBattery;
                if (lowBattery)
                {
                    stoppingLowBattery = true;

                    stop();
                }

                updateColliderEnable();
            }

            if (lowBattery)
            {
                headLight = Time.time % 1.0f > .5f; //blink
            }

            if (headLight != lastHeadlight)
            {
                lastHeadlight = headLight;
                OSCMessage m = getDroneMessage("headlight");
                m.Append(headLight ? 1 : 0);
                MoucheManager.sendMessage(m);
            }

            if (lightMode != lastLightMode)
            {
                lastLightMode = lightMode;
                OSCMessage m = getDroneMessage("lightMode");
                m.Append((int)lightMode);
                MoucheManager.sendMessage(m);
            }

            if (color != lastColor)
            {
                lastColor = color;
                OSCMessage m = getDroneMessage("lightColor");
                m.Append(color.r);
                m.Append(color.g);
                m.Append(color.b);
                MoucheManager.sendMessage(m);
            }

            //Yaw handling
            Vector3 lookAtDrone = DroneLookAt.instance.transform.position;
            transform.LookAt(new Vector3(lookAtDrone.x, transform.position.y, lookAtDrone.z)); //Adjust y to not have pitch/roll changes
            transform.Rotate(0, -90, 0);

            yaw = (-(transform.rotation.eulerAngles.y % 360) + 360) % 360;
            if (stoppingLowBattery) yaw = 0;

            if (lastYaw != yaw)
            {
                lastYaw = yaw;
                OSCMessage m = getDroneMessage("yaw");
                m.Append(yaw);
                MoucheManager.sendMessage(m);
            }
        }
        
        GetComponent<SphereCollider>().radius = DroneManager.instance.selectionRadius;
        if (!testMode) GetComponent<SphereCollider>().center = Vector3.Lerp(Vector3.zero, transform.InverseTransformPoint(realPosition), colliderRealPosLerp);
	}

    public void updateColliderEnable()
    {
        GetComponent<Collider>().enabled = (state == DroneState.Ready && !lowBattery);
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
        if (isLocked()) locker.releaseDrone(this);
        if (transform.position.y <= 0) return;
        List<KeyValuePair<Vector3, float>> pList = new List<KeyValuePair<Vector3, float>>();
        pList.Add(new KeyValuePair<Vector3, float>(homePosition + Vector3.up * .3f,3));
        pList.Add(new KeyValuePair<Vector3, float>(homePosition,2));
        moveToPositions(pList, true);
    }

    public void stop()
    {
        if (isLocked()) locker.releaseDrone(this);
        if (transform.position.y <= 0) return;
        yaw = 0;
        stoppingLowBattery = true;
        List<KeyValuePair<Vector3, float>> pList = new List<KeyValuePair<Vector3, float>>();
        pList.Add(new KeyValuePair<Vector3, float>(new Vector3(realPosition.x, .3f, realPosition.z), 3));
        pList.Add(new KeyValuePair<Vector3, float>(new Vector3(realPosition.x, 0, realPosition.z), 2));
        moveToPositions(pList, true);
        Invoke("releaseStoppingLock", 5);
    }
    
    public void releaseStoppingLock()
    {
        stoppingLowBattery = false;
    }

    public void resyncPosition()
    {
        transform.position = new Vector3(realPosition.x, 0, realPosition.z);
        homePosition = transform.position;
    }

    public void colorTo(Color target,float time)
    {
        if (target == color) return;
        DOTween.Kill(droneName+"-color",false);
        if (time == 0) color = target;
        else DOTween.To(() => color, x => color = x, target, time).SetId(droneName+"-color");
    }

    public void moveToPosition(Vector3 pos, float time = 0)
    {
        moveToPosition(pos, time, false);
    }

    public void moveToPositions(List<KeyValuePair<Vector3,float>> posAndTimes)
    {
        moveToPositions(posAndTimes, false);
    }

    void moveToPosition(Vector3 pos, float time, bool ignoreLowBattery)
    {
        if (!canFly(!ignoreLowBattery)) return;
        transform.DOKill();
        transform.DOMove(pos, time).SetEase(Ease.InOutSine);
    }

    void moveToPositions(List<KeyValuePair<Vector3, float>> posAndTimes, bool ignoreLowBattery)
    {
        if (!canFly(!ignoreLowBattery)) return;
        transform.DOKill();
        float curDelay = 0;
        for(int i=0;i<posAndTimes.Count;i++)
        {
            transform.DOMove(posAndTimes[i].Key, posAndTimes[i].Value).SetEase(Ease.InOutSine).SetDelay(curDelay);
            curDelay += posAndTimes[i].Value;
        }
    }
    

    public void resetKalman()
    {
        OSCMessage m = getDroneMessage("resetKalmanEstimation");
        MoucheManager.sendMessage(m);
    }
    
    public void sendTargetPosition()
    {
        if (!canFly(false)) return;
        if (lowBattery && !stoppingLowBattery) return;

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
        if (transform == null) return false;
        return transform.position.y > 0;
    }

    //Locking
    public bool setLocker(IDroneLocker _locker)
    {
        if (locker == _locker) return true;
        if (isLocked()) return false;
        locker = _locker;
        return true;
    }


    public bool isLocked() {
        return locker != null;
    }

    //Helpers

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
        float radius = DroneManager.instance.selectionRadius;
        Color c = getColorForCurrentState();

        GUIStyle style = new GUIStyle();
        style.normal.textColor = headLight?Color.white:c;
        style.alignment = TextAnchor.MiddleCenter;
        
        Vector3 position = transform.position + Vector3.up * (radius + .2f);

        
        Handles.Label(position, droneName, style);

        Gizmos.color = c;
        Gizmos.DrawWireSphere(transform.position, radius);
        
        if(!DroneManager.instance.testMode)
        {
            Gizmos.color = new Color(1, 1, 1, .4f);
            Handles.DrawLine(transform.position, realPosition);
            Gizmos.DrawWireSphere(realPosition, .1f);
        }

        Gizmos.color = color;
        Gizmos.DrawCube(transform.position+Vector3.down* radius, new Vector3(radius,.1f, radius));

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(transform.position, transform.TransformPoint(Vector3.right));
    }

#endif



}

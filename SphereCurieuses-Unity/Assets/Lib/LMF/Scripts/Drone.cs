using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityOSC;


//Locking
public interface IDroneLocker
{
    void releaseDrone(Drone d);
}

public class Drone : Controllable {

    public enum DroneState { POWERED_OFF, DISCONNECTED, CONNECTING, CALIBRATING, ANALYSIS, TAKING_OFF, FLYING, LANDING, WARNING, READY, ERROR };

    public delegate void DroneStateUpdate(Drone d);
    public event DroneStateUpdate stateUpdatedEvent;

    [OSCProperty(customName = "enabled",ShowInUI = false)]
    public bool itemIsEnabled { get { return _itemIsEnabled; } set { _itemIsEnabled = value; container.SetActive(itemIsEnabled); } }
    bool _itemIsEnabled = true;

    [OSCProperty]
    public Vector3 desiredPosition { get { return _desiredPosition; } set { _desiredPosition = value; transform.position = value; } }
    Vector3 _desiredPosition;
        
    [OSCProperty]
    public Vector3 targetPosition { get { return _targetPosition; } set { _targetPosition = value; /*targetPosTransform.position = value; targetPosLine.SetPosition(1, value);*/ } } //The position computed by LMF's physics motion engine
    Vector3 _targetPosition;

    [OSCProperty]
    public Vector3 realPosition { get { return _realPosition; } set { _realPosition = value; realPosTransform.position = value; realPosLine.SetPositions(new Vector3[] { Vector3.zero, transform.InverseTransformPoint(value) }); } }    //The position detected by the tracking system
    Vector3 _realPosition;

    [OSCProperty]
    public Color lightColor { get { return _lightColor; } set { _lightColor = value; colorMat.DOColor(value, fadeTime).OnUpdate(updateTrailGradient); } }
    Color _lightColor;

    [OSCProperty]
    public float fadeTime;

    [OSCProperty]
    public bool headlight { get { return _headlight; } set { _headlight = value; headlightObject.SetActive(value); } } 
    bool _headlight;

    [OSCProperty]
    public bool batteryLevel { get { return _battery; } set { _battery = value; } }
    public bool _battery;

    [OSCProperty]
    public bool lowBattery { get { return _lowBattery; } set { _lowBattery = value;  } }
    public bool _lowBattery;

    [OSCProperty]
    public Vector3 orientation { get { return _orientation; } set { _orientation = value; transform.rotation = Quaternion.Euler(-_orientation.z,transform.rotation.y,orientation.x); } }
    public Vector3 _orientation;

    public float targetYaw;
    float oldYaw;

    [OSCProperty]
    public int droneState
    {
        get { return (int)_droneState;  }
        set
        {
            if ((int)_droneState == value) return;
            _droneState = (DroneState)value;
            stateUpdated();
        }
    }
    DroneState _droneState;


    //Private
    public String droneName; //for label

    Material stateMat;
    Material colorMat;

    GameObject container;
    Transform realPosTransform;
    LineRenderer realPosLine;
    Transform targetPosTransform;
    LineRenderer targetPosLine;

    GameObject headlightObject;
    Gradient trailGradient;
    TrailRenderer trailRenderer;

    //SpheresCurieuses specific
    
    public IDroneLocker locker;
    public bool isOver;

    public override void Awake()
    {
        usePanel = false;
        TargetScript = this;

        container = transform.Find("object").gameObject;

        headlightObject = GetComponentInChildren<LightShafts>().gameObject;

        trailGradient = new Gradient();
        trailRenderer = GetComponent<TrailRenderer>();

        headlight = false;
        droneState = (int)DroneState.POWERED_OFF;

        stateMat = container.transform.Find("Status").GetComponent<Renderer>().material;
        colorMat = container.transform.Find("LedRing").GetComponent<Renderer>().material;

        targetPosTransform = container.transform.Find("TargetPos");
        targetPosLine = container.transform.Find("TargetPosLine").GetComponent<LineRenderer>();

        realPosTransform = container.transform.Find("RealPos");
        realPosLine = container.transform.Find("RealPosLine").GetComponent<LineRenderer>();

        base.Awake();
    }

    private void Start()
    {
        sendDroneParameter("setup");
    }


    public override void Update () {
        base.Update();

        if (DroneManager.instance.simulationMode)
        {
            processSimulation();
            return;
        }

        if (_droneState != DroneState.FLYING)
        {
            Vector3 p = realPosition;
            if (_droneState == DroneState.READY || _droneState == DroneState.CALIBRATING) p.y = 0;
            transform.position = p;
        }

        switch (_droneState)
        {
            case DroneState.POWERED_OFF:
                break;

            case DroneState.DISCONNECTED:
                break;

            case DroneState.CONNECTING:
                break;

            case DroneState.CALIBRATING:
                break;

            case DroneState.ANALYSIS:
                break;

            case DroneState.READY:
                break;

            case DroneState.TAKING_OFF:
                break;

            case DroneState.FLYING:
                notifyFlyingPosition();
               
                break;

            case DroneState.LANDING:
                break;

            case DroneState.WARNING:
                break;

            case DroneState.ERROR:
                break;
        }
	}

    public bool isReady()
    {
        return _droneState == Drone.DroneState.READY;
    }

    public void processSimulation()
    {
        notifyFlyingPosition();
    }

    public void notifyFlyingPosition()
    {
        if (transform.position != desiredPosition)
        {
            sendDroneParameter("flight/desiredPosition", transform.position.x, transform.position.y, transform.position.z);
            desiredPosition = transform.position;
        }

        if(oldYaw != targetYaw)
        {
            float mapVal = -targetYaw + 90;
            if (mapVal > 180) mapVal -= 360;
            if (mapVal < -180) mapVal += 360;
            sendDroneParameter("flight/targetYaw", mapVal);
            oldYaw = targetYaw;
        }
    }


    public override void setId(string newId)
    {
        base.setId("drone-"+newId);
        gameObject.name = id;
        droneName = newId;
        updateLabel();
    }


    //Event
    public void stateUpdated()
    {
        //log("Status update : " + droneState);
        stateMat.color = getCurrentStateColor();

        switch (_droneState)
        {
            case DroneState.POWERED_OFF:
                break;

            case DroneState.DISCONNECTED:
                break;

            case DroneState.CONNECTING:
                break;

            case DroneState.CALIBRATING:
                break;

            case DroneState.ANALYSIS:
                break;

            case DroneState.READY:
                break;

            case DroneState.TAKING_OFF:
                break;

            case DroneState.FLYING:
                break;

            case DroneState.LANDING:
                break;

            case DroneState.WARNING:
                break;

            case DroneState.ERROR:
                break;
        }


        if (stateUpdatedEvent != null) stateUpdatedEvent(this);
    }


    //Control
    public void launch(float targetHeight = 1.5f)
    {
        //Debug.Log("Launch drone at " + targetHeight);
        sendDroneParameter("controls/takeOffHeight", targetHeight);
        sendDroneParameter("controls/takeOff");
    }

    public void updateLabel()
    {
        GetComponentInChildren<TMPro.TextMeshPro>().SetText(droneName+":"+ transform.GetSiblingIndex());
    }

    internal void land()
    {
        sendDroneParameter("controls/land");
    }

    public void stop()
    {
        sendDroneParameter("controls/stop");
    }

    public void calibrate()
    {
        sendDroneParameter("controls/calibrate");
    }

    public void moveToPosition(Vector3 pos, float time = 0)
    {
        transform.DOKill();
        if (time == 0) transform.position = pos;
        else transform.DOMove(pos, time).SetEase(Ease.InOutSine);
    }


    public void yawTo(float yaw, float time)
    {
        if (time == 0) targetYaw = yaw;
        else DOTween.To(() => targetYaw, x => targetYaw = x, yaw, time).SetEase(Ease.InOutSine);
    }

    public void colorTo(Color c, float time = 0)
    {
        if (!DroneManager.instance.sendLights) return;
        sendDroneParameter("lights/fadeTime", time);
        sendDroneParameter("lights/lightColor", c.r, c.g, c.b); 
    }

    public void setHeadlight(bool value)
    {
        sendDroneParameter("lights/headlight",value?1:0);
    }

    public void blink(int count, float gap, float onOffRatio)
    {
        for(int i=0;i<count;i++)
        {
            Invoke("headlightOn", i * gap);
            Invoke("headlightOff", (i+onOffRatio) * gap);
        }

        Invoke("autoHeadlight", count * gap);
    }

    //Ease function for blink
    public void headlightOn() { setHeadlight(true); }
    public void headlightOff() { setHeadlight(false); }
    public void autoHeadlight() { setHeadlight(isOver); }
    //Locking
    public bool setLocker(IDroneLocker _locker, bool force = false)
    {
        if (locker == _locker && !force) return true;
        if (isLocked() && !force) return false;
        locker = _locker;
        return true;
    }

    public bool isLocked()
    {
        return locker != null;
    }



    //Log
    public void log(string message)
    {
        Debug.Log(id+" : "+message);
    }

    //Helpers

    void sendDroneParameter(string addressRest, params object[] args)
    {
        OSCMessage msg = new OSCMessage("/"+ id + "/" + addressRest);
        foreach (object arg in args) msg.Append(arg);
        LMFClient.sendMessage(msg); 
    }

    public bool isFlying()
    {
        return DroneManager.instance.simulationMode || _droneState == DroneState.FLYING;
    }

    //UI
    void updateTrailGradient()
    {
        trailGradient.SetKeys(new GradientColorKey[] { new GradientColorKey(colorMat.color, 0), new GradientColorKey(Color.white, 1) }, new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
        trailRenderer.colorGradient = trailGradient;
    }

    public Texture2D getIconForState()
    {
        Texture2D t = null;
        switch (_droneState)
        {
            case DroneState.CALIBRATING: t = Resources.Load<Texture2D>("calibrating"); break;
            case DroneState.ANALYSIS: t = Resources.Load<Texture2D>("analysis"); break;
            case DroneState.TAKING_OFF: t = Resources.Load<Texture2D>("takeoff"); break;
            case DroneState.FLYING: t = Resources.Load<Texture2D>("flying"); break;
        }

        return t;
    }

    public Color getCurrentStateColor()
    {
        Color c = Color.white;

        switch (_droneState)
        {
            case DroneState.POWERED_OFF: c = Color.black; break;
            case DroneState.DISCONNECTED: c = Color.grey; break;

            case DroneState.CONNECTING:
            case DroneState.CALIBRATING:
            case DroneState.ANALYSIS:
                c = Color.cyan;
                break;

            case DroneState.READY:
            case DroneState.TAKING_OFF:
            case DroneState.FLYING:
            case DroneState.LANDING:
                c = Color.green;
                if (lowBattery) c = Color.red;
                break;

            case DroneState.WARNING:
                c = Color.yellow;
                break;

            case DroneState.ERROR:
                c = Color.red;
                break;
        }

        return c;
    }
}

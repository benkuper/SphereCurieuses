using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecktrOSC : Controllable {

    public enum Hand { Left, Right }
    
    public delegate void ButtonTouchEvent(Hand hand, int buttonID, bool pressed, DroneController.ButtonState state);
    public static event ButtonTouchEvent buttonUpdate;

    public float doubleTouchParasiteThreshold;
    bool touchLocked;
    public bool debugTouch;


    public override void Awake()
    {
        TargetScript = this;
        base.Awake();
    }

    [OSCMethod]
    public void touch(int handID, int buttonID, int side, bool value)
    {
        if(debugTouch) Debug.Log("touchUpdate : " + handID + "," + buttonID + "," + side + "," + value);

        if (value)
        {
            if (touchLocked)
            {
                if (debug) Debug.Log("Touch is locked, rejected");
                return;
            }

            //touchLocked = true;
            //Debug.Log("accept touch, lock");
            Invoke("unlockTouch", doubleTouchParasiteThreshold);
        }

       if(debugTouch) Debug.Log(" > Accepted");

        DroneController.ButtonState state = (DroneController.ButtonState)side;
        if (buttonUpdate != null) buttonUpdate((Hand)handID, buttonID, value, state);
        
    }

    public void pitchUpdate(int handID, int pitch)
    {
       // if(debug) Debug.Log("Pitch update");
    }

    public void unlockTouch()
    {
        //Debug.Log("Unlock touch");
        touchLocked = false;
    }
}

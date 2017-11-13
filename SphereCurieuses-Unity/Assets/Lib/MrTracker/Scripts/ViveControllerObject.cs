using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerObject : TrackableObject {

    public const int MENU_BT = 1;
    public const int SIDE_BT = 2;
    public const int TOUCHPAD_BT = 32;
    public const int TRIGGER_BT = 33;

    [Header("Buttons")]
    public bool menuBT;
    public bool sideBT;

    [Header("Trigger")]
    public bool triggerTouch;
    public bool triggerPress;
    public float triggerValue;
    [Header("Touchpad")]
    public bool touchpadTouch;
    public bool touchpadPress;
    public Vector2 touchpadPos;

   
    //convert to main thread with prev/next check
    bool nextMenuBT;
    bool nextSideBT;
    bool nextTriggerTouch;
    bool nextTriggerPress;
    bool nextTouchpadTouch;
    bool nextTouchpadPress;
    

    float rollAtTrigger;
    Vector2 posAtTouch;

    public float rollTriggerRelative;
    public Vector2 touchPosRelative;

    public Vector2 deltaTouch;

    public override void Update()
    {
        base.Update();

        checkNewButtonValues();

        if(triggerPress)
        {
            if (rollAtTrigger == 0) rollAtTrigger = transform.localRotation.eulerAngles.y;  //force using transform in main thread
            float targetRollRelative = transform.localRotation.eulerAngles.y - rollAtTrigger;
            if (targetRollRelative > rollTriggerRelative + 180) targetRollRelative -= 360;
            if (targetRollRelative < rollTriggerRelative - 180) targetRollRelative += 360;
            rollTriggerRelative = targetRollRelative;
        }
    }


    public override void setTrackable(Trackable t)
    {
        base.setTrackable(t);

        if (t != null)
        { 
            t.buttonTouch += buttonTouch;
            t.buttonPress += buttonPress;
            t.axisUpdate += axisUpdate;
        }
    }

    public virtual void buttonTouch(Trackable t, int buttonID, bool value)
    {
        switch (buttonID)
        {
            case TRIGGER_BT:
                nextTriggerTouch = value;
                break;

            case TOUCHPAD_BT:
                nextTouchpadTouch = value;
                posAtTouch = Vector2.zero;
                deltaTouch = Vector2.zero;
                touchPosRelative = Vector2.zero;
                break;
        }
    }

    public virtual void buttonPress(Trackable t, int buttonID, bool value)
    {
        switch (buttonID)
        {
            case MENU_BT:
                nextMenuBT = value;
                break;

            case SIDE_BT:
                nextSideBT = value;
                break;

            case TRIGGER_BT:
                nextTriggerPress = value;
                rollTriggerRelative = 0;
                rollAtTrigger = 0;
                break;

            case TOUCHPAD_BT:
                nextTouchpadPress = value;
                if (!value) deltaTouch = Vector2.zero;
                break;

            default:
                Debug.Log("Button id not handled  " + buttonID);
                break;
        }
    }

    public virtual void axisUpdate(Trackable t, int axisID, Vector2 values)
    {

        switch (axisID)
        {
            case TRIGGER_BT:
                if(triggerValue != values.x)
                {
                    triggerValue = values.x;
                    axisUpdateInternal(axisID,values);
                }
                break;

            case TOUCHPAD_BT:
                if(touchpadPos != values)
                {
                    Vector2 prevPos = touchpadPos;
                    touchpadPos = values;
                    if (posAtTouch == Vector2.zero) posAtTouch = touchpadPos;
                    if (touchpadPos != Vector2.zero && touchpadTouch)
                    {
                       deltaTouch = touchpadPos - prevPos;
                    }
                    touchPosRelative = touchpadPos - posAtTouch;

                    axisUpdateInternal(axisID,values);
                }
                break;
        }
    }

    //Main thread calls
    public virtual void buttonTouchInternal(int buttonID, bool value) { }

    public virtual void buttonPressInternal(int buttonID, bool value) { }

    public virtual void axisUpdateInternal(int axisID, Vector2 values) { }



    void checkNewButtonValues()
    {
        if (nextMenuBT != menuBT)
        {
            menuBT = nextMenuBT;

            //special handling
            CalibrateAutoVive[] cav = FindObjectsOfType<CalibrateAutoVive>();
            if (cav.Length > 0) cav[0].trackable = transform;
            cav[0].calibrate();

            buttonPressInternal(MENU_BT, menuBT);
        }

        if (nextSideBT != sideBT)
        {
            sideBT = nextSideBT;
            buttonPressInternal(SIDE_BT, sideBT);
        }

        if (nextTriggerTouch != triggerTouch)
        {
            triggerTouch = nextTriggerTouch;
            buttonTouchInternal(TRIGGER_BT, triggerTouch);
        }
        if (nextTriggerPress != triggerPress)
        {
            triggerPress = nextTriggerPress;
            buttonPressInternal(TRIGGER_BT, triggerPress);
        }

        if (nextTouchpadTouch != touchpadTouch)
        {
            touchpadTouch = nextTouchpadTouch;
            buttonTouchInternal(TOUCHPAD_BT, touchpadTouch);
        }

        if (nextTouchpadPress != touchpadPress)
        {
            touchpadPress = nextTouchpadPress;
            buttonPressInternal(TOUCHPAD_BT, touchpadPress);
        }
    }
}

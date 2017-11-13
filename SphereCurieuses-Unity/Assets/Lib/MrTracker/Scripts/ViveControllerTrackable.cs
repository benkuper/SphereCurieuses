using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerTrackable : Trackable {

    const int MENU_BT = 1;
    const int SIDE_BT = 0;
    const int TOUCHPAD_BT = 30;
    const int TRIGGER_BT = 31;

    public bool menuBT;
    public bool sideBT;
    public bool triggerTouch;
    public bool triggerPress;
    public float triggerValue;
    public bool touchpadTouch;
    public bool touchpadPress;
    public Vector2 touchpadPos;

    //convert to main thread with prev/next check
    public bool nextMenuBT;
    public bool nextSideBT;
    public bool nextTriggerTouch;
    public bool nextTriggerPress;
    public float nextTriggerValue;
    public bool nextTouchpadTouch;
    public bool nextTouchpadPress;
    public Vector2 nextTouchpadPos;


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(menuBT != nextMenuBT)
        {

        }
	}

    public bool getButtonTouch(int buttonID)
    {
        switch (buttonID)
        {
            case TRIGGER_BT: return triggerTouch;
            case TOUCHPAD_BT: return touchpadTouch;
        }

        return false;
    }

    public bool getButtonPress(int buttonID)
    {
        switch (buttonID)
        {
            case TRIGGER_BT: return triggerPress;
            case TOUCHPAD_BT: return touchpadPress;
            case MENU_BT: return menuBT;
            case SIDE_BT: return sideBT;
        }

        return false;
    }

    public Vector2 getAxisValues(int axisID)
    {
        switch (axisID)
        {
            case TRIGGER_BT: return new Vector2(triggerValue, 0);
            case TOUCHPAD_BT: return touchpadPos;
        }

        return Vector2.zero;
    }

    public override void handleButtonTouch(int buttonID, bool value)
    {
        base.handleButtonTouch(buttonID, value);
        switch(buttonID)
        {
            case TRIGGER_BT:
                triggerTouch = value;
                break;

            case TOUCHPAD_BT:
                touchpadTouch = value;
                break;
        }
    }

    public override void handleButtonPress(int buttonID, bool value)
    {
        base.handleButtonPress(buttonID, value);
        switch (buttonID)
        {
            case MENU_BT:
                menuBT = value;
                break;

            case SIDE_BT:
                sideBT = value;
                break;

            case TRIGGER_BT:
                triggerPress = value;
                break;

            case TOUCHPAD_BT:
                touchpadPress = value;
                break;
        }
    }

    public override void handleAxisUpdate(int axisID, Vector2 values)
    {
        base.handleAxisUpdate(axisID, values);

        switch(axisID)
        {
            case TRIGGER_BT:
                triggerValue = values.x;
                break;

            case TOUCHPAD_BT:
                touchpadPos = values;
                break;
        }
    }



    
}

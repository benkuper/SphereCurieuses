using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCController : ViveControllerObject {

    public DroneController dc;
    public SpecktrOSC.Hand specktrHand;

    public SpecktrOSC.ButtonState[] touchStates;

    public bool disableViveButtons;

	// Use this for initialization
	void Start () {
        dc = GetComponent<DroneController>();
        dc.id = trackableID;

        SpecktrOSC.buttonUpdate += specktrButtonUpdate;
        touchStates = new SpecktrOSC.ButtonState[8];
        for (int i = 0; i < touchStates.Length; i++) touchStates[i] = SpecktrOSC.ButtonState.Off;
	}


    // Events
    private void specktrButtonUpdate(SpecktrOSC.Hand hand, int buttonID, SpecktrOSC.ButtonState buttonState)
    {
        if (hand != specktrHand) return;
        if (touchStates[buttonID] == buttonState) return;
        touchStates[buttonID] = buttonState;

        bool pressed = touchStates[buttonID] != SpecktrOSC.ButtonState.Off;
        dc.setButtonState(buttonID, pressed);
        
    }
    


    public override void axisUpdateInternal(int axisID, Vector2 values)
    {
    }

    public override void buttonTouchInternal(int buttonID, bool value)
    {
    }

    public override void buttonPressInternal(int buttonID, bool value)
    {
        if (disableViveButtons) return;

        switch(buttonID)
        {

            case TOUCHPAD_BT:
                dc.setButtonState(0, value);
                break;

            case TRIGGER_BT:
                dc.setButtonState(1, value);
                break;

            case SIDE_BT:
                dc.setButtonState(2, value);
                break;

            case MENU_BT:
                dc.setButtonState(3, value);
                break;
        }
    }
}

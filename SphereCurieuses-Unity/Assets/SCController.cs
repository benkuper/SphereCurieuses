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

        SpecktrOSC.buttonUpdate += specktrButtonUpdate;
        touchStates = new SpecktrOSC.ButtonState[8];
        for (int i = 0; i < touchStates.Length; i++) touchStates[i] = SpecktrOSC.ButtonState.Off;
	}

    public override void Update()
    {
        base.Update();
        dc.id = trackableID;
        //disableViveButtons = specktrHand == SpecktrOSC.Hand.Left;
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
        if (disableViveButtons)
        {
            switch (buttonID)
            {
                case MENU_BT:
                    CalibrateAutoVive[] cav = FindObjectsOfType<CalibrateAutoVive>();
                    if (cav.Length > 0) cav[0].trackable = transform;
                    cav[0].calibrate();
                    cav[0].saveConfig();
                    break;

                case SIDE_BT:
                    SCController[] controllers = FindObjectsOfType<SCController>();
                    foreach (SCController c in controllers) c.specktrHand = c == this ? SpecktrOSC.Hand.Right : SpecktrOSC.Hand.Left;
                    break;
                    
            }

            return;
        }

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

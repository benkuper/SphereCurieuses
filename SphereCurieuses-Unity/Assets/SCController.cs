using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCController : ViveControllerObject {

    public DroneController dc;
    public SpecktrOSC.Hand specktrHand;

    public bool[] touchPressed;
    public DroneController.ButtonState[] touchStates;

    public bool disableViveButtons;

	// Use this for initialization
	void Start () {
        dc = GetComponent<DroneController>();

        SpecktrOSC.buttonUpdate += specktrButtonUpdate;
        touchStates = new DroneController.ButtonState[8];
        touchPressed = new bool[8];
        for (int i = 0; i < touchStates.Length; i++) touchStates[i] = DroneController.ButtonState.Off;
	}

    public override void Update()
    {
        base.Update();
        dc.id = trackableID;
        //disableViveButtons = specktrHand == SpecktrOSC.Hand.Left;
    }

    // Events
    private void specktrButtonUpdate(SpecktrOSC.Hand hand, int buttonID, bool pressed, DroneController.ButtonState buttonState)
    {
       // Debug.Log("Spekctr Button update in SC Controller " + hand+" > "+buttonID + " > " + pressed + " > buttonState : " + buttonState);
        if (hand != specktrHand) return;
        if (touchStates[buttonID] == buttonState && touchPressed[buttonID] == pressed) return;
        touchStates[buttonID] = buttonState;
        touchPressed[buttonID] = pressed;

        dc.setButtonState(buttonID, pressed, buttonState);
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
                    /*
                    CalibrateAutoVive[] cav = FindObjectsOfType<CalibrateAutoVive>();
                    if (cav.Length > 0) cav[0].trackable = transform;
                    cav[0].calibrate();
                    cav[0].saveConfig();
                    */
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
                dc.setButtonState(0, value,DroneController.ButtonState.Up);
                break;

            case TRIGGER_BT:
                dc.setButtonState(1, value, DroneController.ButtonState.Up);
                break;

            case SIDE_BT:
                dc.setButtonState(2, value,DroneController.ButtonState.Up);
                break;

            case MENU_BT:
                dc.setButtonState(3, value,DroneController.ButtonState.Up);
                break;
        }
    }
}

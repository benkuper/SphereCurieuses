using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecktrOSC : OSCControllable {

    public enum Hand { Left, Right }
    public enum ButtonState { Up, Side, Down, Off}

    public delegate void ButtonTouchEvent(Hand hand, int buttonID, ButtonState state);
    public static event ButtonTouchEvent buttonUpdate;

    public float doubleTouchParasiteThreshold;
    bool touchLocked;
    public bool debug;



    [OSCMethod("touchUpdate")]
    public void handleButtonUpdate(int handID, int buttonID, int side, bool value)
    {
        if(debug) Debug.Log("touchUpdate : " + handID + "," + buttonID + "," + side + "," + value);

        if (value)
        {
            if (touchLocked)
            {
                Debug.Log("Lock is touched !");
                return;
            }

            touchLocked = true;
            Debug.Log("accept touch, lock");
            Invoke("unlockTouch", doubleTouchParasiteThreshold);
        }

        ButtonState state = value ? (ButtonState)side : ButtonState.Off;
        if (buttonUpdate != null) buttonUpdate((Hand)handID, buttonID, state);
        
    }

    [OSCMethod("pitchUpdate")]
    public void handlePitchUpdate(int handID, int pitch)
    {
        if(debug) Debug.Log("Pitch update");
    }

    public void unlockTouch()
    {
        Debug.Log("Unlock touch");
        touchLocked = false;
    }
}

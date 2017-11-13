using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecktrOSC : OSCControllable {
 
    public enum HandSide { Left, Right}
    public HandSide handSide;

    public delegate void ButtonTouchEvent(SpecktrOSC s, int buttonID, bool value);
    public event ButtonTouchEvent buttonUpdate;

    public bool[] buttonStates;
    
    // Use this for initialization
    void Start () {
        buttonStates = new bool[8];
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    [OSCMethod("button")]
    void handleButtonUpdate(int buttonID,bool value)
    {
        buttonStates[buttonID] = value;
        if (buttonUpdate != null) buttonUpdate(this,buttonID, value);
    }
}

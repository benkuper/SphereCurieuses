using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCController : ViveControllerObject {

    public SpecktrOSC specktr;
    
	// Use this for initialization
	void Start () {
        if (specktr != null) specktr.buttonUpdate += specktrButtonUpdate;
	}

    // Update is called once per frame
    override public void Update () {
		
	}


    // Events
    private void specktrButtonUpdate(SpecktrOSC s, int buttonID, bool value)
    {
        Debug.Log("Button update here !");
    }

    public override void axisUpdateInternal(int axisID, Vector2 values)
    {
    }

    public override void buttonTouchInternal(int buttonID, bool value)
    {
    }

    public override void buttonPressInternal(int buttonID, bool value)
    {
    }
}

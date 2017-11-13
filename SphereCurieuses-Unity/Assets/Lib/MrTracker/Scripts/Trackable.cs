using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trackable
{
    public delegate void ButtonTouchEvent(Trackable t, int buttonID, bool value);
    public event ButtonTouchEvent buttonTouch;
    public delegate void ButtonPressEvent(Trackable t, int buttonID, bool value);
    public event ButtonPressEvent buttonPress;
    public delegate void AxisUpdateEvent(Trackable t, int axisID, Vector2 values);
    public event AxisUpdateEvent axisUpdate;

    public int sourceType;
    public int type;
    public int id;
    public Vector3 position;
    public Vector3 centroid;
    public Quaternion rotation;
    public Vector3 size = Vector3.one;

    public bool hasBeenUpdated;
    public float timeSinceLastUpdate;

    public virtual void handleButtonTouch(int buttonID, bool value)
    {
        if (buttonTouch != null) buttonTouch(this, buttonID, value);
    }
    public virtual void handleButtonPress(int buttonID, bool value)
    {
        if (buttonPress != null) buttonPress(this, buttonID, value);
    }
    public virtual void handleAxisUpdate(int axisID, Vector2 values)
    {
        if (axisUpdate != null) axisUpdate(this, axisID, values);
    }
}

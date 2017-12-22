using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SCController))]
public class SCControllerEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        if (GUILayout.Button("Vibrate")) MrTrackerClient.instance.sendVibrate((target as SCController).trackableID, .5f, .2f);
    }
}

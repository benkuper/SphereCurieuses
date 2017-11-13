using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Drone))]
public class DroneEditor : Editor {

	// Use this for initialization
	public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        if (GUILayout.Button("Reset Kalman Estimation")) ((Drone)target).resetKalman();
        if (GUILayout.Button("Launch")) ((Drone)target).launch();
        if (GUILayout.Button("Stop")) ((Drone)target).stop();
        if (GUILayout.Button("Home")) ((Drone)target).goHome();
    }
}

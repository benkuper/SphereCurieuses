using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DroneManager))]
public class DroneManagerEditor : Editor {

	// Use this for initialization
	public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        if (GUILayout.Button("Reset All Kalman")) ((DroneManager)target).resetAllKalman();
        if (GUILayout.Button("Launch All")) ((DroneManager)target).launchAll();
        if (GUILayout.Button("Launch Differed")) ((DroneManager)target).launchAll();
        if (GUILayout.Button("Stop All")) ((DroneManager)target).stopAll();
    }
}

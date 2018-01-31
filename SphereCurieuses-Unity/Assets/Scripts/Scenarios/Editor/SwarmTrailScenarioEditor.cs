using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SwarmTrailScenario))]
public class SwarmTrailScenarioEditor : Editor {

    // Use this for initialization
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        if (GUILayout.Button("Clear loops and selection")) ((SwarmTrailScenario)target).clearLoopsAndSelection();
        if (GUILayout.Button("Clear and stop drones")) ((SwarmTrailScenario)target).clearAndStopDrones();
        if (GUILayout.Button("Select all drones")) ((SwarmTrailScenario)target).selectAllDrones();
    }
}

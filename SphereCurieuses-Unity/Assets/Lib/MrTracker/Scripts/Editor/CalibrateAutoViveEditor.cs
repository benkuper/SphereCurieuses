using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CalibrateAutoVive))]
public class CalibrateAutoViveEditor : Editor {

    CalibrateAutoVive cav;

    public override void OnInspectorGUI() 
    {
        if (cav == null) cav = target as CalibrateAutoVive;

        DrawDefaultInspector(); 

        if (GUILayout.Button("Calibrate")) cav.calibrate();
        if (GUILayout.Button("Save")) cav.saveConfig();
        if (GUILayout.Button("Load")) cav.loadConfig();
    }
}

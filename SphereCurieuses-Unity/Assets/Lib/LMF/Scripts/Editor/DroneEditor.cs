using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Drone))]
public class DroneEditor : Editor {

    Drone drone;

    public override void OnInspectorGUI()
    {
        if (drone == null) drone = target as Drone;

        GUIStyle s = new GUIStyle();
        s.normal.textColor = drone.getCurrentStateColor();
        EditorGUILayout.HelpBox("Drone state : " + (Drone.DroneState)drone.droneState, (Drone.DroneState)drone.droneState == Drone.DroneState.WARNING?MessageType.Warning:((Drone.DroneState)drone.droneState == Drone.DroneState.ERROR?MessageType.Error:MessageType.None));
        DrawDefaultInspector();


        EditorGUILayout.HelpBox("Real position : " + drone.realPosition.ToString() + "\nTarget Position :" + drone.targetPosition.ToString() + "\nDesiredPosition :" + drone.desiredPosition.ToString()+"\nOrientation :" +drone.orientation.ToString(), MessageType.None);


        bool hl = GUILayout.Toggle(drone.headlight, "Headlight");
        if (hl != drone.headlight) drone.setHeadlight(hl);
        if (GUILayout.Button("Blink")) drone.blink(3, .2f, .5f);

    }
}

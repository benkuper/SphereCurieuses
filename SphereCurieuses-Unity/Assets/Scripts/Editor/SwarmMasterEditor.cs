using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SwarmMaster))]
public class SwarmMasterEditor : Editor {

    GenericMenu scenarioMenu;
    SwarmMaster m;
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        if(m == null) m = (SwarmMaster)target;

        scenarioMenu = new GenericMenu();
        foreach (SwarmScenario s in m.scenarios) scenarioMenu.AddItem(new GUIContent(s.scenarioName), m.currentScenario == s, scenarioMenuSelected, s);

        EditorGUILayout.LabelField("Current Scenario :");
        if (EditorGUILayout.DropdownButton(new GUIContent(m.currentScenario != null?m.currentScenario.scenarioName:"<None>"),FocusType.Passive))
        {
            scenarioMenu.ShowAsContext();
        }

    }

    void scenarioMenuSelected(object data)
    {
        SwarmScenario s = (SwarmScenario)data;
        m.setCurrentScenario(s);
    }
}

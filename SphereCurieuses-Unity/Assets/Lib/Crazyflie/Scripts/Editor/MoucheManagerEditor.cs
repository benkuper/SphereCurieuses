using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MoucheManager))]
public class MoucheManagerEditor : Editor {


	// Use this for initialization
	public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        MoucheManager m = (MoucheManager)target;

       if (GUILayout.Button("Setup")) m.setup();
    }
}

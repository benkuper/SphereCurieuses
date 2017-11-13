using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Test
{

}

public class Node : OSCControllable {

    [OSCProperty("nodeid")]
    public int id;

    public string nodeName;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void setName(string n)
    {
        oscName = "node-" + n;
        nodeName = n;
        gameObject.name = "Node " + n;

        MoucheManager.sendMessage(new OSCMessage("/" + oscName + "/setup"));
    }


    [OSCMethod("position")]
    public void setPosition(Vector3 pos)
    {
        transform.position = pos;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one *.1f);
    }

    private void OnDrawGizmosSelected()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleCenter;

        Vector3 position = transform.position + Vector3.up * .2f + Vector3.right * .2f;
        Handles.Label(position, nodeName, style);
    }

#endif
}

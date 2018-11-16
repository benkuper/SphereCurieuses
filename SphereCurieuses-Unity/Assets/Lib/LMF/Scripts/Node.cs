using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class Node : Controllable {

    [OSCProperty]
    public int nodeid;

    public string nodeName;


    public override void Awake()
    {
        base.Awake();
        TargetScript = this;
    }

    public void setName(string n)
    {
        id = "node-" + n;
        nodeName = n;
        gameObject.name = "Node " + n;

        LMFClient.sendMessage(new OSCMessage("/" + id + "/setup"));
    }


    [OSCMethod]
    public void position(Vector3 pos)
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

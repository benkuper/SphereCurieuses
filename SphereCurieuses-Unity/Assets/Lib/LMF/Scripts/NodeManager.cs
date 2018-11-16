using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NodeManager : Controllable
{

    //public GameObject nodePrefab;
    //List<Node> nodes;
    Transform cage;

    public override void Awake()
    {

        //nodes = new List<Node>();
        cage = transform.Find("Cage");

        TargetScript = this;
        usePanel = false;
        base.Awake();


    }
    // Use this for initialization
    void Start()
    {

    }

    public override void Update()
    {
        base.Update();
    /*
        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;
        foreach(Node n in nodes)
        { 
            min = new Vector3(Mathf.Min(n.transform.position.x, min.x), Mathf.Min(n.transform.position.y, min.y), Mathf.Min(n.transform.position.z, min.z));
            max = new Vector3(Mathf.Max(n.transform.position.x, max.x), Mathf.Max(n.transform.position.y, max.y), Mathf.Max(n.transform.position.z, max.z));
        }

        cage.position = (min + max) / 2;
        cage.localScale = max - min;
        */
    }

    [OSCMethod]
    public void setup(Vector3 size, float offset)
    {

        cage.position = Vector3.up * (offset + size.y / 2);
        cage.localScale = size;
    }
}

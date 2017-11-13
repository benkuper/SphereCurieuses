using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NodeManager : OSCControllable
{

    public GameObject nodePrefab;
    List<Node> nodes;

    //Debug
    public Mesh hullMesh;
    Mesh _hullMesh;
    public Material mat;
    int[][] vIndices;

    void Awake()
    {
        nodes = new List<Node>();

        _hullMesh = new Mesh();
        _hullMesh.vertices = hullMesh.vertices;
        _hullMesh.triangles = hullMesh.triangles;
        _hullMesh.uv = hullMesh.uv;
        _hullMesh.normals = hullMesh.normals;

        vIndices = new int[][] { new int[] { 1, 14, 16 }, new int[] { 4, 10, 21 }, new int[] { 7, 15, 19 }, new int[] { 2, 8, 22 },
                                 new int[] { 0, 13, 23 }, new int[] { 3, 9, 17 }, new int[] { 6, 12, 20 }, new int[] { 5, 11, 18 } };
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    [OSCMethod("setup", packInArray = true)]
    public void setup(object[] data)
    {
        clean();
        for (int i = 0; i < data.Length; i++) addNode((string)data[i]);
    }

    public void clean()
    {

        foreach (Node n in nodes) Destroy(n.gameObject);
        nodes = new List<Node>();
    }

    public void addNode(string nodeName)
    {
        Node n = Instantiate(nodePrefab).GetComponent<Node>();
        n.setName(nodeName);
        n.transform.SetParent(transform, true);
        n.transform.position = Vector3.right * nodes.Count * .2f;
        nodes.Add(n);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) drawNodeHull();
    }


    private void drawNodeHull()
    {
        Vector3[] vertices = hullMesh.vertices;
        for(int i=0; i<8;i++)
        {
            Node n = getNodeByID(i);
            if (n == null) continue;
            for(int j=0;j<3;j++)
            {
                vertices[vIndices[i][j]] = n.transform.position;
            }
        }
        _hullMesh.vertices = vertices;
        _hullMesh.RecalculateNormals();

        mat.SetPass(0);
        Graphics.DrawMeshNow(_hullMesh, Vector3.zero, Quaternion.identity, 0);
    }
#endif

    public Node getNodeByID(int id)
    {
        if (nodes == null) return null;
        foreach (Node n in nodes) if (n.id == id) return n;
        return null;
    }
}

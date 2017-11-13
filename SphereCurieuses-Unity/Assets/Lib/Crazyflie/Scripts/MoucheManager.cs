using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class MoucheManager : MonoBehaviour {

    public static MoucheManager instance;

    public string host;
    public int port;

    public bool queryAtStart;

    void Awake()
    {
        instance = this;
    }
	// Use this for initialization
	void Start () {
        if (queryAtStart) setup();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void setup()
    {
        OSCMessage m = new OSCMessage("/setup");
        OSCMaster.sendMessage(m, host, port);
    }

    public static void sendMessage(OSCMessage m)
    {
        OSCMaster.sendMessage(m, instance.host, instance.port);
    }
}

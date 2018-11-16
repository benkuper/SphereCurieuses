using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class LMFClient : MonoBehaviour {

    public static LMFClient instance;

    [Header("Connection")]
    public string remoteHost = "127.0.0.1";
    public int remotePort = 13000;

    void Awake()
    {
        instance = this;
        OSCMaster.instance.messageAvailable += messageReceived;
    }

    private void messageReceived(OSCMessage m)
    {
        if(m.Address == "/setup")
        {

        }
    }

    public static void sendMessage(OSCMessage m)
    {
        OSCMaster.sendMessage(m, instance.remoteHost, instance.remotePort);
    }
}

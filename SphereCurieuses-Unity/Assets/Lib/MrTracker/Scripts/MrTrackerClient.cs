using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;


public class MrTrackerClient : MonoBehaviour
{
    public static MrTrackerClient instance;

    public enum SourceType {ALL=0, AUGMENTA =1, VIVE=2};
    public enum ViveTypeFilter { ALL, HMD, CONTROLLER, LIGHTHOUSE, TRACKER };

    const byte TRACKABLE_PACKET_ID = 0x01;
    const byte POSITION_PACKET_ID = 0x06;
    const byte CENTROID_PACKET_ID = 0x02;
    const byte ROTATION_PACKET_ID = 0x04;
    const byte SIZE_PACKET_ID = 0x30;
    const byte BUTTON_TOUCH_ID = 0x40;
    const byte BUTTON_PRESS_ID = 0x41;
    const byte AXIS_ID = 0x42;
   
    public int localPort = 9000;
    public int remotePort = 11500;
    public float trackableLifeThreshold = .1f;

    Dictionary<int,Trackable> trackables;

    List<Trackable> trackablesToAdd;
    List<Trackable> trackablesToRemove;
    UdpClient client;
    UdpClient server;

    public bool invertX;
    public float offsetYaw;
    public bool IsConnected, IsProcessing;

    public delegate void TrackableAddedEvent(Trackable t);
    public event TrackableAddedEvent trackableAdded;
    public delegate void TrackableRemoveEvent(Trackable t);
    public event TrackableAddedEvent trackableRemoved;
    public delegate void TrackableUpdatedEvent(Trackable t);
    public event TrackableAddedEvent trackableUpdated;

    // Use this for initialization
    void Start()
    {
        instance = this;

        IsConnected = false;
        localPort = PlayerPrefs.GetInt("MrTracker_Port", 9000);
        Connect();

        server = new UdpClient();
        server.Connect(new IPEndPoint(IPAddress.Loopback, remotePort));

        trackablesToAdd = new List<Trackable>();
        trackables = new Dictionary<int, Trackable>();
        trackablesToRemove = new List<Trackable>();
    }

    public void Connect()
    {
        if (client != null)
        {
            client.Close();
            IsConnected = false;
        }
        try
        {
            client = new UdpClient(localPort);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error : " + e);
            IsConnected = false;
            return;
        }
        
        client.BeginReceive(new AsyncCallback(udpReceiveHandler), null);
        Debug.Log("MrTracket connected on " + localPort);
        
        IsConnected = true;
    }

    void OnDisable()
    {
        IsConnected = false;
        client.Close();
    }

    // Update is called once per frame
    void Update()
    {
        foreach(KeyValuePair<int,Trackable> tk in trackables)
        {
            Trackable t = tk.Value;
            if (t.hasBeenUpdated)
            {
                t.timeSinceLastUpdate = Time.time;
                t.hasBeenUpdated = false;
            }

            if(Time.time > t.timeSinceLastUpdate + trackableLifeThreshold)
            {
                trackablesToRemove.Add(t);
            }
        }

        foreach(Trackable t in trackablesToAdd) if (trackableAdded != null) trackableAdded(t);
        foreach (Trackable t in trackablesToRemove)
        {
            if (trackableRemoved != null)
                trackableRemoved(t);
            trackables.Remove(t.id);
        }

        trackablesToAdd.Clear();
        trackablesToRemove.Clear();

        if (trackables.Count == 0)
            IsProcessing = false;
    }

    void processData(byte[] data)
    {
        IsProcessing = true;
        if (data[0] != TRACKABLE_PACKET_ID) return;

        byte expectedSize = data[1];
        if(expectedSize != data[1])
        {
            Debug.LogWarning("Expected Size " + expectedSize+" is different from packet size, stopping");
            return;
        }

        var sourceType = data[2];
        var trackableType = data[3];

        var trackableID = BitConverter.ToInt32(data, 4);
        var numSubPackets = data[8];
        Trackable t = null;

        if(!trackables.TryGetValue(trackableID,out t))
        {
            //Debug.Log("New tracker");
            t = new Trackable();
            t.sourceType = sourceType;
            t.type = trackableType;
            t.id = trackableID;
            trackables.Add(t.id, t);
            trackablesToAdd.Add(t);
        }
        

        int curIndex = 9;
        
        for(int i=0;i<numSubPackets;i++)
        {
            byte packetID = data[curIndex];
            byte packetSize = data[curIndex + 1];
            
            switch(packetID)
            {
                case POSITION_PACKET_ID:
                    t.position = new Vector3(BitConverter.ToSingle(data, curIndex + 2)*(invertX?-1:1), BitConverter.ToSingle(data, curIndex + 6), BitConverter.ToSingle(data, curIndex + 10));
                    break;

                case ROTATION_PACKET_ID:
                    t.rotation = Quaternion.Euler(BitConverter.ToSingle(data, curIndex + 2), BitConverter.ToSingle(data, curIndex + 6)+offsetYaw, BitConverter.ToSingle(data, curIndex + 10));
                    break;

                case CENTROID_PACKET_ID:
                    t.centroid = new Vector3(BitConverter.ToSingle(data, curIndex + 2), BitConverter.ToSingle(data, curIndex + 6), BitConverter.ToSingle(data, curIndex + 10));
           
                    break;

                case SIZE_PACKET_ID:
                    t.size = new Vector3(BitConverter.ToSingle(data, curIndex + 2), BitConverter.ToSingle(data, curIndex + 6), BitConverter.ToSingle(data, curIndex + 10));
                    break;

                case BUTTON_TOUCH_ID:
                    {
                        int buttonID = data[curIndex + 2];
                        bool value = data[curIndex + 3] > 0;
                        t.handleButtonTouch(buttonID, value);
                    }
                    break;

                case BUTTON_PRESS_ID:
                    {
                        int buttonID = data[curIndex + 2];
                        bool value = data[curIndex + 3] > 0;
                        t.handleButtonPress(buttonID, value);
                    }
                    break;

                case AXIS_ID:
                    {
                        int axisID = data[curIndex + 2];
                        Vector2 values = new Vector2(BitConverter.ToSingle(data, curIndex + 3), BitConverter.ToSingle(data, curIndex + 7));
                        t.handleAxisUpdate(axisID, values);
                    }
                    break;
            }

            curIndex += packetSize;
        }

        t.hasBeenUpdated = true;
        if (trackableUpdated != null) trackableUpdated(t);

    }

    void udpReceiveHandler(IAsyncResult res)
    {
        IsProcessing = false;
        IPEndPoint remoteIP = null;
        byte[] data = client.EndReceive(res, ref remoteIP);
        try
        {
            processData(data);
        }
        catch(Exception e)
        {
            Debug.LogWarning("Error processing data : " + e.Message+"\n"+ e.StackTrace);
        }
        client.BeginReceive(new AsyncCallback(udpReceiveHandler), null);
    }

    public void sendVibrate(int controllerID, float strength, float time)
    {
        List<byte> msg = new List<byte>() { (byte)'v', (byte)controllerID };
        msg.AddRange(BitConverter.GetBytes(strength));
        msg.AddRange(BitConverter.GetBytes(time));
        msg.Add(255);
        server.Send(msg.ToArray(), msg.Count);

    }


    void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("MrTracker_Port", localPort);
    }
}

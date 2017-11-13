using UnityEngine;
using System.Collections;
using UnityOSC;


public class OSCMaster : MonoBehaviour {

    public static OSCMaster instance;

    OSCServer server;
    public int port = 6000;

    OSCClient client;

    public bool logIn;
    public bool logOut;

    OSCControllable[] controllables;

    private void Awake()
    {
        instance = this;

        server = new OSCServer(port);
        server.PacketReceivedEvent += packetReceived;
        server.Connect();

        client = new OSCClient(System.Net.IPAddress.Loopback, 0, false);

        updateListOfControllables();
    }

    // Use this for initialization
    void Start () {
        
	}

    void updateListOfControllables()
    {
 
        controllables = FindObjectsOfType<OSCControllable>();
    }

    
    void packetReceived(OSCPacket p)
    {
        
        OSCMessage m = (OSCMessage)p;
        string[] addSplit = m.Address.Split(new char[] { '/' });

        if (addSplit.Length != 3) return;

        string target = addSplit[1];
        string property = addSplit[2];

        if(logIn)
        {
            string args = "";

            for (int i = 0; i < m.Data.Count; i++)
            {
                object d = m.Data[i];
                args += (i > 0 ? ", " : "") + d.ToString();
            }
            Debug.Log("Received " + m.Address + " : " + args);
        }

        OSCControllable c = getControllableForID(target);
        if (c == null)
        {
            updateListOfControllables();
            c = getControllableForID(target);
            if (c == null) return;
        }

         c.setProp(property, m.Data);
    }

    OSCControllable getControllableForID(string id)
    {
        foreach(OSCControllable c in controllables)
        {
            if (c.oscName == id) return c;
        }
        return null;
    }
	
    public static void sendMessage(OSCMessage m, string host, int port)
    {
        if (instance.logOut)
        {
            string args = "";
            for (int i = 0; i < m.Data.Count; i++) args += (i > 0 ? ", " : "") + m.Data[i].ToString();
            Debug.Log("Sending " + m.Address + " : "+args);
        }

        instance.client.SendTo(m, host, port);
    }

	// Update is called once per frame
	void Update () {
         server.Update();
	}


    void OnDestroy()
    {
        server.Close();
    }
}

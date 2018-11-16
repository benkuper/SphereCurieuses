using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DroneLoop : MonoBehaviour, IDroneLocker
{
    Drone drone;
    bool isEnabled;

    public float timeAtRecord;
    public float recordRate;
    public float loopTime;
    public float playPosition;

    public bool isRecording;
    public bool isPlaying;


    public List<KeyValuePair<float, Vector3>> timePos;

    public void setup(Drone d)
    {
        drone = d;
        isEnabled = true;
        timePos = new List<KeyValuePair<float, Vector3>>();

        recordRate = 1.0f / 20; //20Hz
    }

    ~DroneLoop()
    {
        drone.setLocker(null);
    }

    private void Update()
    {
        if (!isEnabled) return;

        if (isRecording)
        {
            if (timePos.Count == 0 || (Time.time - timeAtRecord) > timePos[timePos.Count - 1].Key + recordRate) recordPosition();
        }

        if (isPlaying)
        {
            playPosition = (playPosition + Time.deltaTime) % loopTime;
            Vector3 p = getPositionForTime(playPosition);
            if(p != Vector3.zero) drone.moveToPosition(p,recordRate);
        }
    }

    public void startRecord()
    {
        timePos.Clear();
        timeAtRecord = Time.time;
        isPlaying = false;
        isRecording = true;
    }

    public void recordPosition()
    {
        timePos.Add(new KeyValuePair<float, Vector3>(Time.time - timeAtRecord, drone.transform.position));
    }

    public void stopRecord()
    {

        //timePos.Add(timePos[0]);

        /*
        Vector3 first = timePos[0].Value;
        Vector2 last = timePos[timePos.Count - 1].Value;

        float dist = Vector3.Distance(first,last);

        
        int numSteps = (int)(dist / (recordRate * gapSpeed));
        for(float i=0;i<numSteps;i++)
        {
            timePos.Add(new KeyValuePair<float,Vector3>(Time.time + i * recordRate, Vector3.Lerp(first, last, i * 1.0f / numSteps)));
        }
        */

        loopTime = Time.time - timeAtRecord;
        isRecording = false;
        playPosition = 0;
    }

    public void play()
    {
        if (isRecording) stopRecord();

        isPlaying = true;
        drone.setLocker(this);
    }

    public void pause()
    {
        isPlaying = false;
        if ((Object)drone.locker == this) drone.setLocker(null);
    }

    public void stop()
    {
        isPlaying = false;
        playPosition = 0;
        if ((Object)drone.locker == this) drone.setLocker(null);
    }

    public Vector3 getPositionForTime(float relativeTime)
    {
        if (timePos.Count == 0) return Vector3.zero;
        if (relativeTime < timePos[0].Key) return timePos[0].Value;
        for (int i = timePos.Count -1; i >= 0; i--) if (relativeTime >= timePos[i].Key) return timePos[i].Value; //Can improve perf with dichotomy
        
        return Vector3.zero;
    }

    public void releaseDrone(Drone d)
    {
        if (d != drone)
        {
            Debug.Log("Should not be here !");
            return;
        }

        isEnabled = false;
        drone.setLocker(null);
    }


    void OnDrawGizmos()
    {
        if (timePos.Count < 2) return;
        
        for (int i = 0; i < timePos.Count; i++)
        {
            if (isRecording) Gizmos.color = Color.yellow;
            else if (isPlaying) Gizmos.color = Color.Lerp(Color.blue, Color.red, timePos[i].Key / loopTime);
            else Gizmos.color = Color.grey;

            if(i > 0) Gizmos.DrawLine(timePos[i - 1].Value, timePos[i].Value);
            Gizmos.DrawWireCube(timePos[i].Value, Vector3.one * .02f);
        }
    }
}

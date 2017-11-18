using UnityEngine;
using System.Collections.Generic;

public class Trail
{
    public List<Vector3> samples;
    public Drone headDrone;
    
    public Trail()
    {
        samples = new List<Vector3>();
    }


    public float getTotalDistance()
    {
        float d = 0;
        int numSamples = samples.Count;
        for (int i = 1; i < numSamples; i++)
        {
            d += Vector3.Distance(samples[i], samples[i - 1]);
        }

        return d;
    }

    public Vector3 getPosAtDistanceFromEnd(float distance)
    {

        
        float d = 0;
        int numSamples = samples.Count;
        if (numSamples == 0) return Vector3.zero;
        if (distance == 0) return samples[numSamples - 1];

        for (int i = numSamples - 1; i >= 1; i--)
        {
            float nd = Vector3.Distance(samples[i], samples[i - 1]);
            if (d + nd < distance) d += nd;
            else
            {
                float relD = (distance - d) / nd;
                return Vector3.Lerp(samples[i], samples[i - 1], relD);
            }
        }

        return samples[numSamples - 1];
    }

    public Vector3 getPosAtDistanceFromStart(float distance)
    {
        float d = 0;
        int numSamples = samples.Count;
        if (numSamples == 0) return Vector3.zero;
        if (distance == 0) return samples[0];

        for (int i = 1; i < numSamples; i++)
        {
            float nd = Vector3.Distance(samples[i - 1], samples[i]);
            if (d + nd < distance) d += nd;
            else
            {
                float relD = (distance - d) / nd;
                return Vector3.Lerp(samples[i - 1], samples[i], relD);
            }
        }

        return samples[0];
    }
}
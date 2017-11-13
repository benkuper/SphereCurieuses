using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmScenario : MonoBehaviour {

    public string scenarioName;
    public bool isCurrent;

    public float startTime;
    public float endTime;
    protected float timeAtStart;
    protected float timeAtEnd;
    public bool isStarting { get { return isCurrent && Time.time < timeAtStart + startTime; } }
    public bool isEnding { get { return Time.time < timeAtEnd + endTime; } }

    public virtual void Awake()
    {
        isCurrent = false;
    }

    public virtual void Start() {

    }
    
    public virtual void Update() {
        if (isCurrent && !isStarting && !isEnding) updateScenario();
    }

    public virtual void startScenario() { }
    public virtual void updateScenario() { }
    public virtual void endScenario() { }

    public void setCurrent(bool value)
    {
        if (isCurrent == value) return;
        isCurrent = value;

        if(isCurrent)
        {
            //start
            timeAtStart = Time.time;
            startScenario();
        }
        else
        {
            //end
            timeAtEnd = Time.time;
            endScenario();

        }
    }
}

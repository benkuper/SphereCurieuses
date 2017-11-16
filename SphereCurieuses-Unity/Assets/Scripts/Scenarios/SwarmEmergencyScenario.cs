using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmEmergencyScenario : SwarmScenario
{

    override public void Start()
    {

    }

    public override void startScenario()
    {
        SwarmMaster.instance.homeAllDrones();
    }

    override public void updateScenario()
    {

    }
}

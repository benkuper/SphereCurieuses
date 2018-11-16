using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LighthouseTrackable : TrackableObject
{
    public override void setTrackable(Trackable t)
    {
        base.setTrackable(t);
        GetComponentInChildren<TMPro.TextMeshPro>().SetText(trackable.id.ToString());
    }
}

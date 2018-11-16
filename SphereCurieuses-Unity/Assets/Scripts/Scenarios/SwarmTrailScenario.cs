using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SwarmTrailScenario : SwarmScenario
{

    const int SELECTION = 0; //index on thumb
    const int MOVE = 1; // major on thumb, also used for zoom in
    const int LOOP = 2; // annular on thumb, alose used for zoom out
    const int TRAIL = 3; // auricular on thumb

    const int SELECTION_CLEAR = 4; //index on palm
    const int CLEAR = 5; //major on palm

    public enum SelectionMode { Touch, Paint }
    [Header("Selections")]
    public SelectionMode selectionMode;
    public bool clearSelectionOnTouch;

    [Header("Light Feedback")]
    public Color idleGroundColor;
    public Color idleFlyingColor;
    public Color selectedColor;
    public Color movingColor;
    public Color trailColor;
    public Color trailHeadColor;
    public Color loopingRecordingColor;
    public Color loopingColor;

    [Header("Trail")]
    public bool autoDistance;
    public float trailDroneDistance;
    public float minAutoDroneDistance;
    //public float debugDist;

    [Header("Zoom")]
    public float zoomSpeed;

    [Header("Look")]
    public Transform lookAtTarget;

    [Header("Takeoff")]
    public float minTakeOffPitch;
    public float maxTakeOffPitch;
    public float minTakeOffHeight;
    public float maxTakeOffHeight;
    [Range(0, 1)]
    public float relPitch;
    public float takeOffHeight;

    DroneController[] controllers;
    Dictionary<Drone, DroneLoop> loops;
    Dictionary<DroneController, List<List<DroneLoop>>> orderedLoopGroups;

    Dictionary<DroneController, List<Drone>> droneSelection;
    Dictionary<DroneController, Dictionary<Drone, Vector3>> droneManipulationOffsets; //Rigid constraint when manipulating drones
    Dictionary<DroneController, bool> isMovingDrones;
    Dictionary<DroneController, bool> isRecordingDroneLoop;
    Dictionary<DroneController, Trail> trails;
    Dictionary<DroneController, List<float>> trailsDronesDistances;
    Dictionary<DroneController, float> zoomFactors;




    public override void Start()
    {
        base.Start();
        DroneManager.instance.droneStateUpdate += droneStateUpdate;
    }



    public override void startScenario()
    {
        base.startScenario();

        Debug.Log("Start scenario !");
        loops = new Dictionary<Drone, DroneLoop>();
        orderedLoopGroups = new Dictionary<DroneController, List<List<DroneLoop>>>();

        droneSelection = new Dictionary<DroneController, List<Drone>>();
        droneManipulationOffsets = new Dictionary<DroneController, Dictionary<Drone, Vector3>>();
        isMovingDrones = new Dictionary<DroneController, bool>();
        isRecordingDroneLoop = new Dictionary<DroneController, bool>();
        trails = new Dictionary<DroneController, Trail>();
        trailsDronesDistances = new Dictionary<DroneController, List<float>>();

        zoomFactors = new Dictionary<DroneController, float>();

        controllers = FindObjectsOfType<DroneController>();
        foreach (var dc in controllers)
        {
            orderedLoopGroups.Add(dc, new List<List<DroneLoop>>());

            droneSelection.Add(dc, new List<Drone>());
            droneManipulationOffsets.Add(dc, new Dictionary<Drone, Vector3>());
            isMovingDrones.Add(dc, false);
            isRecordingDroneLoop.Add(dc, false);
            trails.Add(dc, null);
            trailsDronesDistances.Add(dc, new List<float>());
            zoomFactors.Add(dc, 0);
        }

        List<Drone> drones = SwarmMaster.instance.getAvailableDrones(true, true);
        foreach (Drone d in drones)
        {
            updateColorForDrone(d);
        }
    }

    override public void updateScenario()
    {
        //Check for non ready drone and clean the selection
        /*
        List<KeyValuePair<DroneController,Drone>> dronesToDeselect = new List<KeyValuePair<DroneController,Drone>>();
        foreach (var ds in droneSelection) // Check all controllers for selected drones
        {
            foreach (var d in ds.Value) if (d.state != Drone.DroneState.Ready) dronesToDeselect.Add(new KeyValuePair<DroneController,Drone>(ds.Key,d));
        }

        foreach (KeyValuePair<DroneController,Drone> ds in dronesToDeselect) deselectDrone(ds.Key, ds.Value);
        */

        List<Drone> flyingDrones = SwarmMaster.instance.getAvailableDrones(true, false);
        foreach (var d in flyingDrones)
        {
            Vector3 lookVec = Quaternion.LookRotation(lookAtTarget.position - d.transform.position).eulerAngles;
            d.yawTo(lookVec.y, 0);
        }

        foreach (var ds in droneSelection) // Check all controllers for selected drones
        {
            DroneController dc = ds.Key;
            if (dc == null) return;

            if (!isMovingDrones.ContainsKey(dc)) isMovingDrones.Add(dc, false);

            if (isMovingDrones[dc])
            {
                List<Drone> drones = ds.Value;
                if (drones.Count == 0) continue; //If no drone in selection, stop there for this controller

                if (trails[dc] == null) //No trails : moving with fixed constraint
                {
                    foreach (var d in drones)
                    {
                        if (droneManipulationOffsets[dc].ContainsKey(d))
                        {
                            droneManipulationOffsets[dc][d] *= 1 + (zoomFactors[dc] * zoomSpeed) * Time.deltaTime; //

                            Vector3 target = dc.transform.TransformPoint(droneManipulationOffsets[dc][d]);
                            //Debug.Log("Moving drone :"+d.droneName+" > "+target);
                            d.moveToPosition(target);
                        }

                    }
                }
                else //moving with trail mode
                {
                    Drone headDrone = drones[drones.Count - 1];
                    Trail trail = trails[dc];
                    Vector3 target = dc.transform.TransformPoint(droneManipulationOffsets[dc][headDrone]);
                    headDrone.moveToPosition(target);
                    trail.samples.Add(target);

                    //Debug.Log("Distances count : " +trailsDronesDistances[dc].Count);

                    //first drone in list is last drone in trail
                    for (int i = 0; i < drones.Count - 1; i++)
                    {
                        float targetDist = autoDistance ? trailsDronesDistances[dc][i] : trailDroneDistance * (drones.Count-1-i);
                        Vector3 targetPos = trail.getPosAtDistanceFromStart(targetDist);

                        //Debug.Log("Drone move with dist " + drones[i].droneName + " : " + targetDist + " > " + targetPos);
                        Debug.DrawLine(drones[i].transform.position, targetPos+Vector3.up,Color.Lerp(Color.yellow, Color.blue, targetDist/trail.getTotalDistance()));
                        //Debug.DrawLine(targetPos+Vector3.down, targetPos+Vector3.up, Color.Lerp(Color.yellow, Color.blue, targetDist / trail.getTotalDistance()));

                        drones[i].moveToPosition(targetPos, 0);
                    }

                    //Vector3 debugPos = trail.getPosAtDistanceFromStart(debugDist);
                    //Debug.DrawLine(debugPos + Vector3.down, debugPos + Vector3.up, Color.Lerp(Color.yellow, Color.blue, debugDist / trail.getTotalDistance()));
                }

            }
        }

        foreach (var c in controllers)
        {
            if (c.buttonStates[SELECTION] && selectionMode == SelectionMode.Paint)
            {
                selectOverDrone(c);
            }
        }
    }

    public override void endScenario()
    {
        base.endScenario();

        clearLoops();
        clearSelection();
        //foreach (DroneController dc in controllers) setMovingDrones(dc, false);
    }



    // CONTROLLER INTERACTION

    public override void triggerShortPress(DroneController dc, int buttonID, DroneController.ButtonState state)
    {
        //Debug.Log("Trigger Short press : " + state);

        if (buttonID == MOVE && state == DroneController.ButtonState.Down) //Palm facing upwards
        {
            Debug.Log("Launch All Selected (Both controllers)");
            List<Drone> dronesToLaunch = new List<Drone>();
            if (droneSelection[dc].Count == 0) //if launching controller has no selected drone, launch all drones from any controller
            {
                foreach (KeyValuePair<DroneController, List<Drone>> ddSel in droneSelection)
                {
                    for (int i = 0; i < ddSel.Value.Count; i++) dronesToLaunch.Add(ddSel.Value[i]);
                    {

                    }
                }
            }
            else //only launch drones from launching controller
            {
                foreach (Drone d in droneSelection[dc]) dronesToLaunch.Add(d);
            }

            relPitch = (dc.pitch - minTakeOffPitch) / (maxTakeOffPitch - minTakeOffPitch);
            takeOffHeight = Mathf.Lerp(minTakeOffHeight, maxTakeOffHeight, relPitch);

            for (int i = 0; i < dronesToLaunch.Count; i++)
            {
                StartCoroutine(launchDelayed(dronesToLaunch[i], i * 1, takeOffHeight));

            }

            return;
        }


        switch (buttonID)
        {
            case SELECTION:
                if (state == DroneController.ButtonState.Down) //Palm facing upwards
                {
                    //Debug.Log("Select All !");
                    clearSelection();
                    selectAllDrones(dc);
                }
                break;

            case MOVE: // Moving
            case TRAIL:
                if (trails != null) trails[dc] = null;

                Debug.Log("Trail button, drone selection :" + (droneSelection != null));
                if (droneSelection != null)
                {
                    if (!droneSelection.ContainsKey(dc) || droneSelection[dc].Count == 0)
                    {
                        Debug.Log("No drone in selection !");
                        return; // no selection, or empty selection
                    }

                    Debug.Log("Short press, buttonID = " + buttonID + " > " + (buttonID == TRAIL));
                    if (buttonID == TRAIL)
                    {
                        setMovingDrones(dc, true, true);
                        setupTrail(dc);
                    }
                    else
                    {
                        setMovingDrones(dc, !isMovingDrones[dc], false); //Switch moving drones

                    }


                }

                break;

            case CLEAR:
                clearLoopsAndSelection();
                break;

            case SELECTION_CLEAR:
                clearSelection();
                break;

            case LOOP:
                if(state == DroneController.ButtonState.Down)
                {
                    Debug.Log("Clear loops !");
                    MrTrackerClient.instance.sendMultiVibrate(dc.id, 2, .4f, 3, .1f);
                    clearLoops();
                } else
                {
                    if (isRecordingDroneLoop != null && !isRecordingDroneLoop[dc])
                    {
                        if (isMovingDrones[dc])
                        {
                            isRecordingDroneLoop[dc] = true;

                            List<Drone> dronesToDeselect = new List<Drone>();

                            foreach (var d in droneSelection[dc])
                            {
                                if (loops.ContainsKey(d))
                                {
                                    dronesToDeselect.Add(d);
                                    continue;
                                    //Destroy(loops[d]);
                                }

                                DroneLoop l = gameObject.AddComponent<DroneLoop>();
                                l.setup(d);
                                if (!loops.ContainsKey(d))
                                {
                                    loops.Add(d, l);
                                   
                                }

                                Debug.Log("Start record");
                                l.startRecord();
                                updateColorForDrone(d);
                            }

                            foreach (Drone d in dronesToDeselect) deselectDrone(dc, d);


                            MrTrackerClient.instance.sendVibrate(dc.id, .6f, .5f);
                        }
                        else
                        {
                            clearLastLoopGroup(dc);

                            MrTrackerClient.instance.sendVibrate(dc.id, .4f, .5f);
                        }

                    }
                    else
                    {
                        isRecordingDroneLoop[dc] = false;
                        //setMovingDrones(dc, false);

                        List<DroneLoop> loopGroup = new List<DroneLoop>();

                        if (droneSelection.ContainsKey(dc))
                        {
                            foreach (var d in droneSelection[dc])
                            {
                                loops[d].stopRecord();
                                loops[d].play();
                                loopGroup.Add(loops[d]);

                                //updateColorForDrone(d);
                            }
                        }

                        orderedLoopGroups[dc].Add(loopGroup);
                        clearSelection(dc);

                        MrTrackerClient.instance.sendVibrate(dc.id, .6f, .3f);
                    }
                }
                

                break;
        }
    }

   

    public override void triggerLongPress(DroneController controller, int buttonID, DroneController.ButtonState state)
    {
        if (buttonID == MOVE && state == DroneController.ButtonState.Down) //Palm facing upwards
        {
            Debug.Log("Launch All Selected Synchro (Both controllers)");

            List<Drone> dronesToLaunch = new List<Drone>();
            if (droneSelection[controller].Count == 0) //if launching controller has no selected drone, launch all drones from any controller
            {
                foreach (KeyValuePair<DroneController, List<Drone>> ddSel in droneSelection)
                {
                    for (int i = 0; i < ddSel.Value.Count; i++) dronesToLaunch.Add(ddSel.Value[i]);
                    {

                    }
                }
            }
            else //only launch drones from launching controller
            {
                foreach (Drone d in droneSelection[controller]) dronesToLaunch.Add(d);
            }

            relPitch = Mathf.Clamp01((controller.pitch - minTakeOffPitch) / (maxTakeOffPitch - minTakeOffPitch));
            takeOffHeight = Mathf.Lerp(minTakeOffHeight, maxTakeOffHeight, relPitch);

            foreach (Drone d in dronesToLaunch)
            {
                d.launch(takeOffHeight);
            }

            return;
        }

        switch (buttonID)
        {
            case CLEAR:
                MrTrackerClient.instance.sendVibrate(controller.id, .4f, 2);
                clearAndLand();
                break;

            case 4:
                if (state == DroneController.ButtonState.Up)
                {
                    if (droneSelection != null) landDrones(controller);
                    // clearSelection(controller);

                }
                break;
            //Zoom + / 1
            case MOVE:
            case LOOP:
                if (isMovingDrones[controller]) zoomFactors[controller] = buttonID == MOVE ? -1 : 1;
                MrTrackerClient.instance.sendVibrate(controller.id, .4f, .1f);
                break;
        }
    }





    public override void buttonStateUpdate(DroneController dc, int buttonID, bool value, DroneController.ButtonState state)
    {
        switch (buttonID)
        {
            case SELECTION: // Selection
                switch (selectionMode)
                {
                    case SelectionMode.Touch:
                        if (value) selectOverDrone(dc, true);
                        break;

                    case SelectionMode.Paint:
                        if (value)
                        {
                            if (clearSelectionOnTouch)
                            {
                                Debug.Log("Selection Touch paint, clearSelection");
                                clearSelection(dc);
                            }

                        }
                        break;
                }
                break;



            case MOVE:
            case LOOP:
                if (!value)
                {
                    zoomFactors[dc] = 0; //reset zoom factor
                    foreach (KeyValuePair<Drone, Vector3> dm in droneManipulationOffsets[dc])
                    {
                        //droneManipulationOffsets[dc][dm.Key] = dc.transform.InverseTransformPoint(dm.Key.transform.position); //reset manipOffset so the drone stays where it is when stopping zooming
                    }
                }
                break;


        }
    }

    public override void overDroneUpdate(DroneController dc, Drone d)
    {
        //Debug.Log("Over Drone Update ! " +dc.id);
        if (d.isOver)
        {
            MrTrackerClient.instance.sendVibrate(dc.id, .5f, .03f);
        }

        updateColorForDrone(d);
    }

    //Launch
    IEnumerator launchDelayed(Drone d, float time, float targetHeight = 1.5f)
    {
        yield return new WaitForSeconds(time);
        d.launch(targetHeight);
    }

    //SELECTION
    void clearSelection(DroneController dc = null)
    {
        if (droneSelection == null) return;
        if (dc == null)
        {
            Debug.Log("Clear all controller selections");
            foreach (var dl in droneSelection)
            {
                clearSelection(dl.Key);
            }
            return;
        }


        Debug.Log("Clear Selection for controller : " + dc.name);
        setMovingDrones(dc, false);

        Drone[] drones = droneSelection[dc].ToArray(); //Keep a list before clearing the selection, then update the color
        droneSelection[dc].Clear();

        //Debug.Log("Deselected " + drones.Length + " drones");
        foreach (var d in drones)
        {

            updateColorForDrone(d);
        }

        MrTrackerClient.instance.sendMultiVibrate(dc.id, 3, .2f, .3f, .1f);
    }

    void clearLoops(DroneController dc = null)
    {
        Debug.Log("Clear loop " + dc);
        List<Drone> drones = new List<Drone>();
        if (loops != null)
        {
            foreach (var dl in loops)
            {
                //Debug.Log("Clearing loop for " + dl.Key.droneName + " ?");
                if (dc != null && !droneSelection[dc].Contains(dl.Key)) continue; //if controller is not null, only remove loops from this controller
                //Debug.Log("Ok, clearing");
                drones.Add(dl.Key);
                Destroy(dl.Value);
            }

        }

        foreach (Drone d in drones)
        {
            loops.Remove(d);
            updateColorForDrone(d);
        }

        if(dc != null) orderedLoopGroups[dc] = new List<List<DroneLoop>>();
        else
        {
            if (orderedLoopGroups != null)
            {
                foreach (var dlg in orderedLoopGroups)
                {
                    orderedLoopGroups[dlg.Key].Clear();
                }
            }
        }
    }

    private void clearLastLoopGroup(DroneController dc)
    {
        if (orderedLoopGroups[dc].Count == 0) return;
        List<DroneLoop> loopGroup = orderedLoopGroups[dc][orderedLoopGroups[dc].Count - 1];

        List<Drone> dronesToRemoveLoop = new List<Drone>();
        foreach(var dl in loops)
        {
            if(loopGroup.Contains(dl.Value))
            {
                dronesToRemoveLoop.Add(dl.Key);
                Destroy(dl.Value);
            }
        }

        foreach(Drone d in dronesToRemoveLoop)
        {
            loops.Remove(d);
            updateColorForDrone(d);
        }

        orderedLoopGroups[dc].RemoveAt(orderedLoopGroups[dc].Count - 1);
    }



    void clearMovingDrones()
    {
        clearSelection();
        foreach (var ds in isMovingDrones) isMovingDrones[ds.Key] = false;
    }

    void selectOverDrone(DroneController dc, bool toggleSelection = false, bool takeOverSelectionIfAlreadySelected = true)
    {
        Drone d = null;
        //
        if (SwarmMaster.instance.overDrones.TryGetValue(dc, out d)) //if a drone is over
        {
            if (droneSelection[dc].Contains(d) && toggleSelection)
            {
                deselectDrone(dc, d);
                MrTrackerClient.instance.sendMultiVibrate(dc.id, 3, .2f, .5f, .1f);
            }
            else
            {
                if (takeOverSelectionIfAlreadySelected)
                {
                    foreach (KeyValuePair<DroneController, List<Drone>> dcDList in droneSelection)
                    {
                        if (dcDList.Key != dc && dcDList.Value.Contains(d)) droneSelection[dcDList.Key].Remove(d);
                    }
                }

                selectDrone(dc, d);
                MrTrackerClient.instance.sendMultiVibrate(dc.id, 2, .3f, .5f, .2f);
            }
        }
    }

    [OSCMethod]
    public void selectAllDrones(DroneController dc)
    {
        Debug.Log("Select all drones");
        List<Drone> drones = SwarmMaster.instance.getZOrderedAvailableDrones(true, true);
        for (int i = 0; i < drones.Count; i++) selectDrone(dc, drones[i]);// StartCoroutine(selectDroneDelayed(dc, drones[i], .1f * i));
    }

    IEnumerator selectDroneDelayed(DroneController dc, Drone d, float time)
    {
        yield return new WaitForSeconds(time);
        selectDrone(dc, d);
    }

    void selectDrone(DroneController dc, Drone d, bool autoDeselectFlyingDronesIfOnGround = true)
    {
        //Debug.Log("Select drone");
        //bypass lock
        /*
        if (d.isLocked() && (Object)d.locker != this)
        {
            Debug.LogWarning("Other locker ! " + d.locker.ToString());
            Debug.LogWarning(" >  "+((SwarmScenario)d.locker).scenarioName);
            return;
        }
        */
        //Debug.Log("Select for real");

        if (!droneSelection[dc].Contains(d))
        {
            if (!d.isFlying() && autoDeselectFlyingDronesIfOnGround)
            {
                List<Drone> ddList = new List<Drone>();
                foreach (Drone dd in droneSelection[dc]) if (dd.isFlying()) ddList.Add(dd);
                foreach (Drone dd in ddList) deselectDrone(dc, dd);
            }

            droneSelection[dc].Add(d);
            //d.blink(3, .2f, .5f);
            updateColorForDrone(d);
        }
    }

    [OSCMethod]
    void deselectDrone(DroneController dc, Drone d)
    {
        //bypass lock

        //if (d.isLocked() && (Object)d.locker != this) return;

        if (droneSelection[dc].Contains(d))
        {
            droneSelection[dc].Remove(d);
            d.setHeadlight(false);
            updateColorForDrone(d);
        }
    }



    void setMovingDrones(DroneController dc, bool value, bool trailMode = false)
    {
        if (!isMovingDrones.ContainsKey(dc)) isMovingDrones.Add(dc, false);
        if (isMovingDrones[dc] == value) return;
        isRecordingDroneLoop[dc] = false;
        trails[dc] = null;

        if (droneSelection[dc].Count == 0)
        {
            Debug.Log("# No drone to move, not doing anything");
            return;
        }

        isMovingDrones[dc] = value;

        MrTrackerClient.instance.sendVibrate(dc.id, .2f, value ? .5f : .1f);

        if (value)
        {
            droneManipulationOffsets[dc].Clear();

            List<Drone> selectedDrones = droneSelection[dc];
            foreach (var d in selectedDrones)
            {
                droneManipulationOffsets[dc].Add(d, dc.transform.InverseTransformPoint(d.transform.position));
                updateColorForDrone(d);
                lockDrone(d);
                //d.blink(2, .4f, .5f);
            }
        }
        else
        {
            List<Drone> selectedDrones = droneSelection[dc];
            foreach (var d in selectedDrones)
            {
                updateColorForDrone(d);
                releaseDrone(d);
            }
        }
    }

    void setupTrail(DroneController dc)
    {
        if (droneSelection[dc].Count == 0)
        {
            Debug.Log("No drone in selection !");
            return;
        }

        trails[dc] = new Trail();
        List<Drone> drones = droneSelection[dc];
        Drone headDrone = drones[drones.Count - 1];
        trails[dc].headDrone = headDrone;
        float[] distances = new float[drones.Count];


        float curDist = 0;

        Debug.Log("Setup trail with " + drones.Count + "(head : " + headDrone.droneName + ")");

        for (int i = 0; i < drones.Count; i++)
        {
            trails[dc].samples.Add(drones[i].transform.position);
        }

        for (int i = drones.Count - 1; i >= 0; i--) //from head to tail
        {

            if (i == drones.Count - 1) distances[i] = 0; //Add 0 dist for head drone
            else
            {
                float dist = Vector3.Distance(drones[i].transform.position, drones[i + 1].transform.position);
                curDist += Mathf.Max(dist, minAutoDroneDistance);
                distances[i] = curDist;
                Debug.Log("[" + i + "] Drone " + drones[i].droneName + " dist : " + distances[i]);
            }

            updateColorForDrone(drones[i]);
        }

        trailsDronesDistances[dc] = new List<float>(distances);

        MrTrackerClient.instance.sendMultiVibrate(dc.id, 2, .5f, 3f, .3f);
    }


    public void clearLoopsAndSelection(DroneController dc = null)
    {
        //setMovingDrones(dc, false);
        //List<Drone> drones = SwarmMaster.instance.getAvailableDrones(true,true);

        clearLoops(dc);
        clearSelection(dc);
    }

    public void landDrones(DroneController dc)
    {
        foreach (Drone d in droneSelection[dc])
        {
            //updateColorForDrone(d);
            d.land();
        }
    }

    public void clearAndLand()
    {
        //foreach (DroneController dc in controllers) setMovingDrones(dc, false);
        clearLoops();
        clearSelection();

        List<Drone> drones = SwarmMaster.instance.getAvailableDrones(true, true);
        foreach (Drone d in drones)
        {
            //updateColorForDrone(d);
            d.land();
        }
    }


    //STATE UPDATE

    private void droneStateUpdate(Drone d)
    {
        updateColorForDrone(d);
    }

    private void updateColorForDrone(Drone d)
    {
        if (d == null) return;

        Color c = d.isFlying() ? idleGroundColor : idleGroundColor;

        //Selection / moving / trail check
        foreach (var ds in droneSelection)
        {
            if (ds.Value.Contains(d))
            {
                if (isMovingDrones[ds.Key])
                {
                    if (trails[ds.Key] != null) c = trails[ds.Key].headDrone == d ? trailHeadColor : trailColor;
                    else c = movingColor;
                }
                else
                {
                    c = selectedColor;
                }
                break;
            }
        }

        foreach (var l in loops)
        {
            if (l.Key == d)
            {
                if (l.Value.isRecording) c = loopingRecordingColor;
                else c = loopingColor;
                break;
            }
        }


        d.setHeadlight(d.isOver);
        d.colorTo(c, 0);
    }



    //DEBUG AND GIZMOS
    private void OnDrawGizmos()
    {
        if (trails == null) return;
        foreach (var ts in trails)
        {
            if (ts.Value == null) continue;
            for (int i = 0; i < ts.Value.samples.Count; i++)
            {
                Gizmos.color = Color.Lerp(Color.yellow,Color.blue,i*1.0f/(ts.Value.samples.Count-1));
                Gizmos.DrawWireCube(ts.Value.samples[i], Vector3.one * .05f);
                if (i > 0) Gizmos.DrawLine(ts.Value.samples[i], ts.Value.samples[i - 1]);
            }
        }

        foreach(var ds in droneSelection)
        {
            Gizmos.color = isMovingDrones[ds.Key] ? Color.cyan : Color.yellow;
            foreach(Drone d in ds.Value)
            {
                Gizmos.DrawWireSphere(d.transform.position + Vector3.up * .2f, .1f);
            }
        }
    }
}


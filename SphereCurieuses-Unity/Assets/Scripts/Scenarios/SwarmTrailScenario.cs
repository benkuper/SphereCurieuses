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

    public Drone.LightMode idleLightMode;
    public Drone.LightMode selectedLightMode;

    [Header("Trail")]
    public bool autoDistance;
    public float trailDroneDistance;
    public float minAutoDroneDistance;

    [Header("Zoom")]
    public float zoomSpeed;

    DroneController[] controllers;
    Dictionary<Drone, DroneLoop> loops;
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
        Debug.Log("Start scenario !");
        loops = new Dictionary<Drone, DroneLoop>();

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
        List<KeyValuePair<DroneController,Drone>> dronesToDeselect = new List<KeyValuePair<DroneController,Drone>>();
        foreach (var ds in droneSelection) // Check all controllers for selected drones
        {
            foreach (var d in ds.Value) if (d.state != Drone.DroneState.Ready) dronesToDeselect.Add(new KeyValuePair<DroneController,Drone>(ds.Key,d));
        }
        foreach (KeyValuePair<DroneController,Drone> ds in dronesToDeselect) deselectDrone(ds.Key, ds.Value);




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
                        droneManipulationOffsets[dc][d] *= 1 + (zoomFactors[dc] * zoomSpeed) * Time.deltaTime; //

                        Vector3 target = dc.transform.TransformPoint(droneManipulationOffsets[dc][d]);
                        //Debug.Log("Moving drone :"+d.droneName+" > "+target);
                        d.moveToPosition(target);
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
                    for (int i = 0; i < drones.Count - 1; i++)
                    {
                        float targetDist = autoDistance ? trailsDronesDistances[dc][i] : trailDroneDistance * (drones.Count - (i + 1));
                        Vector3 targetPos = trail.getPosAtDistanceFromEnd(targetDist);
                        //Debug.Log("Drone move with dist " + drones[i].droneName + " : " + targetDist);
                        drones[i].moveToPosition(targetPos, 0);
                    }
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
            foreach(KeyValuePair<DroneController, List<Drone>> ddSel in droneSelection)
            {
                for (int i = 0; i < ddSel.Value.Count; i++)
                {
                    ddSel.Value[i].Invoke("launch", i * 1f);
                }
            }
            
            return;
        }


        switch (buttonID)
        {
            case SELECTION:
                if (state == DroneController.ButtonState.Down) //Palm facing upwards
                {
                    Debug.Log("Select All !");
                    clearSelection();
                    List<Drone> dList = SwarmMaster.instance.getZOrderedAvailableDrones(true, true);

                    foreach (Drone d in dList)
                    {
                        Debug.Log("Select ordered : " + d.droneName);
                        selectDrone(dc, d);
                    }
                }
                break;

            case MOVE: // Moving
            case TRAIL:
                trails[dc] = null;

                if (!droneSelection.ContainsKey(dc) || droneSelection[dc].Count == 0) return; // no selection, or empty selection

                setMovingDrones(dc, !isMovingDrones[dc], buttonID == TRAIL); //Switch moving drones

                if (isMovingDrones[dc] && buttonID == TRAIL) setupTrail(dc); //if trail button setup trail

                break;

            case CLEAR:
                clearLoopsAndSelection();
                break;

            case SELECTION_CLEAR:
                clearSelection();
                break;

            case LOOP:
                if (!isRecordingDroneLoop[dc])
                {
                    if (isMovingDrones[dc])
                    {
                        isRecordingDroneLoop[dc] = true;
                        foreach (var d in droneSelection[dc])
                        {
                            if (loops.ContainsKey(d)) Destroy(loops[d]);

                            DroneLoop l = gameObject.AddComponent<DroneLoop>();
                            l.setup(d);
                            loops.Add(d, l);
                            l.startRecord();
                            updateColorForDrone(d);
                        }

                        MrTrackerClient.instance.sendVibrate(dc.id, .6f, .5f);
                    }
                    else
                    {
                        List<Drone> dronesToSelect = new List<Drone>();
                        foreach (KeyValuePair<Drone, DroneLoop> dl in loops) dronesToSelect.Add(dl.Key);
                        clearLoopsAndSelection();
                        foreach (Drone d in dronesToSelect) selectDrone(dc, d);
                        MrTrackerClient.instance.sendVibrate(dc.id, .4f, .5f);
                    }
                    
                }
                else
                {
                    isRecordingDroneLoop[dc] = false;
                    //setMovingDrones(dc, false);
                    if (droneSelection.ContainsKey(dc))
                    {
                        foreach (var d in droneSelection[dc])
                        {
                            loops[d].stopRecord();
                            loops[d].play();
                            //updateColorForDrone(d);
                        }
                    }

                    clearSelection();

                    MrTrackerClient.instance.sendVibrate(dc.id, .6f, .3f);
                }

                break;
        }
    }

    public override void triggerLongPress(DroneController controller, int buttonID, DroneController.ButtonState state)
    {
        switch (buttonID)
        {
            case CLEAR:
                MrTrackerClient.instance.sendVibrate(controller.id, .4f, 2);
                clearAndStopDrones();
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
                        if (value) selectOverDrone(dc,true);
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
                if (!value) zoomFactors[dc] = 0; //reset zoomfactor

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


    //SELECTION
    void clearSelection()
    {
        //Debug.Log("Clear selection");
        foreach (var dl in droneSelection)
        {
            clearSelection(dl.Key);
        }

        

    }

    void clearSelection(DroneController dc)
    {
        //Debug.Log("Clear Selection for controller : " + dc.name);
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

    void clearLoops()
    {
        List<Drone> drones = new List<Drone>();
        if (loops != null)
        {
            foreach (var dl in loops)
            {
                drones.Add(dl.Key);
                Destroy(dl.Value);

            }
        }
        loops.Clear();
        foreach (Drone d in drones) updateColorForDrone(d);
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

    public void selectAllDrones()
    {
        List<Drone> drones = SwarmMaster.instance.getAvailableDrones(true, true);
        DroneController dc = controllers[0];
        foreach (Drone d in drones) selectDrone(dc, d);
    }

    void selectDrone(DroneController dc, Drone d, bool autoDeselectFlyingDronesIfOnGround = true)
    {
        if (d.isLocked() && (Object)d.locker != this) return;
        if (!droneSelection[dc].Contains(d))
        {
            if(!d.isFlying() && autoDeselectFlyingDronesIfOnGround)
            {
                List<Drone> ddList = new List<Drone>();
                foreach (Drone dd in droneSelection[dc]) if (dd.isFlying()) ddList.Add(dd);
                foreach (Drone dd in ddList) deselectDrone(dc, dd);
            }

            droneSelection[dc].Add(d);
            d.headLight = true;
            updateColorForDrone(d);
        }
    }

    void deselectDrone(DroneController dc, Drone d)
    {
        if (d.isLocked() && (Object)d.locker != this) return;
        if (droneSelection[dc].Contains(d))
        {
            droneSelection[dc].Remove(d);
            d.headLight = false;
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
            }
        }
        else
        {
            List<Drone> selectedDrones = droneSelection[dc];
            foreach (var d in selectedDrones)
            {
                updateColorForDrone(d);
                if ((Object)d.locker == this) releaseDrone(d);
            }
        }
    }

    void setupTrail(DroneController dc)
    {

        trails[dc] = new Trail();
        List<Drone> drones = droneSelection[dc];
        Drone headDrone = drones[drones.Count - 1];
        trails[dc].headDrone = headDrone;
        float[] distances = new float[drones.Count];


        float curDist = 0;
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
            }

            updateColorForDrone(drones[i]);
        }

        trailsDronesDistances[dc] = new List<float>(distances);
    }



    //CLEARING
    public void clearLoopsAndSelection()
    {
        //setMovingDrones(dc, false);
        clearLoops();
        clearSelection();
    }

    

    public void clearAndStopDrones()
    {
        //foreach (DroneController dc in controllers) setMovingDrones(dc, false);
        clearLoops();
        clearSelection();

        List<Drone> drones = SwarmMaster.instance.getAvailableDrones(true, true);
        foreach (Drone d in drones)
        {
            //updateColorForDrone(d);
            d.stop();
        }
    }


    //STATE UPDATE

    private void droneStateUpdate(Drone d)
    {
        updateColorForDrone(d);
    }

    private void updateColorForDrone(Drone d)
    {
        Color c = d.isFlying() ? idleGroundColor : idleGroundColor;
        Drone.LightMode mode = idleLightMode;

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
                    mode = selectedLightMode;
                    c = selectedColor;
                }
                break;
            }
        }

        foreach (var l in loops)
        {
            if (l.Key == d)
            {
                mode = idleLightMode;
                if (l.Value.isRecording) c = loopingRecordingColor;
                else c = loopingColor;
                break;
            }
        }


        d.headLight = d.isOver;
        d.lightMode = mode;
        d.colorTo(c, .1f);
    }



    //DEBUG AND GIZMOS
    private void OnDrawGizmos()
    {
        if (trails == null) return;
        foreach (var ts in trails)
        {
            if (ts.Value == null) continue;
            Gizmos.color = Color.yellow;
            for (int i = 0; i < ts.Value.samples.Count; i++)
            {
                Gizmos.DrawCube(ts.Value.samples[i], Vector3.one * .01f);
                if (i > 0) Gizmos.DrawLine(ts.Value.samples[i], ts.Value.samples[i - 1]);
            }
        }
    }
}


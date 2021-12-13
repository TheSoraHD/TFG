using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;


public class SteamVRDriver : AvatarDriver
{
    // Global constants
    private static float HEAD_COSINE_DEVIATION_THRESHOLD = 0.5f;
    private static float MAX_HEAD_TO_WAIST_DISTANCE = 0.8f;
    private static uint maxDeviceCount = System.Math.Min(OpenVR.k_unMaxTrackedDeviceCount, 16);

    // Aliases GameObjects for SteamVR devices
    private GameObject BaseStation1;
    private GameObject BaseStation2;
    private GameObject HMD;
    private GameObject ControllerLeft;
    private GameObject ControllerRight;
    private GameObject TrackerRoot;
    private GameObject TrackerLeft;
    private GameObject TrackerRight;

    // Devices axes
    private GameObject BaseStation1Axes;
    private GameObject BaseStation2Axes;
    private GameObject HMDAxes;
    private GameObject ControllerLeftAxes;
    private GameObject ControllerRightAxes;
    private GameObject TrackerRootAxes;
    private GameObject TrackerLeftAxes;
    private GameObject TrackerRightAxes;

    // Devices tracking indices
    private int BaseStation1Index;
    private int BaseStation2Index;
    private int ControllerLeftIndex;
    private int ControllerRightIndex;
    private int TrackerRootIndex;
    private int TrackerLeftIndex;
    private int TrackerRightIndex;

    // How many devices detected
    private uint numConnectedControllers;
    private uint numConnectedTrackers;

    // Constructor
    public SteamVRDriver(GameObject obj) : base(obj)
    {
        type = AvatarDriver.AvatarDriverType.SteamVR;

        // Get references to GameObjects
        Transform base1 = Utils.FindDescendants(obj.transform, "Base Station (lighthouse1)");
        if (base1 != null)
        {
            BaseStation1 = base1.gameObject;
        }
        Transform base2 = Utils.FindDescendants(obj.transform, "Base Station (lighthouse2)");
        if (base2 != null)
        {
            BaseStation2 = base2.gameObject;
        }
        HMD = head;
        ControllerLeft = handLeft;
        ControllerRight = handRight;
        TrackerRoot = pelvis;
        TrackerLeft = footLeft;
        TrackerRight = footRight;

#if UNITY_EDITOR
        // Create a layer for everything that should not be seen from the HMD
        LayerUtils.CreateLayer("NotHMD");
        if (HMD)
        {
            LayerUtils.HideLayerInCamera("NotHMD", HMD.GetComponentInChildren<Camera>());
        }

        // Create a layer for everything that should only be seen from the HMD
        LayerUtils.CreateLayer("OnlyHMD");
        if (HMD)
        {
            Camera[] activeCameras = Camera.allCameras; // FIXME: should loop over all cameras, not just active ones
            foreach (Camera cam in activeCameras)
            {
                if (cam != HMD.GetComponentInChildren<Camera>())
                {
                    LayerUtils.HideLayerInCamera("OnlyHMD", cam);
                }
            }
        }
#endif

        // No axes yet
        BaseStation1Axes = null;
        BaseStation2Axes = null;
        HMDAxes = null;
        ControllerLeftAxes = null;
        ControllerRightAxes = null;
        TrackerRootAxes = null;
        TrackerLeftAxes = null;
        TrackerRightAxes = null;

        // No device indices yet
        BaseStation1Index = -1;
        BaseStation2Index = -1;
        ControllerLeftIndex = -1;
        ControllerRightIndex = -1;
        TrackerRootIndex = -1;
        TrackerLeftIndex = -1;
        TrackerRightIndex = -1;

        // No device detected yet
        numConnectedControllers = 0;
        numConnectedTrackers = 0;

        // Create devices models
        setupDevicesModels();

        // TODO: mask sphere to hide head from user
        // for now, disable it, as it is not working
        if (HMD)
        {
            if (Utils.FindDescendants(HMD.transform, "MaskHead"))
            {
                Utils.FindDescendants(HMD.transform, "MaskHead").gameObject.SetActive(false);
            }
        }

        // Devices have not been identified
        ready = false;
    }

    private void clearIndices()
    {
        BaseStation1Index = -1;
        BaseStation2Index = -1;
        ControllerLeftIndex = -1;
        ControllerRightIndex = -1;
        TrackerRootIndex = -1;
        TrackerLeftIndex = -1;
        TrackerRightIndex = -1;
    }

    // Devices
    private void setupDevicesModels()
    {
        // TODO: remove any child GameObject with name "Model"
        // Add models from file ourselves (this ensures models will be present,
        // and removes possible conflits with SteamVR_RenderModel)

#if UNITY_EDITOR
        // Hide HMD model from HMD
        if (HMD)
        {
            if (HMD.transform.Find("Model"))
            {
                LayerUtils.MoveToLayer(HMD.transform.Find("Model").gameObject, "NotHMD");
            }
        }
#endif

        // Colour devices
        colourDevices();
    }

    // Colour devices according to stickers
    public void colourDevices()
    {
        if (OpenVR.RenderModels != null)
        {
            GameObject baseStation1Model = BaseStation1.transform.Find("Model").gameObject;
            if (baseStation1Model)
            {
                if (baseStation1Model.GetComponentInChildren<MeshRenderer>())
                {
                    baseStation1Model.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
                }
            }
            GameObject baseStation2Model = BaseStation2.transform.Find("Model").gameObject;            
            if (baseStation2Model)
            {
                if (baseStation2Model.GetComponentInChildren<MeshRenderer>())
                {
                    baseStation2Model.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
                }
            }
            GameObject cameraHeadModel = HMD.transform.Find("Model").gameObject;
            if (cameraHeadModel)
            {
                if (cameraHeadModel.GetComponentInChildren<MeshRenderer>())
                {
                    cameraHeadModel.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
                }
            }
            GameObject controllerLeftModel = ControllerLeft.transform.Find("Model").gameObject;
            if (controllerLeftModel)
            {
                if (controllerLeftModel.GetComponentInChildren<MeshRenderer>())
                {
                    controllerLeftModel.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                }
            }
            GameObject controllerRightModel = ControllerRight.transform.Find("Model").gameObject;
            if (controllerRightModel)
            {
                if (controllerRightModel.GetComponentInChildren<MeshRenderer>())
                {
                    controllerRightModel.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.magenta;
                }
            }
            GameObject trackerRootModel = TrackerRoot.transform.Find("Model").gameObject;
            if (trackerRootModel)
            {
                if (trackerRootModel.GetComponentInChildren<MeshRenderer>())
                {
                    trackerRootModel.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                }
            }
            GameObject trackerLeftModel = TrackerLeft.transform.Find("Model").gameObject;
            if (trackerLeftModel)
            {
                if (trackerLeftModel.GetComponentInChildren<MeshRenderer>())
                {
                    trackerLeftModel.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
                }
            }
            GameObject trackerRightModel = TrackerRight.transform.Find("Model").gameObject;
            if (trackerRightModel)
            {
                if (trackerRightModel.GetComponentInChildren<MeshRenderer>())
                {
                    trackerRightModel.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
                }
            }
        }
    }

    // Assigns device indices and enables/disables GameObjects accordingly
    public bool setDevicesIndex()
    {
        if (BaseStation1Index > 0) 
        {
            BaseStation1.SetActive(true);
            BaseStation1.GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex) BaseStation1Index;
        }
        else
        {
            BaseStation1.SetActive(false);
            BaseStation1.GetComponent<SteamVR_TrackedObject>().index = SteamVR_TrackedObject.EIndex.None;
        }

        if (BaseStation2Index > 0) 
        {
            BaseStation2.SetActive(true);
            BaseStation2.GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex) BaseStation2Index;
        }
        else
        {
            BaseStation2.SetActive(false);
            BaseStation2.GetComponent<SteamVR_TrackedObject>().index = SteamVR_TrackedObject.EIndex.None;
        }

        if (ControllerLeftIndex > 0) 
        {
            ControllerLeft.SetActive(true);
            ControllerLeft.GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex) ControllerLeftIndex;
        }
        else
        {
            ControllerLeft.SetActive(false);
            ControllerLeft.GetComponent<SteamVR_TrackedObject>().index = SteamVR_TrackedObject.EIndex.None;
        }

        if (ControllerRightIndex > 0) 
        {
            ControllerRight.SetActive(true);
            ControllerRight.GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex) ControllerRightIndex;
        }
        else
        {
            ControllerRight.SetActive(false);
            ControllerRight.GetComponent<SteamVR_TrackedObject>().index = SteamVR_TrackedObject.EIndex.None;
        }

        if (TrackerRootIndex > 0) 
        {
            TrackerRoot.SetActive(true);
            TrackerRoot.GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex) TrackerRootIndex;
        }
        else
        {
            TrackerRoot.SetActive(false);
            TrackerRoot.GetComponent<SteamVR_TrackedObject>().index = SteamVR_TrackedObject.EIndex.None;
        }

        if (TrackerLeftIndex > 0) 
        {
            TrackerLeft.SetActive(true);
            TrackerLeft.GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex) TrackerLeftIndex;
        }
        else
        {
            TrackerLeft.SetActive(false);
            TrackerLeft.GetComponent<SteamVR_TrackedObject>().index = SteamVR_TrackedObject.EIndex.None;
        }

        if (TrackerRightIndex > 0) 
        {
            TrackerRight.SetActive(true);
            TrackerRight.GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex) TrackerRightIndex;
        }
        else
        {
            TrackerRight.SetActive(false);
            TrackerRight.GetComponent<SteamVR_TrackedObject>().index = SteamVR_TrackedObject.EIndex.None;
        }

        return true;
    }

    // Finds connected devices and sets temporal device indices
    public bool detectDevices(DisplayMirror displayMirror)
    {
        // Init global vars
        numConnectedControllers = 0;
        numConnectedTrackers = 0;

        // Reset indices
        clearIndices();

        // Get pose relative to the safe bounds defined by the user
        TrackedDevicePose_t[] trackedDevicePoses = new TrackedDevicePose_t[maxDeviceCount];
        if (OpenVR.Settings != null)
        {
            OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, trackedDevicePoses);
        }

        // Loop over connected devices
        bool oneBase = false;
        bool oneController = false;
        bool oneTracker = false;
        bool twoTrackers = false;
        for (uint i = 0; i < maxDeviceCount; ++i)
        {
            // deviceClass sometimes returns the wrong class for a device, hence we use a string
            /*ETrackedDeviceClass deviceClass = ETrackedDeviceClass.Invalid;
            if (OpenVR.Settings != null)
            {
                deviceClass = OpenVR.System.GetTrackedDeviceClass(i);
            }*/
            ETrackingResult status = trackedDevicePoses[i].eTrackingResult;
            var result = new System.Text.StringBuilder((int)64);
            var error = ETrackedPropertyError.TrackedProp_Success;
            if (OpenVR.System != null)
            {
                OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_RenderModelName_String, result, 64, ref error);
            }
            //Debug.Log(i + " " + result.ToString() + " - " + deviceClass + " - status: " + status);

            // Handle base stations
            if (result.ToString().Contains("basestation") && status == ETrackingResult.Running_OK)
            //if (deviceClass == ETrackedDeviceClass.TrackingReference && status == ETrackingResult.Running_OK)
            {
                if (!oneBase)
                {
                    BaseStation1Index = (int) i;
                    oneBase = true;
                } 
                else 
                {
                    BaseStation2Index = (int) i;
                }
            }

            // Handle HMD
            else if (result.ToString().Contains("hmd") && status == ETrackingResult.Running_OK)
            //else if (deviceClass == ETrackedDeviceClass.HMD && status == ETrackingResult.Running_OK)
            {
                continue;
            }

            // Handle controllers
            else if (result.ToString().Contains("controller") && status == ETrackingResult.Running_OK)
            //else if (deviceClass == ETrackedDeviceClass.Controller && status == ETrackingResult.Running_OK)
            {
                if (!oneController) 
                {
                    ControllerLeftIndex = (int) i;
                    oneController = true;
                }
                else
                {
                    ControllerRightIndex = (int) i;
                }
                numConnectedControllers++;
            }

            // Handle trackers
            else if (result.ToString().Contains("tracker") && status == ETrackingResult.Running_OK)
            //else if (deviceClass == ETrackedDeviceClass.GenericTracker && status == ETrackingResult.Running_OK)
            {
                if (!oneTracker) 
                {
                    TrackerRootIndex = (int) i;
                    oneTracker = true;
                }
                else if (!twoTrackers) 
                {
                    TrackerLeftIndex = (int) i;
                    twoTrackers = true;
                }
                else
                {
                    TrackerRightIndex = (int)i;
                }
                numConnectedTrackers++;
            }
        }

        string message = string.Format(PipelineUtils.progressMessageAt(PipelineUtils.Stage.DIRTY), numConnectedControllers, numConnectedTrackers);
        if (numConnectedControllers + numConnectedTrackers >= 2 && numConnectedControllers > 0)
        {
            string message2 = PipelineUtils.introMessageAt(PipelineUtils.Stage.DEVICES);
            //Debug.Log(message + "\n");
            //Debug.Log(message2 + "\n");
            if (displayMirror)
            {
                displayMirror.ShowTextAgain(message, new Color(1.0f, 1.0f, 1.0f, 0.5f), 2, message2, new Color(1.0f, 1.0f, 1.0f, 0.5f), 0, true);
            }
        }
        else
        {
            message = message + " " + PipelineUtils.failureMessageAt(PipelineUtils.Stage.DIRTY);
            //Debug.Log(message + "\n");
            if (displayMirror)
            {
                displayMirror.ShowText(message, new Color(1.0f, 0.0f, 0.0f, 0.5f), 0, true);
            }
        }

        // Not done
        ready = false;

        // Asign correct indices
        return setDevicesIndex();
    }

    // Fixes indices of tracked devices
    public bool identifyDevices(DisplayMirror displayMirror)
    {
        displayMirror.avatarVR.singleInput.blockControllers(true);

        // HACK: Record baseStation transforms
        Utils.RecordBaseStations(BaseStation1, BaseStation2);

        if (numConnectedControllers + numConnectedTrackers < 2)
        {
            string message = PipelineUtils.failureMessageAt(PipelineUtils.Stage.DEVICES, 0);
            Debug.Log(message + "\n");
            if (displayMirror)
            {
                displayMirror.ShowText(message, new Color (1.0f, 0.0f, 0.0f, 0.5f), 2, true);
            }
            //ViveInput.blockControllers(false); // no need, displayPanel will unblock them
            return false;
        }

        uint numPoints = 1 + numConnectedControllers + numConnectedTrackers;
        Vector3[] points = new Vector3[numPoints];
        int[] deviceIndices = new int[numPoints];
        //  points[0] = HMD position                deviceIndices[0] = HMD index     
        //  points[1] = Controller 1 position       deviceIndices[1] = Controller 1 index     
        //  points[2] = Controller 2 position       deviceIndices[2] = Controller 2 index     
        // ...                                      ...
        //  points[n] = Controller n position       deviceIndices[n] = Controller n index     
        //  points[n+1] = Tracker 1 position        deviceIndices[n+1] = Tracker 1 index     
        //  points[n+2] = Tracker 2 position        deviceIndices[n+2] = Tracker 2 index     
        //  points[n+3] = Tracker 3 position        deviceIndices[n+3] = Tracker 3 index     
        // ...                                      ...
        uint controllerIndex0 = 1;
        uint trackerIndex0 = controllerIndex0 + numConnectedControllers;

        uint controllerIndex = controllerIndex0;
        uint trackerIndex = trackerIndex0;

        if (HMD.activeInHierarchy)
        {
            points[0] = HMD.transform.position;
            deviceIndices[0] = (int) SteamVR_TrackedObject.EIndex.Hmd;
        }

        if (ControllerLeft.activeInHierarchy)
        {
            points[controllerIndex] = ControllerLeft.transform.position;
            deviceIndices[controllerIndex] = (int) ControllerLeft.GetComponent<SteamVR_TrackedObject>().index;
            controllerIndex++;
        }
        if (ControllerRight.activeInHierarchy)
        {
            points[controllerIndex] = ControllerRight.transform.position;
            deviceIndices[controllerIndex] = (int) ControllerRight.GetComponent<SteamVR_TrackedObject>().index;
            controllerIndex++;
        }
        Debug.Assert(controllerIndex == numConnectedControllers + 1);

        if (TrackerRoot.activeInHierarchy)
        {
            points[trackerIndex] = TrackerRoot.transform.position;
            deviceIndices[trackerIndex] = (int) TrackerRoot.GetComponent<SteamVR_TrackedObject>().index;
            trackerIndex++;
        }
        if (TrackerLeft.activeInHierarchy)
        {
            points[trackerIndex] = TrackerLeft.transform.position;
            deviceIndices[trackerIndex] = (int) TrackerLeft.GetComponent<SteamVR_TrackedObject>().index;
            trackerIndex++;
        }
        if (TrackerRight.activeInHierarchy)
        {
            points[trackerIndex] = TrackerRight.transform.position;
            deviceIndices[trackerIndex] = (int) TrackerRight.GetComponent<SteamVR_TrackedObject>().index;
            trackerIndex++;
        }
        Debug.Assert(trackerIndex == numConnectedControllers + numConnectedTrackers + 1);

        // Fit plane to tracked objects locations
        float a = 0.0f, b = 0.0f, c = 0.0f, d = 0.0f;
        bool res = Utils.FitPlane(numPoints, points, ref a, ref b, ref c, ref d);
        if (!res)
        {            
            string message = PipelineUtils.failureMessageAt(PipelineUtils.Stage.DEVICES, 1);
            Debug.Log(message + "\n");
            if (displayMirror)
            {
                displayMirror.ShowText(message, new Color (1.0f, 0.0f, 0.0f, 0.5f), 2, true);
            }
            //ViveInput.blockControllers(false); // no need, displayPanel will unblock them
            return false;
        }
        Vector3 n = new Vector3(a, b, c);
        n = Vector3.Normalize(n);

        // Get HMD forward vector
        Vector3 f = HMD.transform.forward;
        f = Vector3.Normalize(f);

        //  Compute deviation between plane normal and HMD forward
        float deviation = Vector3.Dot(n, f);

        // Make sure plane points in the same direction 
        if (System.Math.Abs(deviation) < HEAD_COSINE_DEVIATION_THRESHOLD)
        {
            string message = PipelineUtils.failureMessageAt(PipelineUtils.Stage.DEVICES, 2);
            Debug.Log(message + "\n");
            if (displayMirror)
            {
                displayMirror.ShowText(message, new Color (1.0f, 0.0f, 0.0f, 0.5f), 2, true);
            }
            //ViveInput.blockControllers(false); // no need, displayPanel will unblock them
            return false;
        }
        if (deviation < 0.0f)
        {
            n = -1.0f * n;
        }

        // Get a point on the plane
        Vector3 p = new Vector3(0.0f, 0.0f, -d / c);

        // Project points on plane
        Vector3[] projectedPoints = new Vector3[numPoints];
        for (uint i = 0; i < numPoints; ++i)
        {
            Vector3 t = points[i] - p;
            float dist = Vector3.Dot(t, n);
            projectedPoints[i] = points[i] - dist * n;
        }

        // Build u,v coordinate system
        Vector3 v = Vector3.up;
        Vector3 u = Vector3.Cross(v, n);
        float u0 = Vector3.Dot(projectedPoints[0], u); // HMD
        float v0 = Vector3.Dot(projectedPoints[0], v);

        // Get uv coordinates
        Vector2[] planePoints = new Vector2[numPoints];
        planePoints[0] = new Vector2(0.0f, 0.0f); // HMD will be origin of uv space
        for (uint i = 1; i < numPoints; ++i)
        {
            float u_coord = Vector3.Dot(projectedPoints[i], u) - u0;
            float v_coord = Vector3.Dot(projectedPoints[i], v) - v0;
            Vector2 uv = new Vector2(u_coord, v_coord);
            planePoints[i] = uv;
        }

        // Reset indices
        ControllerLeftIndex = ControllerRightIndex = -1;
        TrackerRootIndex = TrackerLeftIndex = TrackerRightIndex = -1;

        // Identify controllers/trackers according to uv coordinates
        for (uint i = 0; i < numConnectedControllers; ++i)
        {
            if (planePoints[controllerIndex0+i].x < 0.0f)
            {
                ControllerLeftIndex = (int) deviceIndices[controllerIndex0+i];
            }
            else
            {
                ControllerRightIndex = (int) deviceIndices[controllerIndex0+i];
            }
        }
        for (uint i = 0; i < numConnectedTrackers; ++i)
        {
            if (System.Math.Abs(planePoints[trackerIndex0+i].y) < MAX_HEAD_TO_WAIST_DISTANCE)
            {
                TrackerRootIndex = (int) deviceIndices[trackerIndex0+i];
            }  
            else if (planePoints[trackerIndex0+i].x < 0.0f)
            {
                TrackerLeftIndex = (int) deviceIndices[trackerIndex0+i];
            }  
            else
            {
                TrackerRightIndex = (int) deviceIndices[trackerIndex0+i];
            }
        }
                
        // Asign correct indices
        ready = true;
        displayMirror.avatarVR.singleInput.blockControllers(false);
        displayMirror.CleanText();
        return setDevicesIndex();
    }   

    // Show/hide rendering of the tracking references
    public void showTrackingReferences(bool flag)
    {
        if (BaseStation1)
        {
            BaseStation1.transform.Find("Model").gameObject.SetActive(flag);
        }
        if (BaseStation2)
        {
            BaseStation2.transform.Find("Model").gameObject.SetActive(flag);
        }
    }

    // Show/hide rendering of the tracked devices
    public void showTrackedDevices(bool flag)
    {
        if (HMD)
        {
            HMD.transform.Find("Model").gameObject.SetActive(flag);
        }
        if (ControllerLeft)
        {
            ControllerLeft.transform.Find("Model").gameObject.SetActive(flag);
        }
        if (ControllerRight)
        {
            ControllerRight.transform.Find("Model").gameObject.SetActive(flag);
        }
        if (TrackerRoot)
        {
            TrackerRoot.transform.Find("Model").gameObject.SetActive(flag);
        }
        if (TrackerLeft)
        {
            TrackerLeft.transform.Find("Model").gameObject.SetActive(flag);
        }
        if (TrackerRight)
        {
            TrackerRight.transform.Find("Model").gameObject.SetActive(flag);
        }
    }

    // Show/hide rendering of the tracking references axes
    public void showTrackingReferencesAxes(bool flag)
    {
        if (BaseStation1Axes)
        {
            BaseStation1Axes.SetActive(flag);
        }
        if (BaseStation2Axes)
        {
            BaseStation2Axes.SetActive(flag);
        }
    }

    // Show/hide rendering of the tracked devices axes
    public void showTrackedDevicesAxes(bool flag)
    {
        if (HMDAxes)
        {
            HMDAxes.SetActive(flag);
        }
        if (ControllerLeftAxes)
        {
            ControllerLeftAxes.SetActive(flag);
        }
        if (ControllerRightAxes)
        {
            ControllerRightAxes.SetActive(flag);
        }
        if (TrackerRootAxes)
        {
            TrackerRootAxes.SetActive(flag);
        }
        if (TrackerLeftAxes)
        {
            TrackerLeftAxes.SetActive(flag);
        }
        if (TrackerRightAxes)
        {
            TrackerRightAxes.SetActive(flag);
        }
    }
}

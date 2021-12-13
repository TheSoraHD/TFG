using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ContinuousInput : MonoBehaviour
{
    private AvatarVR avatarVR;

    // Used for capturing points
    public static float CONVERGE_POS_THRESHOLD = 0.02f;
    private static int POINTS_STEP_CONVERGE = 5;
    private static uint CAPTURE_NUM_POINTS_MAX = 2048;
    public bool capturing = false;
    public uint captureIndex = 0;
    public Vector3[] capturedPoints = new Vector3[CAPTURE_NUM_POINTS_MAX];
    public float[] capturedPointsTime = new float[CAPTURE_NUM_POINTS_MAX];

    private GameObject capturingGameObject = null;
    private PipelineUtils.Stage capturingJoint = PipelineUtils.Stage.WRISTS;
    private float capturingThreshold = 0.001f;
    private bool captureFirstMessage = false;
    private int numberPointsUntilTestConverge;
    private Vector3 lastPoint;
    private bool captureSinglePoint;
    private uint capturingNumPoints = 0;
    private bool tipFirst;

    // Second Capturing
    public bool capturingSecond = false;
    public uint captureIndexSecond = 0;
    private GameObject capturingGameObjectSecond = null;
    private float capturingThresholdSecond = 0.001f;
    public Vector3[] capturedPointsSecond = new Vector3[CAPTURE_NUM_POINTS_MAX];
    public float[] capturedPointsTimeSecond = new float[CAPTURE_NUM_POINTS_MAX];
    private int numberPointsUntilTestConvergeSecond;
    private Vector3 lastPointSecond;
    private bool captureSinglePointSecond;
    private uint capturingNumPointsSecond = 0;

    // Rotation Point
    private static float MAX_HEADSET_DEPTH = 0.35f; // meters
    private static float MAX_HEADSET_HALF_WIDTH = 0.15f; // meters
    private bool capturingRotationPoint = false;
    private Vector3[,] initPositions;
    private float[,] maxDistance;
    private float capturingRotationPointStep;
    private GameObject capturingRotationPointDriver;

    void Start()
    {
        avatarVR = transform.parent.GetComponent<AvatarVR>();
    }

    // Only used for caputuring devices location
    void Update()
    {
        if (capturing || capturingSecond)
        {
            storeDevicesLocation();
        }
        if (capturingRotationPoint)
        {
            updateRotationPoint();
        }
    }

    // Used for capturing tracked device locations

    public void captureDevicesLocation(GameObject gO, SteamVR_Controller.Device device, PipelineUtils.Stage joint, uint numPoints, float threshold, bool captureMessage = true, float convergeTh = 0.001f, bool tip = false)
    {
        if (gO == null)
        {
            return;
        }

        capturing = true;

        captureIndexSecond = captureIndex = 0;
        lastPoint = new Vector3(float.MinValue, 0.0f, 0.0f);
        numberPointsUntilTestConverge = POINTS_STEP_CONVERGE;

        CONVERGE_POS_THRESHOLD = convergeTh;

        captureSinglePoint = false;
        capturingGameObject = gO;
        capturingNumPoints = numPoints;
        capturingJoint = joint;
        capturingThreshold = threshold * threshold;
        captureFirstMessage = captureMessage;

        capturingSecond = false;

        tipFirst = tip;
    }

    // This should be called AFTER captureDevicesLocation(...)
    public void captureSecondDevicesLocation(GameObject gO, SteamVR_Controller.Device device, uint numPoints, float threshold, bool tip = false) 
    {
        if (gO == null)
        {
            return;
        }

        capturingSecond = true;

        captureIndexSecond = 0;
        lastPointSecond = new Vector3(float.MinValue, 0.0f, 0.0f);
        numberPointsUntilTestConvergeSecond = POINTS_STEP_CONVERGE;

        captureSinglePointSecond = false;
        capturingGameObjectSecond = gO;
        capturingNumPointsSecond = numPoints;
        capturingThresholdSecond = threshold * threshold;
    }

    private void storeDevicesLocation()
    {
        bool done = false;
        bool doneSecond = false;

        //string message = string.Format(PipelineUtils.progressMessageAt(capturingJoint), (capturingSecond ? Mathf.Min(((float)captureIndex / (float)capturingNumPoints) * 100.0f, ((float)captureIndexSecond / (float)capturingNumPointsSecond) * 100.0f) : ((float)captureIndex / (float)capturingNumPoints) * 100.0f));
        string message = string.Format(PipelineUtils.progressMessageAt(capturingJoint), captureIndexSecond + captureIndex);
        //Debug.Log(message + "\n");
        if (avatarVR.displayMirror)
        {
            avatarVR.displayMirror.ShowText(message, new Color(1.0f, 1.0f, 1.0f, 0.5f), 0, false, captureFirstMessage);
            captureFirstMessage = false;
        }

        // Update FIRST Device
        if (capturing)
        {
            if (captureIndex == 0)
            {
                capturedPoints[captureIndex % (capturingNumPoints * 2)] = getPointFirst();
                capturedPointsTime[captureIndex % (capturingNumPoints * 2)] = Time.time;
                captureIndex++;
                done = captureSinglePoint; // True if Instant Position, false otherwhise
                numberPointsUntilTestConverge -= 1;
            }
            else if (numberPointsUntilTestConverge > 0)
            {
                bool found_closer = false;
                Vector3 newPos = getPointFirst();
                for (uint i = 0; i < Mathf.Min(captureIndex, (capturingNumPoints * 2)) && !found_closer; ++i)
                {
                    Vector3 pos = capturedPoints[i];
                    float dist2 = (pos - newPos).sqrMagnitude;
                    if (dist2 < capturingThreshold)
                    {
                        found_closer = true;
                    }
                }
                if (!found_closer)
                {
                    capturedPoints[captureIndex % (capturingNumPoints * 2)] = newPos;
                    capturedPointsTime[captureIndex % (capturingNumPoints * 2)] = Time.time;
                    captureIndex++;
                    numberPointsUntilTestConverge -= 1;
                }
            }
            else // Test position convergence
            {
                float radius = 0.0f;
                Vector3 pos = new Vector3();
                float quality = 0.0f;
                Utils.FitSphere((uint)Mathf.Min(captureIndex, (capturingNumPoints * 2)), capturedPoints, ref radius, ref pos, ref quality);
                if (lastPoint.x != float.MinValue)
                {
                    float diff = Vector3.Distance(pos, lastPoint);
                    done = diff < CONVERGE_POS_THRESHOLD && captureIndex >= capturingNumPoints;
                }
                lastPoint = pos;
                numberPointsUntilTestConverge = POINTS_STEP_CONVERGE;
            }
        }

        // Update SECOND Device
        if (capturingSecond)
        {
            if (captureIndexSecond == 0)
            {
                capturedPointsSecond[captureIndexSecond % (capturingNumPointsSecond * 2)] = getPointSecond();
                capturedPointsTimeSecond[captureIndexSecond % (capturingNumPointsSecond * 2)] = Time.time;
                captureIndexSecond++;
                doneSecond = captureSinglePointSecond; // True if Instant Position, false otherwhise
                numberPointsUntilTestConvergeSecond -= 1;
            }
            else if (numberPointsUntilTestConvergeSecond > 0)
            {
                bool found_closer = false;
                Vector3 newPos = getPointSecond();
                for (uint i = 0; i < Mathf.Min(captureIndexSecond, (capturingNumPointsSecond * 2)) && !found_closer; ++i)
                {
                    Vector3 pos = capturedPointsSecond[i];
                    float dist2 = (pos - newPos).sqrMagnitude;
                    if (dist2 < capturingThresholdSecond)
                    {
                        found_closer = true;
                    }
                }
                if (!found_closer)
                {
                    capturedPointsSecond[captureIndexSecond % (capturingNumPointsSecond * 2)] = newPos;
                    capturedPointsTimeSecond[captureIndexSecond % (capturingNumPointsSecond * 2)] = Time.time;
                    captureIndexSecond++;
                    numberPointsUntilTestConvergeSecond -= 1;
                }
            }
            else // Test position convergence
            {
                float radius = 0.0f;
                Vector3 pos = new Vector3();
                float quality = 0.0f;
                Utils.FitSphere((uint)Mathf.Min(captureIndexSecond, (capturingNumPointsSecond * 2)), capturedPointsSecond, ref radius, ref pos, ref quality);
                if (lastPointSecond.x != float.MinValue)
                {
                    float diff = Vector3.Distance(pos, lastPointSecond);
                    doneSecond = diff < CONVERGE_POS_THRESHOLD && captureIndexSecond >= capturingNumPointsSecond;
                }
                lastPointSecond = pos;
                numberPointsUntilTestConvergeSecond = POINTS_STEP_CONVERGE;
            }
        }

        // done FIRST device
        if (done)
        {
            capturingRotationPoint = capturing = false;
        }

        // done SECOND device
        if (doneSecond)
        {
            capturingSecond = false;
        }
    }

    private Vector3 getPointFirst()
    {
        if (tipFirst) return capturingGameObject.transform.position - capturingGameObject.transform.forward * 0.175f;
        else return capturingGameObject.transform.position;
    }

    private Vector3 getPointSecond()
    {
        if (tipFirst) return capturingGameObjectSecond.transform.position - capturingGameObjectSecond.transform.forward * 0.175f;
        else return capturingGameObjectSecond.transform.position;
    }

    public void captureInstantPosition(GameObject gO, SteamVR_Controller.Device device, PipelineUtils.Stage joint)
    {
        if (gO == null)
        {
            return;
        }
        capturing = true;

        captureIndex = 0;

        capturingGameObject = gO;
        captureSinglePoint = true;
        capturingJoint = joint;
        capturingNumPoints = 1;
    }

    public Vector3 getInstantPosition()
    {
        Debug.Assert(!capturing, "getInstantPosition() cannot be called until ContinousInput has finished capturing points");
        return capturedPoints[0];
    }

    public void captureRotationPointHead(GameObject driver, float step) // this must be called after a fitting spheres, and it will finish at the same time
    {

        capturingRotationPoint = true;

        capturingRotationPointStep = step;
        capturingRotationPointDriver = driver;

        Vector3 vDepth = driver.transform.forward;
        vDepth.Normalize(); // it should already be normalized... but just in case
        Vector3 vWidth = driver.transform.right;
        vWidth.Normalize();

        int sizeDepth = Mathf.FloorToInt(MAX_HEADSET_DEPTH / step);
        int sizeWidth = Mathf.FloorToInt((2.0f*MAX_HEADSET_HALF_WIDTH) / step);

        initPositions = new Vector3[sizeDepth, sizeWidth];
        maxDistance = new float[sizeDepth, sizeWidth];

        for (int i = 0; i < sizeDepth; ++i)
        {
            float d = i * (-step);
            for (int j = 0; j < sizeWidth; ++j)
            {
                float w = (j * step) - MAX_HEADSET_HALF_WIDTH;
                Vector3 offset = driver.transform.position + w * vWidth + d * vDepth;
                initPositions[i, j] = offset;
            }
        }
    }

    private void updateRotationPoint()
    {
        Vector3 vDepth = capturingRotationPointDriver.transform.forward;
        vDepth.Normalize(); // it should already be normalized... but just in case
        Vector3 vWidth = capturingRotationPointDriver.transform.right;
        vWidth.Normalize();

        int sizeDepth = Mathf.FloorToInt(MAX_HEADSET_DEPTH / capturingRotationPointStep);
        int sizeWidth = Mathf.FloorToInt((2.0f * MAX_HEADSET_HALF_WIDTH) / capturingRotationPointStep);

        for (int i = 0; i < sizeDepth; ++i)
        {
            float d = i * (-capturingRotationPointStep);
            for (int j = 0; j < sizeWidth; ++j)
            {
                float w = (j * capturingRotationPointStep) - MAX_HEADSET_HALF_WIDTH;
                Vector3 offset = capturingRotationPointDriver.transform.position + w * vWidth + d * vDepth;
                float newDistance = (offset - initPositions[i, j]).sqrMagnitude;
                if (newDistance > maxDistance[i, j])
                {
                    maxDistance[i, j] = newDistance;
                }
            }
        }
    }

    public Vector2 getRotationPointHead()
    {
        int sizeDepth = Mathf.FloorToInt(MAX_HEADSET_DEPTH / capturingRotationPointStep);
        int sizeWidth = Mathf.FloorToInt((2.0f * MAX_HEADSET_HALF_WIDTH) / capturingRotationPointStep);

        int minD = 0;
        int minW = 0;
        float min = float.MaxValue;

        for (int i = 0; i < sizeDepth; ++i)
        {
            for (int j = 0; j < sizeWidth; ++j)
            {
                if (maxDistance[i, j] < min)
                {
                    min = maxDistance[i, j];
                    minD = i;
                    minW = j;
                }
            }
        }

        return new Vector2(minD * (-capturingRotationPointStep), (minW * capturingRotationPointStep) - MAX_HEADSET_HALF_WIDTH);
    }
}
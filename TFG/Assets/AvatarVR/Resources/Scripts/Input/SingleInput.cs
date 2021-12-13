using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using UnityEditor;
using System.Linq;
using System.IO;

public class SingleInput : MonoBehaviour
{
    private AvatarVR avatarVR;

    // SteamVR controllers
    private SteamVR_Controller.Device SteamVRControllerLeft;
    private SteamVR_Controller.Device SteamVRControllerRight;
    private SteamVR_Controller.Device SteamVRTrackerRight; // Right Foot
    private SteamVR_Controller.Device SteamVRTrackerLeft; // Left Foot
    private SteamVR_Controller.Device SteamVRTrackerRoot;

    // Whether the SteamVR controllers have been assigned
    private bool controllersStarted = false;

    // Whether the controllers events should not be processed
    private bool applicationBusy = false;
    private bool controllerRecent = false;

    // Stage in the Avatar Setup pipeline
    public PipelineUtils.Stage stage = PipelineUtils.Stage.DEVICES;
    private bool succeeded = false; // whether current stage is completed

    void Start()
    {
        avatarVR = transform.parent.GetComponent<AvatarVR>();
    }

    // Start controllers: for now, we use SteamVR (old) Input System
    private void StartControllers()
    {
        SteamVRControllerLeft = null;
        SteamVRControllerRight = null;
        SteamVRTrackerRight = null;
        SteamVRTrackerLeft = null;
        SteamVRTrackerRoot = null;
        if (avatarVR.driver.handLeft.activeInHierarchy)
        {
            if (avatarVR.driver.handLeft.GetComponent<SteamVR_TrackedObject>())
            {
                if (avatarVR.driver.handLeft.GetComponent<SteamVR_TrackedObject>().index != SteamVR_TrackedObject.EIndex.None)
                {
                    SteamVRControllerLeft = SteamVR_Controller.Input((int)avatarVR.driver.handLeft.GetComponent<SteamVR_TrackedObject>().index);
                    avatarVR.driver.handLeftDevice = SteamVRControllerLeft;
                }
            }
            controllersStarted = true;
        }
        if (avatarVR.driver.handRight.activeInHierarchy)
        {
            if (avatarVR.driver.handRight.GetComponent<SteamVR_TrackedObject>())
            {
                if (avatarVR.driver.handRight.GetComponent<SteamVR_TrackedObject>().index != SteamVR_TrackedObject.EIndex.None)
                {
                    SteamVRControllerRight = SteamVR_Controller.Input((int)avatarVR.driver.handRight.GetComponent<SteamVR_TrackedObject>().index);
                    avatarVR.driver.handRightDevice = SteamVRControllerRight;
                }
            }
            controllersStarted = true;
        }
        if (avatarVR.driver.footRight.activeInHierarchy)
        {
            if (avatarVR.driver.footRight.GetComponent<SteamVR_TrackedObject>())
            {
                if (avatarVR.driver.footRight.GetComponent<SteamVR_TrackedObject>().index != SteamVR_TrackedObject.EIndex.None)
                {
                    SteamVRTrackerRight = SteamVR_Controller.Input((int)avatarVR.driver.footRight.GetComponent<SteamVR_TrackedObject>().index);
                    avatarVR.driver.footRightDevice = SteamVRTrackerRight;
                }
            }
        }
        if (avatarVR.driver.footLeft.activeInHierarchy)
        {
            if (avatarVR.driver.footLeft.GetComponent<SteamVR_TrackedObject>())
            {
                if (avatarVR.driver.footLeft.GetComponent<SteamVR_TrackedObject>().index != SteamVR_TrackedObject.EIndex.None)
                {
                    SteamVRTrackerLeft = SteamVR_Controller.Input((int)avatarVR.driver.footLeft.GetComponent<SteamVR_TrackedObject>().index);
                    avatarVR.driver.footLeftDevice = SteamVRTrackerLeft;
                }
            }
        }
        if (avatarVR.driver.pelvis.activeInHierarchy)
        {
            if (avatarVR.driver.pelvis.GetComponent<SteamVR_TrackedObject>())
            {
                if (avatarVR.driver.pelvis.GetComponent<SteamVR_TrackedObject>().index != SteamVR_TrackedObject.EIndex.None)
                {
                    SteamVRTrackerRoot = SteamVR_Controller.Input((int)avatarVR.driver.pelvis.GetComponent<SteamVR_TrackedObject>().index);
                    avatarVR.driver.pelvisDevice = SteamVRTrackerRoot;
                }
            }
        }
    }

    /*
        Pipeline STAGES:
          - DIRTY   -> Initial stage (This STAGE is NEVER set... it is only used for messaging)
          - DEVICES -> Devices are identified and correctly positioned (DEVICES and T_POSE are done at the same time)
          - T_POSE  -> Some static measurements: eyesHeight, rootHeight, trackterToHMD, handToHand, shoulderCenter, Pos_Ankle and Rot_Ankle (both)
          - ANKLES  -> NOT USED. Improve foot size by triangulation (feet in V (Penguin) pose)
          - ROOT    -> NOT USED
          - LEGS    -> NOT USED
          - SHOULDERS      -> Position left and right shoulder by Fitting Sphere
          - WRISTS         -> Position left and right wrist by Fitting Sphere
          - NECK           -> Position neck by Position or Fitting Sphere
          - ROOT_AVATAR    -> Avatar is constructed using Body measurements, then is placed in the scene, finally the user enters inside to compute rootUserToRootAvatar
          - DONE           -> completed = true, if enabled the measurements are saved to the disk, the avatar is loaded
    */

    private GameObject floorMarker = null;
    private int floorMarkerFlag = 0;
    private GameObject avatarRootStep;
    private bool placeLastAvatar = false;

    void Update()
    {
        // Correct loaded avatar in ROOT_AVATAR step, after 240 frame (to let the animator system to correctly place all bones)
        if (!placeLastAvatar && floorMarker != null && floorMarkerFlag++ == 250)
        {
            RaycastHit hit;
            float difference = 0.0f;
            Vector3 origin = floorMarker.transform.position;
            //Debug.DrawRay(origin, Vector3.down * 0.5f, Color.red, 10000.0f);
            //Debug.DrawRay(origin, Vector3.up * 0.5f, Color.green, 10000.0f);
            if (Physics.Raycast(origin, Vector3.down, out hit, 0.5f))
            {
                difference = hit.point.y - origin.y;
            }
            else if (Physics.Raycast(origin, Vector3.up, out hit, 0.5f))
            {
                difference = hit.point.y - origin.y;
            }

            avatarRootStep.transform.Translate(new Vector3(0.0f, difference, 0.0f), Space.World);
            floorMarker = null;
            floorMarkerFlag = 0;

            if (avatarVR.body.bodyMeasures.rootToAnkleReset)
            {
                avatarVR.body.bodyMeasures.rootToAnkleReset = false;
                // Correct sole offset
                float soleDifferenceLeft = avatarVR.animator.GetBoneTransform(HumanBodyBones.LeftFoot).position.y - avatarVR.body.bodyMeasures.footHeightLeft;
                float soleDifferenceRight = avatarVR.animator.GetBoneTransform(HumanBodyBones.RightFoot).position.y - avatarVR.body.bodyMeasures.footHeightRight;
                // Correct bodyMeasures
                avatarVR.body.bodyMeasures.rootToAnkleLeft -= soleDifferenceLeft;
                avatarVR.body.bodyMeasures.rootToAnkleRight -= soleDifferenceRight;
            }

            // Load new Avatar with bodyMeasures corrected
            avatarVR.clearAvatar();
            floorMarker = avatarVR.loadAvatarMannequin(out avatarRootStep, true);
            placeLastAvatar = true;
        }
        if (placeLastAvatar && floorMarker != null && floorMarkerFlag++ == 30)
        {
            RaycastHit hit;
            float difference = 0.0f;
            Vector3 origin = floorMarker.transform.position;
            if (Physics.Raycast(origin, Vector3.down, out hit, 0.5f))
            {
                difference = hit.point.y - origin.y;
            }
            else if (Physics.Raycast(origin, Vector3.up, out hit, 0.5f))
            {
                difference = hit.point.y - origin.y;
            }

            avatarRootStep.transform.Translate(new Vector3(0.0f, difference, 0.0f), Space.World);
            avatarVR.body.differenceFloor = difference;
            // Reset aux variables
            floorMarker = null;
            floorMarkerFlag = 0;
            placeLastAvatar = false;
        }

        // succeeded -> current stage has been completed
        if (succeeded) // Progress - no input handled here
        {
            PipelineUtils.Stage next = PipelineUtils.nextStage(avatarVR.driver, stage);
            if (!(!avatarVR.skeletonFitting && (next == PipelineUtils.Stage.SHOULDERS || next == PipelineUtils.Stage.NECK)))
            { 
                PipelineUtils.displayInBetweenStagesMessage(avatarVR, avatarVR.displayMirror, stage, next);
            }
            stage = next;
            if (stage == PipelineUtils.Stage.ROOT_AVATAR)
            {
                // after the neck has been set, we have computed all the measurements needed. Now we want to show the new rescaled avatar
                avatarVR.clearAvatar();
                floorMarker = avatarVR.loadAvatarMannequin(out avatarRootStep, true);
            }
            if (stage == PipelineUtils.Stage.DONE)
            {
                avatarVR.disableMirror();
                avatarVR.body.setCompleted(true);
                avatarVR.loadAvatar();
                avatarVR.OnCalibrationFinishedInvoke();
            }
            succeeded = false;
        }

        if (floorMarker != null)
        {
            return;
        }

        if (applicationBusy) // Busy
        {
            return;
        }

        if (controllerRecent) // Busy
        {
            return;
        }

        if (!controllersStarted) // controllersStarted -> false: STAGE is DIRTY
                                 //                       true: controllers detected but NOT identified by position
        {
            if ((avatarVR.driver as SteamVRDriver) != null) // add/change this for different drivers
            {
                (avatarVR.driver as SteamVRDriver).detectDevices(avatarVR.displayMirror);
            }
            StartControllers();
        }

        // Don't start until at least one controller is available
        if ((avatarVR.driver as SteamVRDriver) != null &&
            SteamVRControllerLeft == null && SteamVRControllerRight == null) // add/change this for different drivers
        {
            return;
        }

        if (IsGripDown() && !avatarVR.driver.isReady())
        {
            controllersStarted = false;
            return;
        }

        UpdateControllers();

        // Identify controllers, if needed
        if (IsTriggerDown() && !avatarVR.driver.isReady()) // isReady() -> false: waiting for devices to be identified
                                                           //              true: ready to obtain measures, STAGE is DEVICES
        {
            if ((avatarVR.driver as SteamVRDriver) != null) // add/change this for different drivers
            {
                (avatarVR.driver as SteamVRDriver).identifyDevices(avatarVR.displayMirror);
            }
            StartControllers();
            return;
        }

        // Start pipeline if ready
        if (avatarVR.driver.isReady() && !avatarVR.body.isStarted()) // isStarted() -> false: Pipeline still in STAGE 0 (DEVICES)
                                                                     //                true: if loadBodyMeasures then (STAGE is ROOT_AVATAR) else (STAGE is ANKLES)
        {
            stage = PipelineUtils.Stage.DEVICES;
            PipelineUtils.Stage next = PipelineUtils.nextStage(avatarVR.driver, PipelineUtils.Stage.DEVICES);
            if (avatarVR.skeletonFitting && avatarVR.driverType != AvatarDriver.AvatarDriverType.Simulation)
            {
                PipelineUtils.displayInBetweenStagesMessage(avatarVR, avatarVR.displayMirror, stage, next);
            }
            stage = next;
            if (stage == PipelineUtils.Stage.T_POSE)
            {
                StartCoroutine(avatarVR.body.computeHeightWidthMeasure(result => succeeded = result, avatarVR.driver, avatarVR.displayMirror, avatarVR));
                StartCoroutine(ControllersIdleForSecs(1));
            }
            else Debug.Assert(false, "Something went wrong. This stage should be T_Pose, but it is: " + stage.ToString());

            avatarVR.body.setStarted(true);
            return;
        }

        // Application pipeline: forward
        if (IsTriggerDown() || (stage == PipelineUtils.Stage.SHOULDERS && !avatarVR.skeletonFitting) 
            || (avatarVR.driverType == AvatarDriver.AvatarDriverType.Simulation))
        {
            ForwardPipeline(avatarVR.driver);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            avatarVR.switchActiveMirror();
        }
    }

    // Application pipeline: forward
    public void ForwardPipeline(AvatarDriver driver)
    {
        if ((!avatarVR.skeletonFitting || driver.type == AvatarDriver.AvatarDriverType.Simulation) && stage == PipelineUtils.Stage.SHOULDERS)
        {
            avatarVR.body.setupLeftShoulderJoint(driver.handLeft, avatarVR);
            avatarVR.body.setupRightShoulderJoint(driver.handRight, avatarVR);
            stage = PipelineUtils.nextStage(avatarVR.driver, stage);
        }
        if ((!avatarVR.skeletonFitting || driver.type == AvatarDriver.AvatarDriverType.Simulation) && stage == PipelineUtils.Stage.NECK)
        {
            avatarVR.body.setupNeckJoint(driver.head, avatarVR);
            succeeded = true;
        }

        if ((avatarVR.skeletonFitting && driver.type != AvatarDriver.AvatarDriverType.Simulation) && stage == PipelineUtils.Stage.SHOULDERS)
        {
            StartCoroutine(avatarVR.body.computeArmsMeasures(result =>
            {
                succeeded = result;
            }, driver, avatarVR.displayMirror, avatarVR));
            StartCoroutine(ControllersIdleForSecs(1));
        }
        else if ((avatarVR.skeletonFitting && driver.type != AvatarDriver.AvatarDriverType.Simulation) && stage == PipelineUtils.Stage.NECK)
        {
            StartCoroutine(avatarVR.body.computeHeadMeasures(result =>
            {
                succeeded = result;
            }, driver, avatarVR.displayMirror, avatarVR));
            StartCoroutine(ControllersIdleForSecs(1));
        }
        else if (stage == PipelineUtils.Stage.ROOT_AVATAR)
        {
            StartCoroutine(avatarVR.body.computeRootMeasuresUsingAvatar(result => succeeded = result, driver, avatarVR.displayMirror,
                           avatarVR.animator, avatarVR));
            StartCoroutine(ControllersIdleForSecs(1));
        }
    }

    private void UpdateControllers()
    {
        if (SteamVRTrackerRight == null || SteamVRTrackerLeft == null || SteamVRTrackerRoot == null ||
            SteamVRControllerRight == null || SteamVRControllerLeft == null) StartControllers();
    }

    // Getters / Setters

    public void setApplicationBusy(bool flag)
    {
        applicationBusy = flag;
    }

    public void setControllerRecent(bool flag)
    {
        controllerRecent = flag;
    }

    // Helper functions to block controllers input

    private IEnumerator ControllersIdleForSecs(int secs)
    {
        setControllerRecent(true);
        if (secs > 0)
        {
            yield return new WaitForSeconds(secs);
            setControllerRecent(false);
        }
    }

    public void blockControllers(bool flag)
    {
        setApplicationBusy(flag);
    }

    // Grab inputs
    public bool IsTriggerDown()
    {
        return
            (SteamVRControllerLeft != null && SteamVRControllerLeft.GetHairTriggerDown()) ||
            (SteamVRControllerRight != null && SteamVRControllerRight.GetHairTriggerDown()) ||
            (Input.GetKeyDown(KeyCode.Space));
    }

    private bool IsGripDown()
    {
        return
            (SteamVRControllerLeft != null && SteamVRControllerLeft.GetPressDown(SteamVR_Controller.ButtonMask.Grip)) ||
            (SteamVRControllerRight != null && SteamVRControllerRight.GetPressDown(SteamVR_Controller.ButtonMask.Grip)) ||
            (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.B));
    }
}
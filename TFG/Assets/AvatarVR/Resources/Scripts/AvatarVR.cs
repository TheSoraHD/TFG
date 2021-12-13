using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
using UnityEngine.Events;

public class AvatarVR : MonoBehaviour
{
    // Driver options
    public AvatarDriver.AvatarDriverType driverType = AvatarDriver.AvatarDriverType.SteamVR;
    public GameObject driverSetup;
    private GameObject head;
    private GameObject handLeft;
    private GameObject handRight;
    private GameObject pelvis;
    private GameObject footLeft;
    private GameObject footRight;

    public GameObject head_model { get; private set; }
    public GameObject handLeft_model { get; private set; }
    public GameObject handRight_model { get; private set; }
    public GameObject pelvis_model { get; private set; }
    public GameObject footLeft_model { get; private set; }
    public GameObject footRight_model { get; private set; }

    // Global options
    public enum AvatarStyle
    {
        xbot = 1,
        ybot = 2,
        Josh = 13,
        Megan = 14,
        Male_Default = 15,
        Female_Default = 20,
        Doctor = 21,
        Kate = 22,
        Worker = 23,
        Brian = 24
    }
    public AvatarStyle avatarStyle = AvatarStyle.Megan;
    [HideInInspector] public bool legsConnectedToHips = true; // use false for microsoft rocketbox library
    public Dictionary<AvatarStyle, HandController.Fingers> AvatarStyleToFingers = new Dictionary<AvatarStyle, HandController.Fingers> {
        { AvatarStyle.xbot, HandController.Fingers.Bot },
        { AvatarStyle.ybot, HandController.Fingers.Bot },
        { AvatarStyle.Josh, HandController.Fingers.Josh },
        { AvatarStyle.Megan, HandController.Fingers.Megan },
        { AvatarStyle.Male_Default, HandController.Fingers.MakeHuman },
        { AvatarStyle.Female_Default, HandController.Fingers.MakeHuman },
        { AvatarStyle.Doctor, HandController.Fingers.Josh },
        { AvatarStyle.Kate, HandController.Fingers.Kate },
        { AvatarStyle.Worker, HandController.Fingers.Josh },
        { AvatarStyle.Brian, HandController.Fingers.Josh }
    };

    public enum ControllersStyle
    {
        Show,
        ShowAttached,
        Hidden,
        Attached
    }
    public ControllersStyle controllersStyle = ControllersStyle.Attached;

    public bool skeletonFitting = false;

    public UnityEvent OnCalibrationFinished;

    public void OnCalibrationFinishedInvoke()
    {
        if (OnCalibrationFinished != null) OnCalibrationFinished.Invoke();
    }

    public void resetControllersStyle()
    {
        if (driverType == AvatarDriver.AvatarDriverType.Simulation) return;

        bool active = false;
        bool activeControllers = false;
        if (controllersStyle == ControllersStyle.Show || controllersStyle == ControllersStyle.ShowAttached || singleInput == null || singleInput.stage != PipelineUtils.Stage.DONE)
        {
            handRight_model.transform.SetParent(handRight.transform);
            handRight_model.transform.localPosition = Vector3.zero;
            handRight_model.transform.localRotation = Quaternion.identity;

            handLeft_model.transform.SetParent(handLeft.transform);
            handLeft_model.transform.localPosition = Vector3.zero;
            handLeft_model.transform.localRotation = Quaternion.identity;

            active = true;
            activeControllers = true;

            // Disable attached controllers IK
            if (controller != null && controller is AvatarController_UnityGood)
            {
                ((AvatarController_UnityGood)controller).SetControllersAttached(controllersStyle == ControllersStyle.ShowAttached);
            }
        }
        else if (controllersStyle == ControllersStyle.Hidden)
        {
            active = false;
            activeControllers = false;

            // Disable attached controllers IK
            if (controller != null && controller is AvatarController_UnityGood)
            {
                ((AvatarController_UnityGood)controller).SetControllersAttached(false);
            }
        }
        else if (controllersStyle == ControllersStyle.Attached)
        {
            Animator a = animator;
            if (controller != null && controller is AvatarController_UnityGood)
            {
                a = ((AvatarController_UnityGood)controller).animator;
                ((AvatarController_UnityGood)controller).SetControllersAttached(true);
            }

            if (a != null)
            {
                HandController.SetHands(this, a);

                active = false;
                activeControllers = true;
            }
            else
            {
                active = false;
                activeControllers = false;
            }
        }

        if (body != null && body.jointWristLeft && body.jointWristRight)
        {
            if (controllersStyle == ControllersStyle.ShowAttached)
            {
                body.jointWristLeft.transform.localPosition = -0.175f * Vector3.forward + 0.03f * Vector3.up + 0.02f * Vector3.left;
                body.jointWristRight.transform.localPosition = -0.175f * Vector3.forward + 0.03f * Vector3.up + 0.02f * Vector3.right;
            }
            else
            {
                body.jointWristLeft.transform.localPosition = -0.175f * Vector3.forward;
                body.jointWristRight.transform.localPosition = -0.175f * Vector3.forward;
            }
        }

        if (head_model != null) head_model.SetActive(active);
        if (handLeft_model != null) handLeft_model.SetActive(activeControllers);
        if (handRight_model != null) handRight_model.SetActive(activeControllers);
        if (pelvis_model != null) pelvis_model.SetActive(active);
        if (footLeft_model != null) footLeft_model.SetActive(active);
        if (footRight_model != null) footRight_model.SetActive(active);
    }

    /*********************************************************/

    // Avatar main elements
    [HideInInspector] public AvatarDriver driver;
    [HideInInspector] public AvatarBody body;
    [HideInInspector] public AvatarController controller;
    private GameObject inputSystem;
    [HideInInspector] public SingleInput singleInput;
    [HideInInspector] public ContinuousInput continuousInput;

    // Scene elements
    private GameObject origin;
    private GameObject displayMirrorObj;
    private GameObject displayMirrorObj2;
    [HideInInspector] public DisplayMirror displayMirror;
    [HideInInspector] public GameObject avatar;
    [HideInInspector] public GameObject skeleton;

    [HideInInspector] public Animator animator;

    void Start()
    {
        // Create the avatar driver
        setupDriver(driverType, driverSetup);

        // Create the abstraction of the body
        body = new AvatarBody();

        // No IK controller yet
        controller = null;

        // Create scene elements
        setupScene();

        // Hide chaperone
        VRUtils.hideChaperone();

        // Our scripts to control Vive input
        inputSystem = new GameObject("[Input System]");
        singleInput = inputSystem.AddComponent<SingleInput>();
        continuousInput = inputSystem.AddComponent<ContinuousInput>();
        inputSystem.transform.SetParent(transform);
    }

    public void disableMirror()
    {
        if (displayMirrorObj != null) displayMirrorObj.SetActive(false);
        if (displayMirrorObj2 != null) displayMirrorObj2.SetActive(false);
    }

    public void setActiveMirror2(bool enabled)
    {
        if (displayMirrorObj2 != null) displayMirrorObj2.SetActive(enabled);
        if (displayMirrorObj != null) displayMirrorObj.SetActive(!enabled);
    }

    public void switchActiveMirror()
    {
        setActiveMirror2(!displayMirrorObj2.activeSelf);
    }

    // Scene elements
    public void setupScene()
    {
        if (driverType == AvatarDriver.AvatarDriverType.Simulation) return;

        // Create and place mirror
        displayMirrorObj = GameObject.Instantiate(Resources.Load("Prefabs/DisplayMirror"), this.transform) as GameObject;
        displayMirrorObj.name = "DisplayMirror";
        displayMirrorObj.transform.localPosition = new Vector3(0.0f, 1.5f, 2.0f);
        displayMirrorObj.transform.localEulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
        displayMirrorObj.transform.localScale = new Vector3(0.25f, 0.25f, 0.1f);
        setDisplayMirrorVisible(true);
        displayMirror = displayMirrorObj.transform.Find("Mirror").GetComponent<DisplayMirror>();

        // Create and place second mirror
        displayMirrorObj2 = GameObject.Instantiate(Resources.Load("Prefabs/DisplayMirrorTop"), this.transform) as GameObject;
        displayMirrorObj2.transform.localPosition = new Vector3(0.0f, 1.5f, 2.0f);
        displayMirrorObj2.transform.localEulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
        displayMirrorObj2.transform.localScale = new Vector3(0.25f, 0.25f, 0.1f);
        Camera cameraMirror2 = displayMirrorObj2.GetComponentInChildren<Camera>();
        cameraMirror2.transform.localPosition = new Vector3(0.0f, 14.0f, 27.3f);
        cameraMirror2.transform.localEulerAngles = new Vector3(90.0f, 0.0f, 180.0f);
        cameraMirror2.transform.localScale = new Vector3(1.225f, 1.225f, 1.0f);
        setActiveMirror2(false);
    }

    // Avatar driver
    public void setupDriver(AvatarDriver.AvatarDriverType driverType, GameObject driverSetup)
    {
        // We must have an instance of a driver
        if (!driverSetup)
        {
            throw new System.NullReferenceException("Could not find a VR Setup! Make sure there is one instance in the scene, " +
                "and assign it to SetupMaster via the editor.");
        }

        // The driver must be active
        if (!driverSetup.activeInHierarchy)
        {
            driverSetup.SetActive(true);
        }

        // Create the driver object
        if (driverType == AvatarDriver.AvatarDriverType.Simulation)
        {
            driver = new SimulatorDriver(driverSetup);
            VRUtils.EnableTracking(driverSetup, false);
            driver.SetActive(true);
        }
        else if (driverType == AvatarDriver.AvatarDriverType.SteamVR)
        {
            driver = new SteamVRDriver(driverSetup);
            VRUtils.EnableTracking(driverSetup, true);
        }

        // Show references on inspector
        head = driver.head;
        handLeft = driver.handLeft;
        handRight = driver.handRight;
        pelvis = driver.pelvis;
        footLeft = driver.footLeft;
        footRight = driver.footRight;

        Transform headModelTransform = head.transform.Find("Model");
        head_model = headModelTransform == null ? null : headModelTransform.gameObject;
        Transform handLeftTransform = handLeft.transform.Find("Model");
        handLeft_model = handLeftTransform == null ? null : handLeftTransform.gameObject;
        Transform handRightTransform = handRight.transform.Find("Model");
        handRight_model = handRightTransform == null ? null : handRightTransform.gameObject;
        Transform pelvisTransform = pelvis.transform.Find("Model");
        pelvis_model = pelvisTransform == null ? null : pelvisTransform.gameObject;
        Transform footLeftTransform = footLeft.transform.Find("Model");
        footLeft_model = footLeftTransform == null ? null : footLeftTransform.gameObject;
        Transform footRightTransform = footRight.transform.Find("Model");
        footRight_model = footRightTransform == null ? null : footRightTransform.gameObject;

        resetControllersStyle();
    }

    // Avatar
    public bool loadAvatar()
    {
        if (!body.isCompleted())
        {
            return false;
        }

        clearAvatar();

        // Choose the name of the selected avatar
        string avatarName = avatarStyle.ToString();

        // Create the character
        HumanDescription humanDescription = new HumanDescription();
        AvatarUtils.ScaleSkeletonOptions scaleOptions = new AvatarUtils.ScaleSkeletonOptions(true, skeletonFitting, true, false);
        bool success = AvatarUtils.createCharacter(
            assetName: "Avatars/" + avatarName,
            skeletonFile: "Avatars/" + avatarName + "_skeleton",
            controllerFile: "Controllers/IKController",
            body: body,
            scaleSkeletonOptions: scaleOptions,
            character: out avatar,
            description: ref humanDescription,
            floorMarker: null,
            avatarVR: this
        );
        if (!success)
        {
            return false;
        }
        avatar.name = avatarName;
        avatar.transform.parent = transform;
        avatar.SetActive(true);

        // AvatarController_UnityGood
        controller = avatar.AddComponent<AvatarController_UnityGood>();
        ((AvatarController_UnityGood)controller).avatarVR = this;
        controller.driver = driver;
        controller.body = body;
        (controller as AvatarController_UnityGood).desc = humanDescription;

        setIKActive(true);

        resetControllersStyle();

        return true;
    }

    public GameObject loadAvatarMannequin(out GameObject avatarRootStep, bool scaleSkeletton = false)
    {
        avatarRootStep = null;
        // Choose the name of the selected avatar
        string avatarName = avatarStyle.ToString();
        bool success;

        float avatarEyeHeight = 0.0f, avatarHandToHand = 0.0f;
        AvatarUtils.getCharacterInfo("Avatars/" + avatarName, "Avatars/" + avatarName + "_skeleton", ref avatarEyeHeight, ref avatarHandToHand);

        HumanDescription humanDescription = new HumanDescription();
        AvatarUtils.ScaleSkeletonOptions scaleOptions;
        // Create the character

        if (scaleSkeletton)
        {
            scaleOptions = new AvatarUtils.ScaleSkeletonOptions(true, skeletonFitting, true, false);
        }
        else
        {
            scaleOptions = new AvatarUtils.ScaleSkeletonOptions();
        }

        GameObject floorMarker = new GameObject("Floor Marker");

        success = AvatarUtils.createCharacter(
            assetName: "Avatars/" + avatarName,
            skeletonFile: "Avatars/" + avatarName + "_skeleton",
            controllerFile: "",
            body: body,
            scaleSkeletonOptions: scaleOptions,
            character: out avatar,
            description: ref humanDescription,
            floorMarker: floorMarker,
            avatarVR: this
        );
        if (!success)
        {
            return null;
        }
        avatar.name = avatarName;
        avatar.transform.SetParent(transform, false);
        StartCoroutine(SetAnimationControllers());

        avatarRootStep = avatar;
        return floorMarker;
    }

    public void clearAvatar()
    {
        if (handRight_model != null)
        {
            // Reset RightHand Model before destroying the avatar
            handRight_model.transform.SetParent(handRight.transform);
            handRight_model.transform.localPosition = Vector3.zero;
            handRight_model.transform.localRotation = Quaternion.identity;
        }

        if (handLeft_model != null)
        {
            handLeft_model.transform.SetParent(handLeft.transform);
            handLeft_model.transform.localPosition = Vector3.zero;
            handLeft_model.transform.localRotation = Quaternion.identity;
        }

        if (avatar)
        {
            Destroy(avatar);
        }
        if (skeleton)
        {
            Destroy(skeleton);
        }
    }

    /************************* UI ****************************/

    private void setDisplayMirrorVisible(bool flag)
    {
        if (displayMirrorObj)
        {
            displayMirrorObj.SetActive(flag);
        }
    }

    private void setIKActive(bool flag)
    {
        if (controller != null)
        {
            controller.ikActive = flag;
        }
    }

    public Vector3 queryJointPositionAvatar(HumanBodyBones queryJoint)
    {
        Animator animator = avatar.GetComponent<Animator>();
        Transform t = animator.GetBoneTransform(queryJoint);
        return t.position;
    }


    IEnumerator SetAnimationControllers()
    {
        //Debug.Log("Before AnimationControllers");
        Animator anim = avatar.GetComponent<Animator>();

        anim.runtimeAnimatorController = Resources.Load("Avatars/EmptyCharlesContr") as RuntimeAnimatorController;
        //anim.runtimeAnimatorController = Resources.Load("Avatars/EmptyLeoContr") as RuntimeAnimatorController;

        //Wait atleast 1 frame 
        yield return new WaitForSeconds(0.1f);
        anim.runtimeAnimatorController = Resources.Load("Avatars/TPoseController") as RuntimeAnimatorController;

        //use this one to find the bodyheight possibly adjust the height back to the ground

        yield return new WaitForSeconds(0.1f);

        animator = anim;

        resetControllersStyle();
        avatar.SetActive(true);
    }
}
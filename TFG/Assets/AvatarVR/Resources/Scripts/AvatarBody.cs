using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class AvatarBody
{
    // Global constants
    private static float LOOKAT_DISTANCE_THRESHOLD = 0.3f;        // how much to move forward the lookat object from the HMD

    // Global constants: capture device locations                
    public uint CAPTURE_NUM_POINTS_HANDS = 65;                   // number of points to capture to find location of joints
    public uint CAPTURE_NUM_POINTS_HEAD = 60;
    public uint CAPTURE_NUM_POINTS_SHOULDER = 70;
    public float CAPTURE_DISTANCE_THRESHOLD_HANDS = 0.025f;       // in m: capute point only if it is at least this far from previous captured points
    public float CAPTURE_DISTANCE_THRESHOLD_HEAD = 0.015f;
    public float CAPTURE_DISTANCE_THRESHOLD_SHOULDER = 0.05f;

    // Global GameObjects for joints
    public GameObject jointCenterHead;
    public GameObject jointNeck;
    public GameObject jointWristLeft, jointWristRight;
    public GameObject jointRoot;
    public GameObject jointAnkleLeft, jointAnkleRight;
    public GameObject jointShoulderLeft, jointShoulderRight;
    public GameObject jointLegLeft, jointLegRight;
    public GameObject objLookAt;

    // Aux
    public float differenceFloor;
    public bool usingHeadJointAsNeck; //  neck joint of the user is related to  the head or neck joint of the avatar 
    public bool recordingTrackerInfo = false;
    public float nuriasMethodTh;

    // Body measures obtained from the the setup process
    [Serializable]
    public struct BodyMeasures
    {
        public float footToAnkleLeft;
        public float footToAnkleRight;
        public float footHeightLeft;
        public float footHeightRight;
        public float rootHeight;
        public float rootToAnkleLeft;
        public float rootToAnkleRight;
        public bool rootToAnkleReset;
        public float waistWidth;
        public float legSizeLeft;
        public float legSizeRight;
        public float handToShoulderLeft;
        public float handToShoulderRight;
        public float handToWristLeft;
        public float handToWristRight;
        public float handToHand;
        public Vector3 headToNeck;
        public float neckHeight;
        public float eyesHeight;
        public Vector3 shoulderCenter;
        public Vector3 shoulderRight, shoulderLeft;
        public Vector3 wristRight, wristLeft;
        public Vector3 neck;
        public float avatarAnkleHeightLeftOffset;
        public float avatarAnkleHeightRightOffset;
        public float depthCenterHead;
        public float widthCenterHead;
        public float getShoulderToShoulder()
        {
            /*float wristToShoulderLeft = handToShoulderLeft - handToWristLeft;
            float wristToShoulderRight = handToShoulderRight - handToWristRight;
            float shoulderToShoulder = handToHand - wristToShoulderLeft - wristToShoulderRight;
            Debug.Log("shoulder To shoulder by length: " + shoulderToShoulder);
            */
            float shoulderToShoulder = (shoulderRight - shoulderLeft).magnitude;
            return shoulderToShoulder;
        }
    }
    public BodyMeasures bodyMeasures;

    // Whether all the joints have been set
    public bool completed = false;

    // Whether any joint has been set
    private bool started = false;

    // Constructor
    public AvatarBody()
    {
        jointNeck = null;
        jointWristLeft = null;
        jointWristRight = null;
        jointRoot = null;
        jointAnkleLeft = null;
        jointAnkleRight = null;
        jointShoulderLeft = null;
        jointShoulderRight = null;
        jointLegLeft = null;
        jointLegRight = null;
        objLookAt = null;

        bodyMeasures.footToAnkleLeft = 0;
        bodyMeasures.footToAnkleRight = 0;
        bodyMeasures.footHeightLeft = 0;
        bodyMeasures.footHeightRight = 0;
        bodyMeasures.rootHeight = 0;
        bodyMeasures.waistWidth = 0;
        bodyMeasures.legSizeLeft = 0;
        bodyMeasures.legSizeRight = 0;
        bodyMeasures.handToShoulderLeft = 0;
        bodyMeasures.handToShoulderRight = 0;
        bodyMeasures.handToWristLeft = 0;
        bodyMeasures.handToWristRight = 0;
        bodyMeasures.handToHand = 0;
        bodyMeasures.headToNeck = Vector3.zero;
        bodyMeasures.neckHeight = 0;
        bodyMeasures.rootToAnkleReset = false;

        completed = false;
    }

    // Computing body measures

    // Step 1: height - measure taken from HMD && first aproximation to foot size using root
    public IEnumerator computeHeightWidthMeasure(System.Action<bool> success, AvatarDriver driver, DisplayMirror displayMirror, AvatarVR avatarVR)
    {
        avatarVR.singleInput.blockControllers(true);

        Vector3 headPosition = new Vector3();
        Vector3 pelvisPosition = new Vector3();

        if (driver.head && driver.head.activeInHierarchy)
        {
            avatarVR.continuousInput.captureInstantPosition(driver.head, null, PipelineUtils.Stage.T_POSE);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing);
            headPosition = avatarVR.continuousInput.getInstantPosition();
            bodyMeasures.eyesHeight = headPosition.y;
        }

        // HACK: store hand-to-hand distance now! Not in use right now
        if (driver.handLeft && driver.handLeft.activeInHierarchy && driver.handRight && driver.handRight.activeInHierarchy)
        {
            // Capture handLeft
            avatarVR.continuousInput.captureInstantPosition(driver.handLeft, driver.handLeftDevice, PipelineUtils.Stage.T_POSE);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing);
            Vector3 handLeftPosition = avatarVR.continuousInput.getInstantPosition();
            // Capture handright
            avatarVR.continuousInput.captureInstantPosition(driver.handRight, driver.handRightDevice, PipelineUtils.Stage.T_POSE);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing);
            Vector3 handRightPosition = avatarVR.continuousInput.getInstantPosition();
            // bodymeasures
            bodyMeasures.handToHand = (handLeftPosition - handRightPosition).magnitude;
            bodyMeasures.shoulderCenter = (handLeftPosition + handRightPosition) / 2;
            //Debug.Log ("ShoulderCenter at: " + bodyMeasures.shoulderCenter);
        }

        // HACK: this is the only place where we are sure the user is standing in T-pose
        // Compute the vector from the root to the neck here
        if (driver.head && driver.head.activeInHierarchy && driver.pelvis && driver.pelvis.activeInHierarchy)
        {
            // Capture pelvis
            avatarVR.continuousInput.captureInstantPosition(driver.pelvis, driver.pelvisDevice, PipelineUtils.Stage.T_POSE);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing);
            pelvisPosition = avatarVR.continuousInput.getInstantPosition();
            // bodyMeasures
            bodyMeasures.rootHeight = pelvisPosition.y;
        }

        if (driver.footLeft && driver.footLeft.activeInHierarchy &&
            driver.footRight && driver.footRight.activeInHierarchy)
        {
            // Capture footLeft
            avatarVR.continuousInput.captureInstantPosition(driver.footLeft, driver.footLeftDevice, PipelineUtils.Stage.T_POSE);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing);
            Vector3 footLeftPosition = avatarVR.continuousInput.getInstantPosition();
            bodyMeasures.footHeightLeft = footLeftPosition.y;
            // Capture footRight
            avatarVR.continuousInput.captureInstantPosition(driver.footRight, driver.footRightDevice, PipelineUtils.Stage.T_POSE);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing);
            Vector3 footRightPosition = avatarVR.continuousInput.getInstantPosition();
            bodyMeasures.footHeightRight = footRightPosition.y;

            computeFeetMeasuresUsingRoot(driver, footLeftPosition, footRightPosition, pelvisPosition, avatarVR);
        }

        avatarVR.singleInput.blockControllers(false);
        success(true);
        yield break;
    }

    public IEnumerator computeArmsMeasures(System.Action<bool> success, AvatarDriver driver, DisplayMirror displayMirror, AvatarVR avatarVR)
    {
        avatarVR.singleInput.blockControllers(true);

        if (driver.handLeft && driver.handLeft.activeInHierarchy &&
            driver.handRight && driver.handRight.activeInHierarchy)
        {
            avatarVR.continuousInput.captureDevicesLocation(driver.handLeft, driver.handLeftDevice, PipelineUtils.Stage.SHOULDERS, CAPTURE_NUM_POINTS_SHOULDER, CAPTURE_DISTANCE_THRESHOLD_SHOULDER, true, 0.001f, true);
            avatarVR.continuousInput.captureSecondDevicesLocation(driver.handRight, driver.handRightDevice, CAPTURE_NUM_POINTS_SHOULDER, CAPTURE_DISTANCE_THRESHOLD_SHOULDER, true);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing && !avatarVR.continuousInput.capturingSecond);

            float radiusLeft = 0.0f, radiusRight = 0.0f;
            Vector3 posLeft = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 posRight = new Vector3(0.0f, 0.0f, 0.0f);
            float qualityLeft = 0.0f, qualityRight = 0.0f;
            bool resLeft = Utils.FitSphere((uint)Mathf.Min(avatarVR.continuousInput.captureIndex, CAPTURE_NUM_POINTS_SHOULDER * 2), avatarVR.continuousInput.capturedPoints, ref radiusLeft, ref posLeft, ref qualityLeft);
            bool resRight = Utils.FitSphere((uint)Mathf.Min(avatarVR.continuousInput.captureIndexSecond, CAPTURE_NUM_POINTS_SHOULDER * 2), avatarVR.continuousInput.capturedPointsSecond, ref radiusRight, ref posRight, ref qualityRight);
            if (!resLeft || !resRight)
            {
                string message = PipelineUtils.failureMessageAt(PipelineUtils.Stage.SHOULDERS, 1);
                Debug.Log(message + "\n");
                if (displayMirror)
                {
                    displayMirror.ShowText(message, new Color(1.0f, 0.0f, 0.0f, 0.5f), 2, true, false);
                }
                //ViveInput.blockControllers(false); // no need, displayMirror will unblock them
                success(false);
                yield break;
            }

            bodyMeasures.shoulderCenter.y = (posLeft.y + posRight.y) / 2;

            // Left
            bodyMeasures.shoulderLeft = bodyMeasures.shoulderCenter;
            bodyMeasures.shoulderLeft.x = posLeft.x; //use only x of the sphere

            // Right
            bodyMeasures.shoulderRight = bodyMeasures.shoulderCenter;
            bodyMeasures.shoulderRight.x = posRight.x; //use only x of the sphere

            bodyMeasures.handToShoulderLeft = radiusLeft;
            bodyMeasures.handToWristLeft = radiusLeft;

            bodyMeasures.handToShoulderRight = radiusRight;
            bodyMeasures.handToWristRight = radiusRight;
        }

        setupLeftShoulderJoint(driver.handLeft, avatarVR);
        setupRightShoulderJoint(driver.handRight, avatarVR);

        avatarVR.singleInput.blockControllers(false);
        success(true);
        yield break;
    }

    // Step 9: neck - measures taken from movement
    public IEnumerator computeHeadMeasures(System.Action<bool> success, AvatarDriver driver, DisplayMirror displayMirror, AvatarVR avatarVR)
    {
        avatarVR.singleInput.blockControllers(true);
        Vector3 neck = Vector3.zero;
        if (driver.head && driver.head.activeInHierarchy)
        {
            Vector3 headPosition = Vector3.zero;
            if (driver.head && driver.head.activeInHierarchy)
            {
                headPosition = driver.head.transform.position;
            }


            avatarVR.continuousInput.captureDevicesLocation(driver.head, null, PipelineUtils.Stage.NECK, CAPTURE_NUM_POINTS_HEAD, CAPTURE_DISTANCE_THRESHOLD_HEAD, true, 0.0045f);
            avatarVR.continuousInput.captureRotationPointHead(driver.head, 0.01f);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing);
            Vector2 centerHeadFactors = avatarVR.continuousInput.getRotationPointHead();
            bodyMeasures.depthCenterHead = centerHeadFactors.x;
            bodyMeasures.widthCenterHead = centerHeadFactors.y;

            float radius = 0.0f;
            Vector3 pos = new Vector3(0.0f, 0.0f, 0.0f);
            float quality = 0.0f;
            bool res = Utils.FitSphere((uint)Mathf.Min(avatarVR.continuousInput.captureIndex, CAPTURE_NUM_POINTS_HEAD * 2), avatarVR.continuousInput.capturedPoints, ref radius, ref pos, ref quality);
            if (!res)
            {
                string message = PipelineUtils.failureMessageAt(PipelineUtils.Stage.NECK, 1);
                Debug.Log(message + "\n");
                if (displayMirror)
                {
                    displayMirror.ShowText(message, new Color(1.0f, 0.0f, 0.0f, 0.5f), 2, true, false);
                }
                //ViveInput.blockControllers(false); // no need, displayMirror will unblock them
                success(false);
                yield break;
            }
            // Computing neck position from shouldLeft and shoulderRight positions...
            neck = (bodyMeasures.shoulderLeft + bodyMeasures.shoulderRight) / 2; //center between two shoulders is  the neck
            neck.y = pos.y; //take the captured y Position

            bodyMeasures.headToNeck = -headPosition + neck;
            bodyMeasures.neckHeight = pos.y;
            bodyMeasures.neck = neck;
        }

        setupNeckJoint(driver.head, avatarVR);
        avatarVR.singleInput.blockControllers(false);
        success(true);
        yield break;
    }

    public class RootMeasures
    {
        public Vector3 pelvisPosition;
        public Quaternion pelvisRotation;
        public Vector3 leftFoot;
        public Quaternion leftFootRotation;
        public Vector3 rightFoot;
        public Quaternion rightFootRotation;

        public RootMeasures(Vector3 pelvisPosition, Quaternion pelvisRotation, Vector3 leftFoot, Quaternion leftFootRotation, Vector3 rightFoot, Quaternion rightFootRotation)
        {
            this.pelvisPosition = pelvisPosition;
            this.pelvisRotation = pelvisRotation;
            this.leftFoot = leftFoot;
            this.leftFootRotation = leftFootRotation;
            this.rightFoot = rightFoot;
            this.rightFootRotation = rightFootRotation;
        }
    }

    public RootMeasures SavedRootMeasures = null;

    public IEnumerator computeRootMeasuresUsingAvatar(System.Action<bool> success, AvatarDriver driver, DisplayMirror displayMirror, Animator animator, AvatarVR avatarVR)
    {
        avatarVR.singleInput.blockControllers(true);

        if (driver.pelvis && driver.pelvis.activeInHierarchy)
        {
            // Capture pelvis
            avatarVR.continuousInput.captureInstantPosition(driver.pelvis, driver.pelvisDevice, PipelineUtils.Stage.ROOT_AVATAR);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing);
            Vector3 pelvisPosition = avatarVR.continuousInput.getInstantPosition();
            // Capture left foot
            avatarVR.continuousInput.captureInstantPosition(driver.footLeft, driver.footLeftDevice, PipelineUtils.Stage.ROOT_AVATAR);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing);
            Vector3 leftFoot = avatarVR.continuousInput.getInstantPosition();
            // Capture right foot
            avatarVR.continuousInput.captureInstantPosition(driver.footRight, driver.footRightDevice, PipelineUtils.Stage.ROOT_AVATAR);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing);
            Vector3 rightFoot = avatarVR.continuousInput.getInstantPosition();

            SavedRootMeasures = new RootMeasures(pelvisPosition, driver.pelvis.transform.rotation,
                                                 leftFoot, driver.footLeft.transform.rotation,
                                                 rightFoot, driver.footRight.transform.rotation);

            setupRootJoint(driver.pelvis, pelvisPosition, driver.pelvis.transform.rotation, avatarVR);
            correctLeftAnkleOffset(avatarVR, leftFoot);
            correctRightAnkleOffset(avatarVR, rightFoot);
        }

        avatarVR.singleInput.blockControllers(false);
        success(true);
        yield break;
    }

    public void setupLeftAnkleJoint(GameObject parent, AvatarVR avatarVR)
    {
        // Create left ankle joint
        jointAnkleLeft = new GameObject();
        jointAnkleLeft.transform.SetParent(parent.transform, false);
        jointAnkleLeft.transform.localScale = Vector3.one;
        jointAnkleLeft.name = "jointAnkleLeft";

        // Transform left ankle joint
        Quaternion inverseStart = Quaternion.Inverse(parent.transform.rotation);
        jointAnkleLeft.transform.localRotation = inverseStart;
        jointAnkleLeft.transform.localPosition = jointAnkleLeft.transform.parent.worldToLocalMatrix * new Vector4(0.0f, 0.0f, -bodyMeasures.footToAnkleLeft, 0.0f);   // forward
    }

    public void setupRightAnkleJoint(GameObject parent, AvatarVR avatarVR)
    {
        // Create right ankle joint
        jointAnkleRight = new GameObject();
        jointAnkleRight.transform.SetParent(parent.transform, false);
        jointAnkleRight.transform.localScale = Vector3.one;
        jointAnkleRight.name = "jointAnkleRight";

        // Transform right ankle joint
        Quaternion inverseStart = Quaternion.Inverse(parent.transform.rotation);
        jointAnkleRight.transform.localRotation = inverseStart;
        jointAnkleRight.transform.localPosition = jointAnkleRight.transform.parent.worldToLocalMatrix * new Vector4(0.0f, 0.0f, -bodyMeasures.footToAnkleRight, 0.0f);   // forward
    }

    public void setupRootJoint(GameObject parent, Vector3 pelvisPosition, Quaternion pelvisRotation, AvatarVR avatar)
    {
        // Create root joint
        jointRoot = new GameObject();
        jointRoot.transform.SetParent(parent.transform, false);
        jointRoot.transform.localScale = Vector3.one;
        jointRoot.name = "jointRoot";

        // Transform root joint
        Quaternion inverseStart = Quaternion.Inverse(pelvisRotation);
        jointRoot.transform.localRotation = inverseStart;
        // tracker -> hips
        Vector3 offset = avatar.queryJointPositionAvatar(HumanBodyBones.Hips) - pelvisPosition;
        jointRoot.transform.localPosition = jointRoot.transform.parent.worldToLocalMatrix * new Vector4(offset.x, offset.y, offset.z, 0.0f);
    }

    public void correctLeftAnkleOffset(AvatarVR avatar, Vector3 leftFootPosition)
    {
        // tracker -> leftFoot
        Vector3 offset = avatar.queryJointPositionAvatar(HumanBodyBones.LeftFoot) - leftFootPosition;
        Vector3 offsetLocal = jointAnkleLeft.transform.parent.worldToLocalMatrix * new Vector4(offset.x, offset.y, offset.z, 0.0f);
        jointAnkleLeft.transform.localPosition = offsetLocal;
    }

    public void correctRightAnkleOffset(AvatarVR avatar, Vector3 rightFootPosition)
    {
        // tracker -> rightFoot
        Vector3 offset = avatar.queryJointPositionAvatar(HumanBodyBones.RightFoot) - rightFootPosition;
        Vector3 offsetLocal = jointAnkleRight.transform.parent.worldToLocalMatrix * new Vector4(offset.x, offset.y, offset.z, 0.0f);
        jointAnkleRight.transform.localPosition = offsetLocal;
    }

    public void setupLeftShoulderJoint(GameObject parent, AvatarVR avatarVR)
    {
        // Create left shoulder joint
        jointShoulderLeft = new GameObject();
        jointShoulderLeft.transform.SetParent(parent.transform);
        jointShoulderLeft.name = "jointShoulderLeft";

        // Transform left shoulder joint
        jointShoulderLeft.transform.localRotation = Quaternion.identity;
        jointShoulderLeft.transform.localPosition = -bodyMeasures.handToShoulderLeft * Vector3.forward;

        setupLeftWristJoint(parent, avatarVR);
    }

    public void setupRightShoulderJoint(GameObject parent, AvatarVR avatarVR)
    {
        // Create right shoulder joint
        jointShoulderRight = new GameObject();
        jointShoulderRight.transform.SetParent(parent.transform);
        jointShoulderRight.name = "jointShoulderRight";

        // Transform right shoulder joint
        jointShoulderRight.transform.localRotation = Quaternion.identity;
        jointShoulderRight.transform.localPosition = -bodyMeasures.handToShoulderRight * Vector3.forward;

        setupRightWristJoint(parent, avatarVR);
    }

    public void setupLeftWristJoint(GameObject parent, AvatarVR avatarVR)
    {
        // Create left wrist joint
        jointWristLeft = new GameObject();
        jointWristLeft.transform.SetParent(parent.transform);
        jointWristLeft.name = "jointWristLeft";

        // Transform left wrist joint
        jointWristLeft.transform.localRotation = Quaternion.identity;
        jointWristLeft.transform.localPosition = -0.175f * Vector3.forward;
    }

    public void setupRightWristJoint(GameObject parent, AvatarVR avatarVR)
    {
        // Create right wrist joint
        jointWristRight = new GameObject();
        jointWristRight.transform.SetParent(parent.transform);
        jointWristRight.name = "jointWristRight";

        // Transform right wrist joint
        jointWristRight.transform.localRotation = Quaternion.identity;
        jointWristRight.transform.localPosition = -0.175f * Vector3.forward;
    }

    public void setupNeckJoint(GameObject parent, AvatarVR avatarVR)
    {
        // Create neck joint
        jointNeck = new GameObject();
        jointNeck.transform.SetParent(parent.transform);
        jointNeck.name = "jointNeck";

        // Transform neck joint
        jointNeck.transform.localRotation = Quaternion.identity;
        jointNeck.transform.localPosition = bodyMeasures.headToNeck;
        //jointNeck.transform.localPosition = new Vector3(0.0f, bodyMeasures.headToNeck.y, 0.0f); // HACK

        // Create lookAt Object
        objLookAt = new GameObject();
        objLookAt.transform.SetParent(parent.transform);
        objLookAt.name = "objLookAt";

        // Transform lookAt Object
        objLookAt.transform.localRotation = Quaternion.identity;
        objLookAt.transform.localPosition = LOOKAT_DISTANCE_THRESHOLD * Vector3.forward;
    }
    // Completion flag

    public bool isCompleted()
    {
        return completed;
    }

    public void setCompleted(bool flag)
    {
        completed = flag;
    }

    // Start flag

    public bool isStarted()
    {
        return started;
    }

    public void setStarted(bool flag)
    {
        started = flag;
    }

    private void computeFeetMeasuresUsingRoot(AvatarDriver driver, Vector3 footLeft, Vector3 footRight, Vector3 pelvis, AvatarVR avatarVR)
    {
        if (driver.footLeft && driver.footLeft.activeInHierarchy && driver.pelvis && driver.pelvis.activeInHierarchy)
        {
            //assuming Z is Forward. otherwise compute both Z projection and X projection and take the bigger one (other is the sideways offset from the bodycenter)
            bodyMeasures.footToAnkleLeft = Math.Abs(footLeft.z - pelvis.z);
            setupLeftAnkleJoint(driver.footLeft, avatarVR);

            bodyMeasures.rootToAnkleLeft = Math.Abs(footLeft.y - pelvis.y);
            bodyMeasures.rootToAnkleReset = true;
        }
        if (driver.footRight && driver.footRight.activeInHierarchy && driver.pelvis && driver.pelvis.activeInHierarchy)
        {
            bodyMeasures.footToAnkleRight = Math.Abs(footRight.z - pelvis.z);
            setupRightAnkleJoint(driver.footRight,avatarVR);

            bodyMeasures.rootToAnkleRight = Math.Abs(footRight.y - pelvis.y);
            bodyMeasures.rootToAnkleReset = true;
        }
    }
}
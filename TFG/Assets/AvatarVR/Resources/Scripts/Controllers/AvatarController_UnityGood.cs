using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[RequireComponent(typeof(Animator))]
public class AvatarController_UnityGood : AvatarController
{
    public AvatarVR avatarVR;
    public Animator animator;
    private bool controllersAttached = false;
    private bool controllersAttachedInit = false;

    private int flag = 0;
    public Vector3 hipsToBodyPositionOffset = Vector3.zero;

    private Quaternion InitialWorldNeckRotation;
    private Quaternion InitialWorldSpineRotation;
    private Quaternion InitialLocalHipsRotation;

    public Vector3 ReferenceVector;

    public HumanDescription desc;
    private Vector3 hipsOffset;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        InitialWorldNeckRotation = animator.GetBoneTransform(HumanBodyBones.Head).rotation;
        InitialWorldSpineRotation = animator.GetBoneTransform(HumanBodyBones.Spine).rotation;
        InitialLocalHipsRotation = animator.GetBoneTransform(HumanBodyBones.Hips).localRotation;
        InitFingers();
    }

    void OnAnimatorIK()
    {
        if (!animator)
        {
            return;
        }

        if (!ikActive)
        {
            return;
        }

        Vector3 headPos = driver.head.transform.position;
        Quaternion headRot = driver.head.transform.rotation;
        Vector3 headForward = driver.head.transform.forward;
        Vector3 headRight = driver.head.transform.right;

        // WristLeft
        Vector3 jointWristLeftPos = body.jointWristLeft.transform.position;
        Quaternion jointWristLeftRot = body.jointWristLeft.transform.rotation;
        // WristRight
        Vector3 jointWristRightPos = body.jointWristRight.transform.position;
        Quaternion jointWristRightRot = body.jointWristRight.transform.rotation;
        // Root
        Vector3 jointRootPos = body.jointRoot.transform.position;
        Quaternion jointRootRot = body.jointRoot.transform.rotation;
        // AnkleLeft
        Vector3 jointAnkleLeftPos = body.jointAnkleLeft.transform.position;
        Quaternion jointAnkleLeftRot = body.jointAnkleLeft.transform.rotation;
        // AnkleRight
        Vector3 jointAnkleRightPos = body.jointAnkleRight.transform.position;
        Quaternion jointAnkleRightRot = body.jointAnkleRight.transform.rotation;
        // Head
        Vector3 jointNeckPos = body.jointNeck.transform.position;
        Quaternion jointNeckRot = body.jointNeck.transform.rotation;

        if (flag == 0) // Init
        {
            // Reference Vector to compute vector from spine to neck
            // Assuming T-POSE
            Vector3 centerHead = headPos + headForward * body.bodyMeasures.depthCenterHead + headRight * body.bodyMeasures.widthCenterHead;
            //Debug.Log("centerHead: " + centerHead);
            ReferenceVector = Quaternion.Inverse(animator.bodyRotation) * (centerHead - avatarVR.driver.pelvis.transform.position);
            //Debug.Log("ReferenceVector: " + ReferenceVector);
            ReferenceVector.Normalize();

            flag = 1;
            return;
        }

        hipsOffset = jointRootPos - animator.GetBoneTransform(HumanBodyBones.Hips).position;

        // LeftHand end-effector
        if (body.jointWristLeft != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, jointWristLeftPos - hipsOffset);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, jointWristLeftRot);
        }

        // RightHand end-effector
        if (body.jointWristRight != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, jointWristRightPos - hipsOffset);
            animator.SetIKRotation(AvatarIKGoal.RightHand, jointWristRightRot);
        }

        // LeftFoot end-effector
        if (body.jointAnkleLeft != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, jointAnkleLeftPos - hipsOffset);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, jointAnkleLeftRot);
        }

        // RightFoot end-effector
        if (body.jointAnkleRight != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
            animator.SetIKPosition(AvatarIKGoal.RightFoot, jointAnkleRightPos - hipsOffset);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, jointAnkleRightRot);
        }

        // Root end-effector - NOT REALLY AN END-EFFECTOR, INSTEAD WE CHANGE THE OVERALL BODY TRANSFORM
        if (body.jointRoot != null)
        {
            Transform hipsTransform = animator.GetBoneTransform(HumanBodyBones.Hips);
            animator.SetBoneLocalRotation(HumanBodyBones.Hips, (Quaternion.Inverse(hipsTransform.parent.rotation) * jointRootRot) * InitialLocalHipsRotation);
            //animator.bodyRotation = body.jointRoot.transform.rotation;
        }

        // Neck end-effector - NO END-EFFECTOR AVAILABLE, WE DO THIS MANUALLY
        if (body.jointNeck != null)
        {
            // Spine
            Quaternion bodyRotation = jointRootRot;
            Vector3 centerHead = headPos + headForward * body.bodyMeasures.depthCenterHead + headRight * body.bodyMeasures.widthCenterHead;
            Vector3 rootToNeck = Quaternion.Inverse(bodyRotation) * (centerHead - avatarVR.driver.pelvis.transform.position);
            rootToNeck.Normalize();

            Quaternion newSpineWorldRot = Quaternion.FromToRotation(ReferenceVector, rootToNeck) * InitialWorldSpineRotation;
            animator.SetBoneLocalRotation(HumanBodyBones.Spine, Quaternion.Inverse(animator.GetBoneTransform(HumanBodyBones.Spine).transform.parent.rotation) * newSpineWorldRot);

            // Neck
            // First add the headset rotation to the neck in world space
            Quaternion newNeckWorldRot = jointNeckRot * InitialWorldNeckRotation; // QResult = Q2 * Q1 // Q1 rotation is applied first, then Q2
            // as required by Unity, convert world space to local space by multiplying the inverse of the neck's parent rotation
            animator.SetBoneLocalRotation(HumanBodyBones.Head, Quaternion.Inverse(bodyRotation * animator.GetBoneTransform(HumanBodyBones.Head).transform.parent.rotation) * newNeckWorldRot);
        }
    }

    private void LateUpdate()
    {
        if (flag == 0 || flag == 1)
        {
            if (flag == 1) flag = 2;
            return;
        }

        Vector3 jointRootPos = body.jointRoot.transform.position;

        // Root end-effector
        if (body.jointRoot != null)
        {
            // Hips
            animator.GetBoneTransform(HumanBodyBones.Hips).position = jointRootPos;
        }

        if (controllersAttached)
        {
            // Right Hand
            animator.GetBoneTransform(HumanBodyBones.RightHand).localRotation = animator.GetBoneTransform(HumanBodyBones.RightHand).localRotation * Quaternion.Euler(-0.467f, -80.019f, -32.954f);
            // Left Hand
            animator.GetBoneTransform(HumanBodyBones.LeftHand).localRotation = animator.GetBoneTransform(HumanBodyBones.LeftHand).localRotation * Quaternion.Euler(-2.033f, 79.846f, 32.483f);
        }

        if (controllersAttachedInit)
        {
            InitControllersAttached();
        }

    }

    // Right
    public Transform RightIndexProximal, RightIndexIntermediate, RightIndexDistal;
    private Quaternion RightIndexProximalDef, RightIndexIntermediateDef, RightIndexDistalDef;
    public Transform RightMiddleProximal, RightMiddleIntermediate, RightMiddleDistal;
    private Quaternion RightMiddleProximalDef, RightMiddleIntermediateDef, RightMiddleDistalDef;
    public Transform RightLittleProximal, RightLittleIntermediate, RightLittleDistal;
    private Quaternion RightLittleProximalDef, RightLittleIntermediateDef, RightLittleDistalDef;
    public Transform RightRingProximal, RightRingIntermediate, RightRingDistal;
    private Quaternion RightRingProximalDef, RightRingIntermediateDef, RightRingDistalDef;
    public Transform RightThumbProximal, RightThumbIntermediate, RightThumbDistal;
    private Quaternion RightThumbProximalDef, RightThumbIntermediateDef, RightThumbDistalDef;
    // Left
    public Transform LeftIndexProximal, LeftIndexIntermediate, LeftIndexDistal;
    private Quaternion LeftIndexProximalDef, LeftIndexIntermediateDef, LeftIndexDistalDef;
    public Transform LeftMiddleProximal, LeftMiddleIntermediate, LeftMiddleDistal;
    private Quaternion LeftMiddleProximalDef, LeftMiddleIntermediateDef, LeftMiddleDistalDef;
    public Transform LeftLittleProximal, LeftLittleIntermediate, LeftLittleDistal;
    private Quaternion LeftLittleProximalDef, LeftLittleIntermediateDef, LeftLittleDistalDef;
    public Transform LeftRingProximal, LeftRingIntermediate, LeftRingDistal;
    private Quaternion LeftRingProximalDef, LeftRingIntermediateDef, LeftRingDistalDef;
    public Transform LeftThumbProximal, LeftThumbIntermediate, LeftThumbDistal;
    private Quaternion LeftThumbProximalDef, LeftThumbIntermediateDef, LeftThumbDistalDef;

    // GetBoneTransform of fingers does not work :(
    public void InitFingers()
    {
        if (animator == null) animator = GetComponent<Animator>();
        Transform root = animator.GetBoneTransform(HumanBodyBones.Hips);
        // Right
        RightIndexProximal = RecursiveFindChild(root, "mixamorig:RightHandIndex1");
        if (RightIndexProximal == null) RightIndexProximal = RecursiveFindChild(root, "mixamorig2:RightHandIndex1");
        if (RightIndexProximal == null) RightIndexProximal = RecursiveFindChild(root, "mixamorig1:RightHandIndex1");
        if (RightIndexProximal == null) RightIndexProximal = RecursiveFindChild(root, "mixamorig12:RightHandIndex1");
        RightIndexProximalDef = RightIndexProximal.localRotation;
        RightIndexIntermediate = RecursiveFindChild(root, "mixamorig:RightHandIndex2");
        if (RightIndexIntermediate == null) RightIndexIntermediate = RecursiveFindChild(root, "mixamorig2:RightHandIndex2");
        if (RightIndexIntermediate == null) RightIndexIntermediate = RecursiveFindChild(root, "mixamorig1:RightHandIndex2");
        if (RightIndexIntermediate == null) RightIndexIntermediate = RecursiveFindChild(root, "mixamorig12:RightHandIndex2");
        RightIndexIntermediateDef = RightIndexIntermediate.localRotation;
        RightIndexDistal = RecursiveFindChild(root, "mixamorig:RightHandIndex3");
        if (RightIndexDistal == null) RightIndexDistal = RecursiveFindChild(root, "mixamorig2:RightHandIndex3");
        if (RightIndexDistal == null) RightIndexDistal = RecursiveFindChild(root, "mixamorig1:RightHandIndex3");
        if (RightIndexDistal == null) RightIndexDistal = RecursiveFindChild(root, "mixamorig12:RightHandIndex3");
        RightIndexDistalDef = RightIndexDistal.localRotation;
        RightMiddleProximal = RecursiveFindChild(root, "mixamorig:RightHandMiddle1");
        if (RightMiddleProximal == null) RightMiddleProximal = RecursiveFindChild(root, "mixamorig2:RightHandMiddle1");
        if (RightMiddleProximal == null) RightMiddleProximal = RecursiveFindChild(root, "mixamorig1:RightHandMiddle1");
        if (RightMiddleProximal == null) RightMiddleProximal = RecursiveFindChild(root, "mixamorig12:RightHandMiddle1");
        RightMiddleProximalDef = RightMiddleProximal.localRotation;
        RightMiddleIntermediate = RecursiveFindChild(root, "mixamorig:RightHandMiddle2");
        if (RightMiddleIntermediate == null) RightMiddleIntermediate = RecursiveFindChild(root, "mixamorig2:RightHandMiddle2");
        if (RightMiddleIntermediate == null) RightMiddleIntermediate = RecursiveFindChild(root, "mixamorig1:RightHandMiddle2");
        if (RightMiddleIntermediate == null) RightMiddleIntermediate = RecursiveFindChild(root, "mixamorig12:RightHandMiddle2");
        RightMiddleIntermediateDef = RightMiddleIntermediate.localRotation;
        RightMiddleDistal = RecursiveFindChild(root, "mixamorig:RightHandMiddle3");
        if (RightMiddleDistal == null) RightMiddleDistal = RecursiveFindChild(root, "mixamorig2:RightHandMiddle3");
        if (RightMiddleDistal == null) RightMiddleDistal = RecursiveFindChild(root, "mixamorig1:RightHandMiddle3");
        if (RightMiddleDistal == null) RightMiddleDistal = RecursiveFindChild(root, "mixamorig12:RightHandMiddle3");
        RightMiddleDistalDef = RightMiddleDistal.localRotation;
        RightLittleProximal = RecursiveFindChild(root, "mixamorig:RightHandPinky1");
        if (RightLittleProximal == null) RightLittleProximal = RecursiveFindChild(root, "mixamorig2:RightHandPinky1");
        if (RightLittleProximal == null) RightLittleProximal = RecursiveFindChild(root, "mixamorig1:RightHandPinky1");
        if (RightLittleProximal == null) RightLittleProximal = RecursiveFindChild(root, "mixamorig12:RightHandPinky1");
        RightLittleProximalDef = RightLittleProximal.localRotation;
        RightLittleIntermediate = RecursiveFindChild(root, "mixamorig:RightHandPinky2");
        if (RightLittleIntermediate == null) RightLittleIntermediate = RecursiveFindChild(root, "mixamorig2:RightHandPinky2");
        if (RightLittleIntermediate == null) RightLittleIntermediate = RecursiveFindChild(root, "mixamorig1:RightHandPinky2");
        if (RightLittleIntermediate == null) RightLittleIntermediate = RecursiveFindChild(root, "mixamorig12:RightHandPinky2");
        RightLittleIntermediateDef = RightLittleIntermediate.localRotation;
        RightLittleDistal = RecursiveFindChild(root, "mixamorig:RightHandPinky3");
        if (RightLittleDistal == null) RightLittleDistal = RecursiveFindChild(root, "mixamorig2:RightHandPinky3");
        if (RightLittleDistal == null) RightLittleDistal = RecursiveFindChild(root, "mixamorig1:RightHandPinky3");
        if (RightLittleDistal == null) RightLittleDistal = RecursiveFindChild(root, "mixamorig12:RightHandPinky3");
        RightLittleDistalDef = RightLittleDistal.localRotation;
        RightRingProximal = RecursiveFindChild(root, "mixamorig:RightHandRing1");
        if (RightRingProximal == null) RightRingProximal = RecursiveFindChild(root, "mixamorig2:RightHandRing1");
        if (RightRingProximal == null) RightRingProximal = RecursiveFindChild(root, "mixamorig1:RightHandRing1");
        if (RightRingProximal == null) RightRingProximal = RecursiveFindChild(root, "mixamorig12:RightHandRing1");
        RightRingProximalDef = RightRingProximal.localRotation;
        RightRingIntermediate = RecursiveFindChild(root, "mixamorig:RightHandRing2");
        if (RightRingIntermediate == null) RightRingIntermediate = RecursiveFindChild(root, "mixamorig2:RightHandRing2");
        if (RightRingIntermediate == null) RightRingIntermediate = RecursiveFindChild(root, "mixamorig1:RightHandRing2");
        if (RightRingIntermediate == null) RightRingIntermediate = RecursiveFindChild(root, "mixamorig12:RightHandRing2");
        RightRingIntermediateDef = RightRingIntermediate.localRotation;
        RightRingDistal = RecursiveFindChild(root, "mixamorig:RightHandRing3");
        if (RightRingDistal == null) RightRingDistal = RecursiveFindChild(root, "mixamorig2:RightHandRing3");
        if (RightRingDistal == null) RightRingDistal = RecursiveFindChild(root, "mixamorig1:RightHandRing3");
        if (RightRingDistal == null) RightRingDistal = RecursiveFindChild(root, "mixamorig12:RightHandRing3");
        RightRingDistalDef = RightRingDistal.localRotation;
        RightThumbProximal = RecursiveFindChild(root, "mixamorig:RightHandThumb1");
        if (RightThumbProximal == null) RightThumbProximal = RecursiveFindChild(root, "mixamorig2:RightHandThumb1");
        if (RightThumbProximal == null) RightThumbProximal = RecursiveFindChild(root, "mixamorig1:RightHandThumb1");
        if (RightThumbProximal == null) RightThumbProximal = RecursiveFindChild(root, "mixamorig12:RightHandThumb1");
        RightThumbProximalDef = RightThumbProximal.localRotation;
        RightThumbIntermediate = RecursiveFindChild(root, "mixamorig:RightHandThumb2");
        if (RightThumbIntermediate == null) RightThumbIntermediate = RecursiveFindChild(root, "mixamorig2:RightHandThumb2");
        if (RightThumbIntermediate == null) RightThumbIntermediate = RecursiveFindChild(root, "mixamorig1:RightHandThumb2");
        if (RightThumbIntermediate == null) RightThumbIntermediate = RecursiveFindChild(root, "mixamorig12:RightHandThumb2");
        RightThumbIntermediateDef = RightThumbIntermediate.localRotation;
        RightThumbDistal = RecursiveFindChild(root, "mixamorig:RightHandThumb3");
        if (RightThumbDistal == null) RightThumbDistal = RecursiveFindChild(root, "mixamorig2:RightHandThumb3");
        if (RightThumbDistal == null) RightThumbDistal = RecursiveFindChild(root, "mixamorig1:RightHandThumb3");
        if (RightThumbDistal == null) RightThumbDistal = RecursiveFindChild(root, "mixamorig12:RightHandThumb3");
        RightThumbDistalDef = RightThumbDistal.localRotation;
        // Left
        LeftIndexProximal = RecursiveFindChild(root, "mixamorig:LeftHandIndex1");
        if (LeftIndexProximal == null) LeftIndexProximal = RecursiveFindChild(root, "mixamorig2:LeftHandIndex1");
        if (LeftIndexProximal == null) LeftIndexProximal = RecursiveFindChild(root, "mixamorig1:LeftHandIndex1");
        if (LeftIndexProximal == null) LeftIndexProximal = RecursiveFindChild(root, "mixamorig12:LeftHandIndex1");
        LeftIndexProximalDef = LeftIndexProximal.localRotation;
        LeftIndexIntermediate = RecursiveFindChild(root, "mixamorig:LeftHandIndex2");
        if (LeftIndexIntermediate == null) LeftIndexIntermediate = RecursiveFindChild(root, "mixamorig2:LeftHandIndex2");
        if (LeftIndexIntermediate == null) LeftIndexIntermediate = RecursiveFindChild(root, "mixamorig1:LeftHandIndex2");
        if (LeftIndexIntermediate == null) LeftIndexIntermediate = RecursiveFindChild(root, "mixamorig12:LeftHandIndex2");
        LeftIndexIntermediateDef = LeftIndexIntermediate.localRotation;
        LeftIndexDistal = RecursiveFindChild(root, "mixamorig:LeftHandIndex3");
        if (LeftIndexDistal == null) LeftIndexDistal = RecursiveFindChild(root, "mixamorig2:LeftHandIndex3");
        if (LeftIndexDistal == null) LeftIndexDistal = RecursiveFindChild(root, "mixamorig1:LeftHandIndex3");
        if (LeftIndexDistal == null) LeftIndexDistal = RecursiveFindChild(root, "mixamorig12:LeftHandIndex3");
        LeftIndexDistalDef = LeftIndexDistal.localRotation;
        LeftMiddleProximal = RecursiveFindChild(root, "mixamorig:LeftHandMiddle1");
        if (LeftMiddleProximal == null) LeftMiddleProximal = RecursiveFindChild(root, "mixamorig2:LeftHandMiddle1");
        if (LeftMiddleProximal == null) LeftMiddleProximal = RecursiveFindChild(root, "mixamorig1:LeftHandMiddle1");
        if (LeftMiddleProximal == null) LeftMiddleProximal = RecursiveFindChild(root, "mixamorig12:LeftHandMiddle1");
        LeftMiddleProximalDef = LeftMiddleProximal.localRotation;
        LeftMiddleIntermediate = RecursiveFindChild(root, "mixamorig:LeftHandMiddle2");
        if (LeftMiddleIntermediate == null) LeftMiddleIntermediate = RecursiveFindChild(root, "mixamorig2:LeftHandMiddle2");
        if (LeftMiddleIntermediate == null) LeftMiddleIntermediate = RecursiveFindChild(root, "mixamorig1:LeftHandMiddle2");
        if (LeftMiddleIntermediate == null) LeftMiddleIntermediate = RecursiveFindChild(root, "mixamorig12:LeftHandMiddle2");
        LeftMiddleIntermediateDef = LeftMiddleIntermediate.localRotation;
        LeftMiddleDistal = RecursiveFindChild(root, "mixamorig:LeftHandMiddle3");
        if (LeftMiddleDistal == null) LeftMiddleDistal = RecursiveFindChild(root, "mixamorig2:LeftHandMiddle3");
        if (LeftMiddleDistal == null) LeftMiddleDistal = RecursiveFindChild(root, "mixamorig1:LeftHandMiddle3");
        if (LeftMiddleDistal == null) LeftMiddleDistal = RecursiveFindChild(root, "mixamorig12:LeftHandMiddle3");
        LeftMiddleDistalDef = LeftMiddleDistal.localRotation;
        LeftLittleProximal = RecursiveFindChild(root, "mixamorig:LeftHandPinky1");
        if (LeftLittleProximal == null) LeftLittleProximal = RecursiveFindChild(root, "mixamorig2:LeftHandPinky1");
        if (LeftLittleProximal == null) LeftLittleProximal = RecursiveFindChild(root, "mixamorig1:LeftHandPinky1");
        if (LeftLittleProximal == null) LeftLittleProximal = RecursiveFindChild(root, "mixamorig12:LeftHandPinky1");
        LeftLittleProximalDef = LeftLittleProximal.localRotation;
        LeftLittleIntermediate = RecursiveFindChild(root, "mixamorig:LeftHandPinky2");
        if (LeftLittleIntermediate == null) LeftLittleIntermediate = RecursiveFindChild(root, "mixamorig2:LeftHandPinky2");
        if (LeftLittleIntermediate == null) LeftLittleIntermediate = RecursiveFindChild(root, "mixamorig1:LeftHandPinky2");
        if (LeftLittleIntermediate == null) LeftLittleIntermediate = RecursiveFindChild(root, "mixamorig12:LeftHandPinky2");
        LeftLittleIntermediateDef = LeftLittleIntermediate.localRotation;
        LeftLittleDistal = RecursiveFindChild(root, "mixamorig:LeftHandPinky3");
        if (LeftLittleDistal == null) LeftLittleDistal = RecursiveFindChild(root, "mixamorig2:LeftHandPinky3");
        if (LeftLittleDistal == null) LeftLittleDistal = RecursiveFindChild(root, "mixamorig1:LeftHandPinky3");
        if (LeftLittleDistal == null) LeftLittleDistal = RecursiveFindChild(root, "mixamorig12:LeftHandPinky3");
        LeftLittleDistalDef = LeftLittleDistal.localRotation;
        LeftRingProximal = RecursiveFindChild(root, "mixamorig:LeftHandRing1");
        if (LeftRingProximal == null) LeftRingProximal = RecursiveFindChild(root, "mixamorig2:LeftHandRing1");
        if (LeftRingProximal == null) LeftRingProximal = RecursiveFindChild(root, "mixamorig1:LeftHandRing1");
        if (LeftRingProximal == null) LeftRingProximal = RecursiveFindChild(root, "mixamorig12:LeftHandRing1");
        LeftRingProximalDef = LeftRingProximal.localRotation;
        LeftRingIntermediate = RecursiveFindChild(root, "mixamorig:LeftHandRing2");
        if (LeftRingIntermediate == null) LeftRingIntermediate = RecursiveFindChild(root, "mixamorig2:LeftHandRing2");
        if (LeftRingIntermediate == null) LeftRingIntermediate = RecursiveFindChild(root, "mixamorig1:LeftHandRing2");
        if (LeftRingIntermediate == null) LeftRingIntermediate = RecursiveFindChild(root, "mixamorig12:LeftHandRing2");
        LeftRingIntermediateDef = LeftRingIntermediate.localRotation;
        LeftRingDistal = RecursiveFindChild(root, "mixamorig:LeftHandRing3");
        if (LeftRingDistal == null) LeftRingDistal = RecursiveFindChild(root, "mixamorig2:LeftHandRing3");
        if (LeftRingDistal == null) LeftRingDistal = RecursiveFindChild(root, "mixamorig1:LeftHandRing3");
        if (LeftRingDistal == null) LeftRingDistal = RecursiveFindChild(root, "mixamorig12:LeftHandRing3");
        LeftRingDistalDef = LeftRingDistal.localRotation;
        LeftThumbProximal = RecursiveFindChild(root, "mixamorig:LeftHandThumb1");
        if (LeftThumbProximal == null) LeftThumbProximal = RecursiveFindChild(root, "mixamorig2:LeftHandThumb1");
        if (LeftThumbProximal == null) LeftThumbProximal = RecursiveFindChild(root, "mixamorig1:LeftHandThumb1");
        if (LeftThumbProximal == null) LeftThumbProximal = RecursiveFindChild(root, "mixamorig12:LeftHandThumb1");
        LeftThumbProximalDef = LeftThumbProximal.localRotation;
        LeftThumbIntermediate = RecursiveFindChild(root, "mixamorig:LeftHandThumb2");
        if (LeftThumbIntermediate == null) LeftThumbIntermediate = RecursiveFindChild(root, "mixamorig2:LeftHandThumb2");
        if (LeftThumbIntermediate == null) LeftThumbIntermediate = RecursiveFindChild(root, "mixamorig1:LeftHandThumb2");
        if (LeftThumbIntermediate == null) LeftThumbIntermediate = RecursiveFindChild(root, "mixamorig12:LeftHandThumb2");
        LeftThumbIntermediateDef = LeftThumbIntermediate.localRotation;
        LeftThumbDistal = RecursiveFindChild(root, "mixamorig:LeftHandThumb3");
        if (LeftThumbDistal == null) LeftThumbDistal = RecursiveFindChild(root, "mixamorig2:LeftHandThumb3");
        if (LeftThumbDistal == null) LeftThumbDistal = RecursiveFindChild(root, "mixamorig1:LeftHandThumb3");
        if (LeftThumbDistal == null) LeftThumbDistal = RecursiveFindChild(root, "mixamorig12:LeftHandThumb3");
        LeftThumbDistalDef = LeftThumbDistal.localRotation;
    }

    private void InitControllersAttached()
    {
        if (controllersAttached)
        {
            HandController.SetFingersRotations(this, avatarVR);
        }
        else
        {
            // Right Index
            RightIndexProximal.localRotation = RightIndexProximalDef;
            RightIndexIntermediate.localRotation = RightIndexIntermediateDef;
            RightIndexDistal.localRotation = RightIndexDistalDef;
            // Right Middle
            RightMiddleProximal.localRotation = RightMiddleProximalDef;
            RightMiddleIntermediate.localRotation = RightMiddleIntermediateDef;
            RightMiddleDistal.localRotation = RightMiddleDistalDef;
            // Right Little
            RightLittleProximal.localRotation = RightLittleProximalDef;
            RightLittleIntermediate.localRotation = RightLittleIntermediateDef;
            RightLittleDistal.localRotation = RightLittleDistalDef;
            // Right Ring
            RightRingProximal.localRotation = RightRingProximalDef;
            RightRingIntermediate.localRotation = RightRingIntermediateDef;
            RightRingDistal.localRotation = RightRingDistalDef;
            // Right Thumb
            RightThumbProximal.localRotation = RightThumbProximalDef;
            RightThumbIntermediate.localRotation = RightThumbIntermediateDef;
            RightThumbDistal.localRotation = RightThumbDistalDef;

            // Left Index
            LeftIndexProximal.localRotation = LeftIndexProximalDef;
            LeftIndexIntermediate.localRotation = LeftIndexIntermediateDef;
            LeftIndexDistal.localRotation = LeftIndexDistalDef;
            // Left Middle
            LeftMiddleProximal.localRotation = LeftMiddleProximalDef;
            LeftMiddleIntermediate.localRotation = LeftMiddleIntermediateDef;
            LeftMiddleDistal.localRotation = LeftMiddleDistalDef;
            // Left Little
            LeftLittleProximal.localRotation = LeftLittleProximalDef;
            LeftLittleIntermediate.localRotation = LeftLittleIntermediateDef;
            LeftLittleDistal.localRotation = LeftLittleDistalDef;
            // Left Ring
            LeftRingProximal.localRotation = LeftRingProximalDef;
            LeftRingIntermediate.localRotation = LeftRingIntermediateDef;
            LeftRingDistal.localRotation = LeftRingDistalDef;
            // Left Thumb
            LeftThumbProximal.localRotation = LeftThumbProximalDef;
            LeftThumbIntermediate.localRotation = LeftThumbIntermediateDef;
            LeftThumbDistal.localRotation = LeftThumbDistalDef;
        }
        controllersAttachedInit = false;
    }

    public void SetControllersAttached(bool e)
    {
        controllersAttachedInit = true;
        controllersAttached = e;
    }

    Transform RecursiveFindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }
            else
            {
                Transform found = RecursiveFindChild(child, childName);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
    }
}

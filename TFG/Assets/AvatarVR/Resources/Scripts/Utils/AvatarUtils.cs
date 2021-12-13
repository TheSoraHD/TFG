using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AvatarUtils
{
    public struct ScaleSkeletonOptions
    {
        public bool scaleAvatarMeshY;
        public bool fittingSkeleton;
        public bool scaleAvatarSkeletonChest, scaleAvatarSkeletonFoot;

        public ScaleSkeletonOptions(bool scAvatarMeshY = true, bool scAvatarSkeleton = true, bool scAvatarSkeletonChest = false, bool scAvatarSkeletonFoot = false)
        {
            scaleAvatarMeshY = scAvatarMeshY;
            fittingSkeleton = scAvatarSkeleton;
            scaleAvatarSkeletonChest = scAvatarSkeletonChest;
            scaleAvatarSkeletonFoot = scAvatarSkeletonFoot;
        }
    }
    // Get information from a character from the Resources folder
    public static bool getCharacterInfo(
        string assetName,
        string skeletonFile,
        ref float eyeHeight,
        ref float shoulderToshoulder
    )
    {
        // Catch wrong input
        if (assetName == "")
        {
            Debug.LogError("File " + assetName + " not loaded in from resources correctly. Cannot make character.");
            return false;
        }
        if (skeletonFile == "")
        {
            skeletonFile = assetName + "_skeleton";
        }

        // Import a FBX file from resources
        GameObject character = GameObject.Instantiate(Resources.Load(assetName)) as GameObject;
        if (!character)
        {
            Debug.LogError("File " + assetName + " not loaded in from resources correctly. Cannot make character.");
            return false;
        }

        // Reset character position
        character.transform.position = Vector3.zero;

        // TEST: check if localScale affects the position of joints: answer YES
        //character.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f); 

        // Load in the bone mapping from file
        List<HumanBone> human = new List<HumanBone>();
        List<SkeletonBone> skeleton = new List<SkeletonBone>();
        if (!loadSkeleton(skeletonFile, ref human, ref skeleton))
        {
            return false;
        }

        // Compute info body measures
        Transform foundTransform;
        string boneName = "", boneNameRight = "", boneNameLeft = "";
        if (SkeletonUtils.GetBoneNameByHumanName(human, "LeftEye", ref boneName))
        {
            foundTransform = Utils.FindDescendants(character.transform, boneName);
            eyeHeight = foundTransform.position.y;
        }
        if (SkeletonUtils.GetBoneNameByHumanName(human, "Hips", ref boneName))
        {
            foundTransform = Utils.FindDescendants(character.transform, boneName);
        }
        if (SkeletonUtils.GetBoneNameByHumanName(human, "LeftUpperArm", ref boneNameLeft) && SkeletonUtils.GetBoneNameByHumanName(human, "RightUpperArm", ref boneNameRight))

        {
            Transform foundTransformLeft = Utils.FindDescendants(character.transform, boneNameLeft);
            Transform foundTransformRight = Utils.FindDescendants(character.transform, boneNameRight);
            shoulderToshoulder = (foundTransformLeft.position - foundTransformRight.position).magnitude;
        }
        // Get the skeleton pose in T-pose / at rest
        //SkeletonUtils.SkeletonPose skeletonTPose = SkeletonUtils.GetSkeletonPose(character, human);

        // TEST: check if localScale affects the position of joints: answer YES
        //Debug.Log("Tamaño avatar antes escalado via joints: altura ojos " + eyeHeight + " altura root: " + rootPos.y);

        // Delete the FBX instance
        GameObject.Destroy(character);
        return true;
    }

    // Imports a character from the Resources folder
    public static bool createCharacter(string assetName, string skeletonFile, string controllerFile, AvatarBody body, ScaleSkeletonOptions scaleSkeletonOptions,
        ref GameObject character, AvatarVR avatarVR)
    {
        HumanDescription description = new HumanDescription();
        return createCharacter(assetName, skeletonFile, controllerFile, body, scaleSkeletonOptions, out character, ref description, null, avatarVR);
    }

    // Imports a character from the Resources folder
    public static bool createCharacter(string assetName, string skeletonFile, string controllerFile, AvatarBody body, ScaleSkeletonOptions scaleSkeletonOptions,
        out GameObject character, ref HumanDescription description, GameObject floorMarker, AvatarVR avatarVR)
    {
        // Catch wrong input
        if (assetName == "")
        {
            Debug.LogError("File " + assetName + " not loaded in from resources correctly. Cannot make character.");
            character = null;
            return false;
        }
        if (skeletonFile == "")
        {
            skeletonFile = assetName + "_skeleton";
        }

        // Import a FBX file from resources
        character = GameObject.Instantiate(Resources.Load(assetName)) as GameObject;
        if (!character)
        {
            Debug.LogError("File " + assetName + " not loaded in from resources correctly. Cannot make character.");
            return false;
        }

        // Reset character position
        character.transform.position = Vector3.zero;

        // Add an animator component
        Animator animator;
        if (character.GetComponent<Animator>())
        {
            animator = character.GetComponent<Animator>();
        }
        else
        {
            animator = character.AddComponent<Animator>();
        }
        if (floorMarker != null)
        {
            Transform leftToesTransform = animator.GetBoneTransform(HumanBodyBones.LeftToes);
            floorMarker.transform.SetParent(leftToesTransform, false);
            floorMarker.transform.position = character.transform.position;
        }
        // Set up the description for the humanoid
        bool success = setupHumanDescription(character, skeletonFile, body, scaleSkeletonOptions, ref description, floorMarker, avatarVR);
        if (!success)
        {
            Debug.LogError("Cannot create HumanDescription from resources " + assetName + ".fbx and " + skeletonFile);
            return false;
        }

        // Tell Unity not to modify the positions of the joints
        description.upperArmTwist = 0.5f;
        description.lowerArmTwist = 0.5f;
        description.upperLegTwist = 0.5f;
        description.lowerLegTwist = 0.5f;
        description.armStretch = 0.0f;
        description.legStretch = 0.0f;
        description.feetSpacing = 0.0f;
        description.hasTranslationDoF = false;

        // Create the avatar using the GameObject and the HumanDescription
        Avatar a = AvatarBuilder.BuildHumanAvatar(character, description);
        if (!a)
        {
            Debug.LogError("Cannot build Avatar from resources " + assetName + ".fbx and " + skeletonFile);
            return false;
        }

        // Set the avatar in the animator component
        animator.avatar = a;

        // Load the animator controller from resources if specified
        if (controllerFile != "")
        {
            RuntimeAnimatorController c = (RuntimeAnimatorController)RuntimeAnimatorController.Instantiate(Resources.Load(controllerFile));
            if (!c)
            {
                Debug.LogError("File " + controllerFile + " not loaded in from resources correctly. Cannot add animation controller.");
                return false;
            }

            // Set the controller in the animator component
            animator.runtimeAnimatorController = c;
        }

        return true;
    }

    /***********************************************************************************************************************/

    // Creates a HumanDescription given the parameters for your character
    //
    // parameters:
    //    INPUT
    //    - GameObject character:       the GameObject that will be your character
    //    - string skeletonFile:        text file name in the Resources folder that describes a mapping from your bone names to Unity's bone names
    //    - AvatarBody body:            object containing body measures
    //    - bool scaleMesh:             whether to scale the mesh/model of the avatar
    //    - bool scaleSkeleton:         whether to scale the skeleton of the avatar using the body measures in 'body'
    //    OUTPUT
    //    - HumanDescription desc:      a HumanDescription built with the appropiate skeleton
    //
    private static bool setupHumanDescription(GameObject character, string skeletonFile, AvatarBody body, ScaleSkeletonOptions scaleSkeletonOptions, ref HumanDescription desc, GameObject floorMarker, AvatarVR avatarVR)
    {
        // Load in the bone mapping from file
        List<HumanBone> human = new List<HumanBone>();
        List<SkeletonBone> skeleton = new List<SkeletonBone>();
        if (!loadSkeleton(skeletonFile, ref human, ref skeleton))
        {
            return false;
        }

        // The human bone array is the list we've already composed
        desc.human = human.ToArray();

        // Apply overall characted scale (model/mesh, not skeleton)
        float avatarScaleY = 1.0f;

        if (scaleSkeletonOptions.scaleAvatarMeshY)
        {
            string bone = "";
            if (!SkeletonUtils.GetBoneNameByHumanName(human, "LeftEye", ref bone))
            {
                Debug.LogError("Cannot get eyes height from loaded character.");
                return false;
            }
            Transform t = Utils.FindDescendants(character.transform, bone);
            if (!t)
            {
                Debug.LogError("Cannot get eyes height from loaded character.");
                return false;
            }
            float avatarEyeHeight = t.position.y;
            if (!SkeletonUtils.GetBoneNameByHumanName(human, "Hips", ref bone))
            {
                Debug.LogError("Cannot get root height from loaded character.");
                return false;
            }
            t = Utils.FindDescendants(character.transform, bone);
            if (!t)
            {
                Debug.LogError("Cannot get root height from loaded character.");
                return false;
            }
            avatarScaleY = body.bodyMeasures.eyesHeight / avatarEyeHeight;
            //NOTE: Hips/Root height should be loaded, but are not used?
        }

        // Uniform scale
        character.transform.localScale = new Vector3(avatarScaleY, avatarScaleY, avatarScaleY);
        //Debug.Log("TEST - localScale: " + avatarScaleY);

        // Compute default body measures
        Transform foundTransform1, foundTransform2;
        string boneName1 = "", boneName2 = "";
        string hips = avatarVR.legsConnectedToHips ? "Hips" : "Spine";

        #region Left Arm
        float defaultLeftLowerArmSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "LeftHand", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "LeftLowerArm", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultLeftLowerArmSize = (foundTransform1.position - foundTransform2.position).magnitude;
        }

        float defaultLeftUpperArmSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "LeftLowerArm", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "LeftUpperArm", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultLeftUpperArmSize = (foundTransform1.position - foundTransform2.position).magnitude;
        }
        #endregion

        #region Right Arm
        float defaultRightUpperArmSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "RightLowerArm", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "RightUpperArm", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultRightUpperArmSize = (foundTransform1.position - foundTransform2.position).magnitude;
        }

        float defaultRightLowerArmSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "RightHand", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "RightLowerArm", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultRightLowerArmSize = (foundTransform1.position - foundTransform2.position).magnitude;
        }
        #endregion

        #region Left Leg
        float defaultLeftLowerLegSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "LeftFoot", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "LeftLowerLeg", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            // We need to use magnitude here because when the IK is applied, lowerLeg and lowerArm are "folded"
            defaultLeftLowerLegSize = (foundTransform1.position - foundTransform2.position).magnitude;
        }

        float defaultLeftUpperLegSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "LeftLowerLeg", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "LeftUpperLeg", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultLeftUpperLegSize = (foundTransform1.position - foundTransform2.position).magnitude;
        }

        float defaultLeftUpperLegToHipSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "LeftUpperLeg", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, hips, ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            //defaultLeftUpperLegToHipSize = (foundTransform1.position - foundTransform2.position).magnitude;
            //scale hips to leg just vertically
            defaultLeftUpperLegToHipSize = Mathf.Abs(foundTransform1.position.y - foundTransform2.position.y);
        }
        #endregion

        #region Right Leg
        float defaultRightLowerLegSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "RightFoot", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "RightLowerLeg", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            // We need to use magnitude here because when the IK is applied, lowerLeg and lowerArm are "folded"
            defaultRightLowerLegSize = (foundTransform1.position - foundTransform2.position).magnitude;
        }

        float defaultRightUpperLegSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "RightLowerLeg", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "RightUpperLeg", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultRightUpperLegSize = (foundTransform1.position - foundTransform2.position).magnitude;
        }

        float defaultRightUpperLegToHipSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "RightUpperLeg", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, hips, ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            //defaultRightUpperLegToHipSize = (foundTransform1.position - foundTransform2.position).magnitude;
            //scale leg to hips only vertically
            defaultRightUpperLegToHipSize = Mathf.Abs(foundTransform1.position.y - foundTransform2.position.y);
        }
        #endregion

        #region Shoulders
        float defaultLeftUpperArmToRightUpperArm = 0;
        float defaultShouldersHeight = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "LeftUpperArm", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "RightUpperArm", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultLeftUpperArmToRightUpperArm = (foundTransform1.position - foundTransform2.position).magnitude;
            defaultShouldersHeight = foundTransform1.position.y;
        }

        float defaultLeftShoulderToRightShoulder = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "LeftShoulder", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "RightShoulder", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultLeftShoulderToRightShoulder = (foundTransform1.position - foundTransform2.position).magnitude;
        }

        float defaultRightUpperArmToShoulderSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "RightUpperArm", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "RightShoulder", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultRightUpperArmToShoulderSize = (foundTransform1.position - foundTransform2.position).magnitude;
        }

        float defaultLeftUpperArmToShoulderSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "LeftUpperArm", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "LeftShoulder", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultLeftUpperArmToShoulderSize = (foundTransform1.position - foundTransform2.position).magnitude;
        }

        float defaultRightShoulderUpperChestSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "RightShoulder", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "UpperChest", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultRightShoulderUpperChestSize = (foundTransform1.position - foundTransform2.position).magnitude;
        }

        float defaultLeftShoulderUpperChestSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "LeftShoulder", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "UpperChest", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultLeftShoulderUpperChestSize = (foundTransform1.position - foundTransform2.position).magnitude;
        }
        #endregion

        #region Back
        float defaultHipsToSpineSize = 0;
        float defaultHipsHeight = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "Hips", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "Spine", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultHipsToSpineSize = Mathf.Abs(foundTransform1.position.y - foundTransform2.position.y);
            defaultHipsHeight = foundTransform1.position.y;
        }

        float defaultSpineToChestSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "Spine", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "Chest", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultSpineToChestSize = Mathf.Abs(foundTransform1.position.y - foundTransform2.position.y);
        }

        float defaultChestToUpperChestSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "Chest", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "UpperChest", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultChestToUpperChestSize = Mathf.Abs(foundTransform1.position.y - foundTransform2.position.y);
        }

        float defaultUpperChestToNeckSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "UpperChest", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "Neck", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultUpperChestToNeckSize = Mathf.Abs(foundTransform1.position.y - foundTransform2.position.y);
        }
        #endregion

        #region Head
        float defaultNeckToHeadSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "Neck", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "Head", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultNeckToHeadSize = Mathf.Abs(foundTransform1.position.y - foundTransform2.position.y);
        }
        float defaultHeadToEyes = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "Head", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "LeftEye", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultHeadToEyes = Mathf.Abs(foundTransform1.position.y - foundTransform2.position.y);
        }
        #endregion

        #region Feet
        float defaultFootToToesSize = 0;
        if (SkeletonUtils.GetBoneNameByHumanName(human, "LeftFoot", ref boneName1) &&
            SkeletonUtils.GetBoneNameByHumanName(human, "LeftToes", ref boneName2))
        {
            foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
            foundTransform2 = Utils.FindDescendants(character.transform, boneName2);
            defaultFootToToesSize = (foundTransform1.position - foundTransform2.position).magnitude;
        }
        #endregion

        float defaultWristToShoulderLeft = defaultLeftLowerArmSize + defaultLeftUpperArmSize;
        float defaultWristToShoulderRight = defaultRightLowerArmSize + defaultRightUpperArmSize;

        float defaultShoulderToShoulder = defaultLeftUpperArmToShoulderSize + defaultLeftShoulderToRightShoulder + defaultRightUpperArmToShoulderSize;

        float defaultLegSizeLeft = defaultLeftLowerLegSize + defaultLeftUpperLegSize + defaultLeftUpperLegToHipSize;
        float defaultLegSizeRight = defaultRightLowerLegSize + defaultRightUpperLegSize + defaultRightUpperLegToHipSize;

        float defaultRootToUpperChest = defaultHipsToSpineSize + defaultSpineToChestSize + defaultChestToUpperChestSize;
        float defaultRootToNeck = defaultRootToUpperChest + defaultUpperChestToNeckSize;
        float defaultRootToEyes = defaultRootToNeck + defaultNeckToHeadSize + defaultHeadToEyes;

        // Compute the fitted body measures
        float wristToShoulderLeft = defaultWristToShoulderLeft;
        float wristToShoulderRight = defaultWristToShoulderRight;

        //legsize ends at the ankle and goes up to the hips
        float legSizeLeft = defaultLegSizeLeft;
        float legSizeRight = defaultLegSizeRight;

        //take the root to ankle and split the scaling uniformly across the segments

        const bool shouldersFit = true;

        float defaultUpperChestToHead = defaultUpperChestToNeckSize + defaultNeckToHeadSize;

        float upperChestToHead = defaultUpperChestToHead;
        float rootToEyes = defaultRootToEyes;
        float footToToes = defaultFootToToesSize;
        float rootToUpperChest = defaultRootToUpperChest;

        if (body != null && scaleSkeletonOptions.fittingSkeleton)
        {
            //wristToShoulderLeft = body.bodyMeasures.handToShoulderLeft - body.bodyMeasures.handToWristLeft;
            //wristToShoulderRight = body.bodyMeasures.handToShoulderRight - body.bodyMeasures.handToWristRight;

            //Debug.Log("Left arm length using tracker  = " + wristToShoulderLeft + " right: " + wristToShoulderRight);

            wristToShoulderLeft = body.bodyMeasures.handToShoulderLeft;
            wristToShoulderRight = body.bodyMeasures.handToShoulderRight;
            //Debug.Log("Left arm length using joints  = " + wristToShoulderLeft + " right: " + wristToShoulderRight);

            // Dont use LegSize but use Root Height - Foot Height and apply it uniform
            //Debug.Log("ROOT TO ANKLE LEFT: " + body.bodyMeasures.rootToAnkleLeft);
            //Debug.Log("ROOT TO ANKLE RIGHT: " + body.bodyMeasures.rootToAnkleRight);
            float offsetLegs = avatarVR.legsConnectedToHips ? 0.0f : defaultHipsToSpineSize;
            defaultLegSizeLeft -= offsetLegs;
            defaultLegSizeRight -= offsetLegs;
            legSizeLeft = body.bodyMeasures.rootToAnkleLeft;
            legSizeRight = body.bodyMeasures.rootToAnkleRight;

            // Back
            rootToEyes = body.bodyMeasures.eyesHeight - body.bodyMeasures.rootHeight;
            if (shouldersFit)
            {
                float diffShoulders = (body.bodyMeasures.shoulderCenter.y - body.bodyMeasures.rootHeight) - (defaultShouldersHeight - defaultHipsHeight);
                rootToUpperChest = defaultRootToUpperChest + diffShoulders;
                upperChestToHead = rootToEyes - rootToUpperChest - defaultHeadToEyes;
            }

            //if (scaleSkeletonOptions.scaleAvatarSkeletonFoot)
            //    footToToes = (body.bodyMeasures.footToAnkleLeft + body.bodyMeasures.footToAnkleRight) * 0.5f;
        }

        float leftLowerArmSize = wristToShoulderLeft * defaultLeftLowerArmSize / defaultWristToShoulderLeft;
        float leftUpperArmSize = wristToShoulderLeft * defaultLeftUpperArmSize / defaultWristToShoulderLeft;

        float rightLowerArmSize = wristToShoulderRight * defaultRightLowerArmSize / defaultWristToShoulderRight;
        float rightUpperArmSize = wristToShoulderRight * defaultRightUpperArmSize / defaultWristToShoulderRight;

        float leftUpperArmShoulderSize = defaultLeftUpperArmToShoulderSize;
        float rightUpperArmShoulderSize = defaultRightUpperArmToShoulderSize;

        if (scaleSkeletonOptions.fittingSkeleton && scaleSkeletonOptions.scaleAvatarSkeletonChest)
        {
            float chestSize = body.bodyMeasures.getShoulderToShoulder();
            leftUpperArmShoulderSize = chestSize * defaultLeftUpperArmToShoulderSize / defaultShoulderToShoulder;
            rightUpperArmShoulderSize = chestSize * defaultRightUpperArmToShoulderSize / defaultShoulderToShoulder;
        }

        float leftLowerLegSize = legSizeLeft * defaultLeftLowerLegSize / defaultLegSizeLeft;
        float leftUpperLegSize = legSizeLeft * defaultLeftUpperLegSize / defaultLegSizeLeft;

        float rightLowerLegSize = legSizeRight * defaultRightLowerLegSize / defaultLegSizeRight;
        float rightUpperLegSize = legSizeRight * defaultRightUpperLegSize / defaultLegSizeRight;

        //although leg2Hip is not straight vertical, this is correct. Ratio will be computed consistently using the y Components.
        float leftUpperLegToHips = legSizeLeft * defaultLeftUpperLegToHipSize / defaultLegSizeLeft;
        float rightUpperLegToHips = legSizeRight * defaultRightUpperLegToHipSize / defaultLegSizeRight;

        float rootTo = rootToEyes;
        float defaultRootTo = defaultRootToEyes;
        if (shouldersFit)
        {
            rootTo = rootToUpperChest;
            defaultRootTo = defaultRootToUpperChest;
        }

        float hipsToSpine = rootTo * defaultHipsToSpineSize / defaultRootTo;
        float spineToChest = rootTo * defaultSpineToChestSize / defaultRootTo;
        float chestToUpperChest = rootTo * defaultChestToUpperChestSize / defaultRootTo;

        if (shouldersFit)
        {
            rootTo = upperChestToHead;
            defaultRootTo = defaultUpperChestToHead;
        }

        float upperChestToNeck = rootTo * defaultUpperChestToNeckSize / defaultRootTo;
        float neckToHead = rootTo * defaultNeckToHeadSize / defaultRootTo;

        // Skeleton bones - this is where the pose transform is stored
        SkeletonBone[] sk = skeleton.ToArray();

        // For all the bones in the skeleton
        for (int i = 0; i < sk.Length; ++i)
        {
            // Get the default transform that comes from the FBX file
            Transform defaultTransform = Utils.FindDescendants(character.transform, sk[i].name);
            if (!defaultTransform)
            {
                Debug.Log("Did not find default bone transform " + sk[i].name + " in hierarchy. Defaulting to empty transform.");
            }

            // Not resizing skeleton - so set default transform and done
            if (body == null || !scaleSkeletonOptions.fittingSkeleton)
            {
                sk[i].name = defaultTransform.name;
                sk[i].position = defaultTransform.localPosition;
                sk[i].rotation = defaultTransform.localRotation;
                sk[i].scale = Vector3.one; // defaultTransform.localScale is NOT correct, defaultTransform.localPosition is already with the "scaled" positions (I guess this is the problem)
                continue;
            }

            // Get the transform computed from the body measures
            SkeletonUtils.InternalTransform measuredTransform = new SkeletonUtils.InternalTransform(defaultTransform);
            string humanName = "";
            if (SkeletonUtils.GetHumanNameByBoneName(human, sk[i].name, ref humanName))
            {
                // Left arm
                if (humanName == "LeftHand")
                {
                    //measuredTransform.localPosition = defaultTransform.localPosition.normalized * (leftLowerArmSize / avatarScaleY);

                    // Only scale world X Axis
                    Vector3 localScaledPos = measuredTransform.worldToLocalMatrixParent * new Vector4(-(leftLowerArmSize - defaultLeftLowerArmSize), 0.0f, 0.0f, 0.0f);
                    measuredTransform.localPosition.Set(
                        measuredTransform.localPosition.x + localScaledPos.x,
                        measuredTransform.localPosition.y + localScaledPos.y,
                        measuredTransform.localPosition.z + localScaledPos.z);
                }
                else if (humanName == "LeftLowerArm")
                {
                    //measuredTransform.localPosition = defaultTransform.localPosition.normalized * (leftUpperArmSize / avatarScaleY);

                    // Only scale world X Axis
                    Vector3 localScaledPos = measuredTransform.worldToLocalMatrixParent * new Vector4(-(leftUpperArmSize - defaultLeftUpperArmSize), 0.0f, 0.0f, 0.0f);
                    measuredTransform.localPosition.Set(
                        measuredTransform.localPosition.x + localScaledPos.x,
                        measuredTransform.localPosition.y + localScaledPos.y,
                        measuredTransform.localPosition.z + localScaledPos.z);
                }
                // Right arm
                else if (humanName == "RightHand")
                {
                    //measuredTransform.localPosition = defaultTransform.localPosition.normalized * (rightLowerArmSize / avatarScaleY);

                    // Only scale world X Axis
                    Vector3 localScaledPos = measuredTransform.worldToLocalMatrixParent * new Vector4(rightLowerArmSize - defaultRightLowerArmSize, 0.0f, 0.0f, 0.0f);
                    measuredTransform.localPosition.Set(
                        measuredTransform.localPosition.x + localScaledPos.x,
                        measuredTransform.localPosition.y + localScaledPos.y,
                        measuredTransform.localPosition.z + localScaledPos.z);
                }
                else if (humanName == "RightLowerArm")
                {
                    //measuredTransform.localPosition = defaultTransform.localPosition.normalized * (rightUpperArmSize / avatarScaleY);

                    // Only scale world X Axis
                    Vector3 localScaledPos = measuredTransform.worldToLocalMatrixParent * new Vector4(rightUpperArmSize - defaultRightUpperArmSize, 0.0f, 0.0f, 0.0f);
                    measuredTransform.localPosition.Set(
                        measuredTransform.localPosition.x + localScaledPos.x,
                        measuredTransform.localPosition.y + localScaledPos.y,
                        measuredTransform.localPosition.z + localScaledPos.z);
                }
                // Left clavicle
                else if (humanName == "LeftUpperArm")
                {
                    //measuredTransform.localPosition = defaultTransform.localPosition.normalized * (leftUpperArmShoulderSize / avatarScaleY);

                    // Only scale world X Axis
                    Vector3 localScaledPos = measuredTransform.worldToLocalMatrixParent * new Vector4(-(leftUpperArmShoulderSize - defaultLeftUpperArmToShoulderSize), 0.0f, 0.0f, 0.0f);
                    measuredTransform.localPosition.Set(
                        measuredTransform.localPosition.x + localScaledPos.x,
                        measuredTransform.localPosition.y + localScaledPos.y,
                        measuredTransform.localPosition.z + localScaledPos.z);
                }
                // Right clavicle
                else if (humanName == "RightUpperArm")
                {
                    //measuredTransform.localPosition = defaultTransform.localPosition.normalized * (rightUpperArmShoulderSize / avatarScaleY);

                    // Only scale world X Axis
                    Vector3 localScaledPos = measuredTransform.worldToLocalMatrixParent * new Vector4(rightUpperArmShoulderSize - defaultRightUpperArmToShoulderSize, 0.0f, 0.0f, 0.0f);
                    measuredTransform.localPosition.Set(
                        measuredTransform.localPosition.x + localScaledPos.x,
                        measuredTransform.localPosition.y + localScaledPos.y,
                        measuredTransform.localPosition.z + localScaledPos.z);
                }
                // Left leg
                else if (humanName == "LeftUpperLeg")
                {
                    //To Scale the bone between hips and the left upper leg bone(just y)
                    //Debug.Log("LEFT UPPER LEG WORLD POS: " + measuredTransform.position.ToString("F4"));
                    //Vector3 oldPos = measuredTransform.localPosition;
                    //Vector3 scaledPos = defaultTransform.localPosition.normalized * leftUpperLegToHips;
                    //measuredTransform.localPosition.Set(oldPos.x, scaledPos.y, oldPos.z);


                    // Only scale world Y Axis
                    Vector3 localScaledPos = measuredTransform.worldToLocalMatrixParent * new Vector4(0.0f, -(leftUpperLegToHips - defaultLeftUpperLegToHipSize), 0.0f, 0.0f);
                    measuredTransform.localPosition.Set(
                        measuredTransform.localPosition.x + localScaledPos.x,
                        measuredTransform.localPosition.y + localScaledPos.y,
                        measuredTransform.localPosition.z + localScaledPos.z);
                }
                else if (humanName == "LeftLowerLeg")
                {
                    //measuredTransform.localPosition = defaultTransform.localPosition.normalized * (leftUpperLegSize / avatarScaleY);

                    // Only scale world Y Axis
                    Vector3 localScaledPos = measuredTransform.worldToLocalMatrixParent * new Vector4(0.0f, -(leftUpperLegSize - defaultLeftUpperLegSize), 0.0f, 0.0f);
                    measuredTransform.localPosition.Set(
                        measuredTransform.localPosition.x + localScaledPos.x,
                        measuredTransform.localPosition.y + localScaledPos.y,
                        measuredTransform.localPosition.z + localScaledPos.z);
                }
                else if (humanName == "LeftFoot")
                {
                    //measuredTransform.localPosition = defaultTransform.localPosition.normalized * (leftLowerLegSize / avatarScaleY);

                    // Only scale world Y Axis
                    Vector3 localScaledPos = measuredTransform.worldToLocalMatrixParent * new Vector4(0.0f, -(leftLowerLegSize - defaultLeftLowerLegSize), 0.0f, 0.0f);
                    measuredTransform.localPosition.Set(
                        measuredTransform.localPosition.x + localScaledPos.x,
                        measuredTransform.localPosition.y + localScaledPos.y,
                        measuredTransform.localPosition.z + localScaledPos.z);
                }
                else if (humanName == "LeftToes")
                {
                    measuredTransform.localPosition = defaultTransform.localPosition.normalized * footToToes;
                }
                else if(humanName == "RightToes")
                {
                    measuredTransform.localPosition = defaultTransform.localPosition.normalized * footToToes;
                }
                // Right leg
                else if (humanName == "RightUpperLeg")
                {
                    //To Scale the bone between hips and the right upper leg bone (just scale y component)
                    //Vector3 oldPos = measuredTransform.localPosition;
                    //Vector3 scaledPos = defaultTransform.localPosition.normalized * rightUpperLegToHips;
                    //measuredTransform.localPosition.Set(oldPos.x, scaledPos.y, oldPos.z);

                    // Only scale world Y Axis
                    Vector3 localScaledPos = measuredTransform.worldToLocalMatrixParent * new Vector4(0.0f, -(rightUpperLegToHips - defaultRightUpperLegToHipSize), 0.0f, 0.0f);
                    measuredTransform.localPosition.Set(
                         measuredTransform.localPosition.x + localScaledPos.x,
                         measuredTransform.localPosition.y + localScaledPos.y,
                         measuredTransform.localPosition.z + localScaledPos.z);
                }
                else if (humanName == "RightLowerLeg")
                {
                    //measuredTransform.localPosition = defaultTransform.localPosition.normalized * (rightUpperLegSize / avatarScaleY);

                    // Only scale world Y Axis
                    Vector3 localScaledPos = measuredTransform.worldToLocalMatrixParent * new Vector4(0.0f, -(rightUpperLegSize - defaultRightUpperLegSize), 0.0f, 0.0f);
                    measuredTransform.localPosition.Set(
                        measuredTransform.localPosition.x + localScaledPos.x,
                        measuredTransform.localPosition.y + localScaledPos.y,
                        measuredTransform.localPosition.z + localScaledPos.z);
                }
                else if (humanName == "RightFoot")
                {
                    //measuredTransform.localPosition = defaultTransform.localPosition.normalized * (rightLowerLegSize / avatarScaleY);

                    // Only scale world Y Axis
                    Vector3 localScaledPos = measuredTransform.worldToLocalMatrixParent * new Vector4(0.0f, -(rightLowerLegSize - defaultRightLowerLegSize), 0.0f, 0.0f);
                    measuredTransform.localPosition.Set(
                        measuredTransform.localPosition.x + localScaledPos.x,
                        measuredTransform.localPosition.y + localScaledPos.y,
                        measuredTransform.localPosition.z + localScaledPos.z);
                }
                // Root-to-Neck
                else if (avatarVR.legsConnectedToHips && humanName == "Spine")
                {
                    //measuredTransform.localPosition = defaultTransform.localPosition.normalized * (hipsToSpine / avatarScaleY);

                    // Only scale world Y Axis
                    Vector3 localScaledPos = measuredTransform.worldToLocalMatrixParent * new Vector4(0.0f, hipsToSpine - defaultHipsToSpineSize, 0.0f, 0.0f);
                    measuredTransform.localPosition.Set(
                        measuredTransform.localPosition.x + localScaledPos.x,
                        measuredTransform.localPosition.y + localScaledPos.y,
                        measuredTransform.localPosition.z + localScaledPos.z);
                }
                else if (humanName == "Chest")
                {
                    //measuredTransform.localPosition = defaultTransform.localPosition.normalized * (spineToChest / avatarScaleY);

                    // Only scale world Y Axis
                    float spineOffset = avatarVR.legsConnectedToHips ? 0.0f : (hipsToSpine - defaultHipsToSpineSize);
                    Vector3 localScaledPos = measuredTransform.worldToLocalMatrixParent * new Vector4(0.0f, (spineToChest - defaultSpineToChestSize) + spineOffset, 0.0f, 0.0f);
                    measuredTransform.localPosition.Set(
                        measuredTransform.localPosition.x + localScaledPos.x,
                        measuredTransform.localPosition.y + localScaledPos.y,
                        measuredTransform.localPosition.z + localScaledPos.z);
                }
                else if (humanName == "UpperChest")
                {
                    //measuredTransform.localPosition = defaultTransform.localPosition.normalized * (chestToUpperChest / avatarScaleY);

                    // Only scale world Y Axis
                    Vector3 localScaledPos = measuredTransform.worldToLocalMatrixParent * new Vector4(0.0f, chestToUpperChest - defaultChestToUpperChestSize, 0.0f, 0.0f);
                    measuredTransform.localPosition.Set(
                        measuredTransform.localPosition.x + localScaledPos.x,
                        measuredTransform.localPosition.y + localScaledPos.y,
                        measuredTransform.localPosition.z + localScaledPos.z);
                }
                else if (humanName == "Neck")
                {
                    //measuredTransform.localPosition = defaultTransform.localPosition.normalized * (upperChestToNeck / avatarScaleY);

                    // Only scale world Y Axis
                    Vector3 localScaledPos = measuredTransform.worldToLocalMatrixParent * new Vector4(0.0f, upperChestToNeck - defaultUpperChestToNeckSize, 0.0f, 0.0f);
                    measuredTransform.localPosition.Set(
                        measuredTransform.localPosition.x + localScaledPos.x,
                        measuredTransform.localPosition.y + localScaledPos.y,
                        measuredTransform.localPosition.z + localScaledPos.z);
                }
                else if (humanName == "Head")
                {
                    //measuredTransform.localPosition = defaultTransform.localPosition.normalized * (neckToHead / avatarScaleY);

                    // Only scale world Y Axis
                    Vector3 localScaledPos = measuredTransform.worldToLocalMatrixParent * new Vector4(0.0f, neckToHead - defaultNeckToHeadSize, 0.0f, 0.0f);
                    measuredTransform.localPosition.Set(
                        measuredTransform.localPosition.x + localScaledPos.x,
                        measuredTransform.localPosition.y + localScaledPos.y,
                        measuredTransform.localPosition.z + localScaledPos.z);
                }
            }

            // Set the corresponding transforms
            //Debug.Log(sk[i].name);
            sk[i].name = defaultTransform.name;
            sk[i].position = measuredTransform.getPosition();
            sk[i].rotation = measuredTransform.getRotation();
            sk[i].scale = Vector3.one; // measuredTransform.getScale() is NOT correct, measuredTransform.getPosition() is already with the "scaled" positions (I guess this is the problem)
        }

        // Set the skeleton definition
        desc.skeleton = sk;

        //        {  // TESTING
        //           if (SkeletonUtils.GetBoneNameByHumanName(human, "Hips", ref boneName1))
        //            {
        //               foundTransform1 = Utils.FindDescendants(character.transform, boneName1);
        //              Debug.Log("setupHumanDescription::Hips position: " + foundTransform1.position.ToString("F4"));
        //           }
        //        }

        // Return
        return true;
    }

    // Returns a mapping from the unity bone names to our file bone names, along with a 
    // list of any extra bones that we want to include (see Unity HumanDescription).
    //
    // parameters:
    //    INPUT
    //    - string skeletonFile:            text file name in the Resources folder that describes a mapping from your bone names to Unity's bone names
    //    OUTPUT
    //    - List<HumanBone> human:          array of those bones in your character that are mapped to a standard Unity bone
    //    - List<SkeletonBone> skeleton:    array of the bones mapped to standard bones in your character, plus any additional bones specified
    //
    private static bool loadSkeleton(string skeletonFile, ref List<HumanBone> human, ref List<SkeletonBone> skeleton)
    {
        // load in the skeleton definition file from an asset
        TextAsset skeleton_text = Resources.Load(skeletonFile) as TextAsset;
        if (!skeleton_text)
        {
            Debug.LogError("File " + skeletonFile + " not loaded in from resources correctly. Cannot make skeleton definition.");
            return false;
        }

        // Each line has the following format:
        //   unitybonename,modelbonename
        // For those whose unitybonename is "none", these are not mapped to a standard bone but you wanted to keep them around

        // Split the file into an array of each line
        string[] lines = skeleton_text.text.Split(new char[] { '\n' });

        // For each line
        for (int i = 0; i < lines.Length; i++)
        {
            // If it is a comment, skip it
            if (lines[i][0] == '#')
            {
                continue;
            }

            // If it doesn't have a comma, skip it
            if (lines[i].IndexOf(',') == -1)
            {
                continue;
            }

            // Split the line by comma
            string[] bones = lines[i].Split(new char[] { ',' });

            // If the bone has a mapping, add it to the mappedBones list as a HumanBone
            if (bones[0].Trim() != "none")
            {
                HumanBone b = new HumanBone();
                b.boneName = bones[1].Trim();
                b.humanName = bones[0].Trim();
                b.limit.useDefaultValues = true; // bone limit to default values
                human.Add(b);
            }

            // Regardless of whether there's a mapping or not, add it to the SkeletonBone list of all the bones
            SkeletonBone sb = new SkeletonBone();
            sb.name = bones[1].Trim();
            skeleton.Add(sb);
        }
        return true;
    }
}
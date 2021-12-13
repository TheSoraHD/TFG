using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingersController : MonoBehaviour
{
    [ContextMenu("SetFingers()")]
    public void SetFingers()
    {
        AvatarController_UnityGood avatarController_UnityGood = GetComponent<AvatarController_UnityGood>();
        avatarController_UnityGood.InitFingers();
        HandController.SetFingersRotations(avatarController_UnityGood, FindObjectOfType<AvatarVR>());
    }
}

public static class HandController {

    public enum Fingers
    {
        MakeHuman,
        Josh,
        Megan,
        Bot,
        Kate
    }

    public static void SetFingersRotations(AvatarController_UnityGood controller, AvatarVR avatarVR)
    {
        if (avatarVR.controllersStyle == AvatarVR.ControllersStyle.Attached ||
            avatarVR.controllersStyle == AvatarVR.ControllersStyle.ShowAttached)
        {
            SetFingers(controller);
        }
    }

    public static void SetHands(AvatarVR avatarVR, Animator a)
    {
        Fingers type = avatarVR.AvatarStyleToFingers[avatarVR.avatarStyle];
        switch (type)
        {
            case Fingers.MakeHuman:
                SetMakeHumanHand(avatarVR, a);
                break;
            case Fingers.Josh:
                SetJoshHand(avatarVR, a);
                break;
            case Fingers.Megan:
                SetMeganHand(avatarVR, a);
                break;
            case Fingers.Bot:
                SetBotHand(avatarVR, a);
                break;
            case Fingers.Kate:
                SetKateHand(avatarVR, a);
                break;
            default:
                SetMakeHumanHand(avatarVR, a);
                break;
        }
    }

    private static void SetKateHand(AvatarVR avatarVR, Animator a)
    {
        avatarVR.handRight_model.transform.SetParent(a.GetBoneTransform(HumanBodyBones.RightHand));
        avatarVR.handRight_model.transform.localPosition = new Vector3(-0.0757f, 0.1202f, 0.0304f);
        avatarVR.handRight_model.transform.localRotation = Quaternion.Euler(-136.98f, 94.23399f, -6.542999f);

        avatarVR.handLeft_model.transform.SetParent(a.GetBoneTransform(HumanBodyBones.LeftHand));
        avatarVR.handLeft_model.transform.localPosition = new Vector3(0.0788f, 0.1206f, 0.028f);
        avatarVR.handLeft_model.transform.localRotation = Quaternion.Euler(-41.285f, 91.116f, 172.795f);
    }

    private static void SetBotHand(AvatarVR avatarVR, Animator a)
    {
        avatarVR.handRight_model.transform.SetParent(a.GetBoneTransform(HumanBodyBones.RightHand));
        avatarVR.handRight_model.transform.localPosition = new Vector3(-0.096f, 0.13f, 0.089f);
        avatarVR.handRight_model.transform.localRotation = Quaternion.Euler(-28.741f, -85.53201f, 192.674f);

        avatarVR.handLeft_model.transform.SetParent(a.GetBoneTransform(HumanBodyBones.LeftHand));
        avatarVR.handLeft_model.transform.localPosition = new Vector3(0.0927f, 0.1355f, 0.1015f);
        avatarVR.handLeft_model.transform.localRotation = Quaternion.Euler(-30.164f, 77.704f, 187.81f);
    }

    private static void SetMakeHumanHand(AvatarVR avatarVR, Animator a)
    {
        avatarVR.handRight_model.transform.SetParent(a.GetBoneTransform(HumanBodyBones.RightHand));
        avatarVR.handRight_model.transform.localPosition = new Vector3(-0.0758f, 0.138f, 0.023f);
        avatarVR.handRight_model.transform.localRotation = Quaternion.Euler(-128.451f, 92.23399f, 4.358994f);

        avatarVR.handLeft_model.transform.SetParent(a.GetBoneTransform(HumanBodyBones.LeftHand));
        avatarVR.handLeft_model.transform.localPosition = new Vector3(0.0813f, 0.1338f, 0.0257f);
        avatarVR.handLeft_model.transform.localRotation = Quaternion.Euler(-142.142f, -90.853f, -5.855988f);
    }

    private static void SetJoshHand(AvatarVR avatarVR, Animator a)
    {
        avatarVR.handRight_model.transform.SetParent(a.GetBoneTransform(HumanBodyBones.RightHand));
        avatarVR.handRight_model.transform.localPosition = new Vector3(-0.0855f, 0.13611f, 0.03282f);
        avatarVR.handRight_model.transform.localRotation = Quaternion.Euler(-134.65f, 92.905f, 3.85598f);

        avatarVR.handLeft_model.transform.SetParent(a.GetBoneTransform(HumanBodyBones.LeftHand));
        avatarVR.handLeft_model.transform.localPosition = new Vector3(0.08684845f, 0.139512f, 0.03927847f);
        avatarVR.handLeft_model.transform.localRotation = Quaternion.Euler(-45.136f, 90.203f, 173.443f);
    }

    private static void SetMeganHand(AvatarVR avatarVR, Animator a)
    {
        avatarVR.handRight_model.transform.SetParent(a.GetBoneTransform(HumanBodyBones.RightHand));
        avatarVR.handRight_model.transform.localPosition = new Vector3(-0.0789f, 0.129f, 0.0312f);
        avatarVR.handRight_model.transform.localRotation = Quaternion.Euler(-134.317f, 95.36099f, -7.868988f);

        avatarVR.handLeft_model.transform.SetParent(a.GetBoneTransform(HumanBodyBones.LeftHand));
        avatarVR.handLeft_model.transform.localPosition = new Vector3(0.0837f, 0.1291f, 0.0275f);
        avatarVR.handLeft_model.transform.localRotation = Quaternion.Euler(-41.574f, 90.658f, 173.078f);
    }

    private static void SetFingers(AvatarController_UnityGood controller)
    {
        // Right Index
        controller.RightIndexProximal.localRotation = Quaternion.Euler(20.42f, -8.739f, 11.418f);
        controller.RightIndexIntermediate.localRotation = Quaternion.Euler(71.423f, -14.123f, -30.836f);
        controller.RightIndexDistal.localRotation = Quaternion.Euler(49.375f, -9.302f, -17.397f);
        // Right Middle
        controller.RightMiddleProximal.localRotation = Quaternion.Euler(61.87f, -18.929f, -25.477f);
        controller.RightMiddleIntermediate.localRotation = Quaternion.Euler(74.8930f, -16.67f, -8.732f);
        controller.RightMiddleDistal.localRotation = Quaternion.Euler(12.461f, 0.0f, 0.0f);
        // Right Little
        controller.RightLittleProximal.localRotation = Quaternion.Euler(66.68f, -151.626f, -139.51f);
        controller.RightLittleIntermediate.localRotation = Quaternion.Euler(34.422f, -0.406f, -0.701f);
        controller.RightLittleDistal.localRotation = Quaternion.Euler(0.0f, 0.021f, 0.418f);
        // Right Ring
        controller.RightRingProximal.localRotation = Quaternion.Euler(85.064f, -36.555f, -35.21f);
        controller.RightRingIntermediate.localRotation = Quaternion.Euler(57.517f, -0.208f, 9.912f);
        controller.RightRingDistal.localRotation = Quaternion.Euler(-0.001f, 0.413f, 0.207f);
        // Right Thumb
        controller.RightThumbProximal.localRotation = Quaternion.Euler(11.959f, 14.248f, 25.318f);
        controller.RightThumbIntermediate.localRotation = Quaternion.Euler(-2.425f, -3.201f, 10.377f);
        controller.RightThumbDistal.localRotation = Quaternion.Euler(-8.216f, -0.373f, -1.683f);

        // Left Index
        controller.LeftIndexProximal.localRotation = Quaternion.Euler(23.895f, -9.691f, -18.843f);
        controller.LeftIndexIntermediate.localRotation = Quaternion.Euler(117.63f, -179.06f, -178.93f);
        controller.LeftIndexDistal.localRotation = Quaternion.Euler(56.752f, 0.157f, 0.181f);
        // Left Middle
        controller.LeftMiddleProximal.localRotation = Quaternion.Euler(54.222f, 8.785f, 3.769f);
        controller.LeftMiddleIntermediate.localRotation = Quaternion.Euler(52.731f, 11.354f, 0.598f);
        controller.LeftMiddleDistal.localRotation = Quaternion.Euler(53.304f, -0.408f, -0.494f);
        // Left Little
        controller.LeftLittleProximal.localRotation = Quaternion.Euler(80.83f, 116.418f, 106.235f);
        controller.LeftLittleIntermediate.localRotation = Quaternion.Euler(33.618f, -10.585f, -10.776f);
        controller.LeftLittleDistal.localRotation = Quaternion.Euler(11.462f, -0.08f, -0.281f);
        // Left Ring
        controller.LeftRingProximal.localRotation = Quaternion.Euler(69.004f, 22.728f, 15.365f);
        controller.LeftRingIntermediate.localRotation = Quaternion.Euler(64.201f, -0.214f, -0.236f);
        controller.LeftRingDistal.localRotation = Quaternion.Euler(10.576f, -0.009f, -0.059f);
        // Left Thumb
        controller.LeftThumbProximal.localRotation = Quaternion.Euler(19.214f, -12.195f, -19.239f);
        controller.LeftThumbIntermediate.localRotation = Quaternion.Euler(-10.341f, 2.667f, -11.941f);
        controller.LeftThumbDistal.localRotation = Quaternion.Euler(-0.011f, 0.997f, -4.696f);
    }
}
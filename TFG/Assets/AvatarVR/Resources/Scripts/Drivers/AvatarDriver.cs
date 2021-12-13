using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AvatarDriver
{
    // Whether the driver is ready to obtain measures
    protected bool ready;

    // The type of driver
    public enum AvatarDriverType
    {
        Simulation = 0,
        SteamVR = 1
    }
    public AvatarDriver.AvatarDriverType type; 

    // Elements that drive an avatar
    public GameObject head;
    public GameObject handLeft;
    public GameObject handRight;
    public GameObject pelvis;
    public GameObject footLeft;
    public GameObject footRight;

    public SteamVR_Controller.Device handLeftDevice;
    public SteamVR_Controller.Device handRightDevice;
    public SteamVR_Controller.Device pelvisDevice;
    public SteamVR_Controller.Device footRightDevice;
    public SteamVR_Controller.Device footLeftDevice;

    public AvatarDriver(GameObject obj)
    {
        Transform t;

        string[] headNames = { "HMD", "Neck", "Head", "Camera", "Camera (eye)" };
        t = Utils.FindDescendants(obj.transform, headNames);
        if (t != null)
        {
            head = t.gameObject;
        }

        string[] handLeftNames = { "LeftHand", "HandLeft", "Controller (left)" };
        t = Utils.FindDescendants(obj.transform, handLeftNames);
        if (t != null)
        {
            handLeft = t.gameObject;
        }

        string[] handRightNames = { "RightHand", "HandRight", "Controller (right)" };
        t = Utils.FindDescendants(obj.transform, handRightNames);
        if (t != null)
        {
            handRight = t.gameObject;
        }

        string[] pelvisNames = { "Pelvis", "Root", "Hips", "Tracker (root)" };
        t = Utils.FindDescendants(obj.transform, pelvisNames);
        if (t != null)
        {
            pelvis = t.gameObject;
        }

        string[] footLeftNames = { "LeftFoot", "FootLeft", "Tracker (left)" };
        t = Utils.FindDescendants(obj.transform, footLeftNames);
        if (t != null)
        {
            footLeft = t.gameObject;
        }

        string[] footRightNames = { "RightFoot", "FootRight", "Tracker (right)" };
        t = Utils.FindDescendants(obj.transform, footRightNames);
        if (t != null)
        {
            footRight = t.gameObject;
        }
    }

    public void SetActive(bool flag)
    {
        if (head)
        {
            head.SetActive(flag);
        }
        if (handLeft)
        {
            handLeft.SetActive(flag);
        }
        if (handRight)
        {
            handRight.SetActive(flag);
        }
        if (pelvis)
        {
            pelvis.SetActive(flag);
        }
        if (footLeft)
        {
            footLeft.SetActive(flag);
        }
        if (footRight)
        {
            footRight.SetActive(flag);
        }
    }

    // Ready flag

    public bool isReady()
    {
        return ready;
    }

    public void setReady(bool flag)
    {
        ready = flag;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SimulatorDriver : AvatarDriver
{
    public bool rotatingArmLeft = false;
    public bool rotatingArmRight = false;
    public bool rotatingHandLeft = false;
    public bool rotatingHandRight = false;
    public bool rotatingHead = false;

    // Driver measures and T-pose
    private AvatarBody.BodyMeasures bodyMeasures = new AvatarBody.BodyMeasures();
    private Vector3 locationHead;
    private Vector3 locationHandLeft;
    private Vector3 locationHandRight;
    private Vector3 locationPelvis;
    private Vector3 locationFootLeft;
    private Vector3 locationFootRight;

    public SimulatorDriver(GameObject obj) : base(obj)
    {
        type = AvatarDriver.AvatarDriverType.Simulation;

        // Ready to obtain measures
        ready = true;

        // Driver measures and T-pose
        bodyMeasures.footToAnkleLeft = 0.3f;
        bodyMeasures.footToAnkleRight = 0.3f;
        bodyMeasures.footHeightLeft = 0.02f;
        bodyMeasures.footHeightRight = 0.02f;
        bodyMeasures.rootHeight = 1.0f;
        bodyMeasures.waistWidth = 0.0f;
        bodyMeasures.legSizeLeft = 0.0f;
        bodyMeasures.legSizeRight = 0.0f;
        bodyMeasures.handToShoulderLeft = 0.6f;
        bodyMeasures.handToShoulderRight = 0.6f;
        bodyMeasures.handToWristLeft = 0.6f;
        bodyMeasures.handToWristRight = 0.6f;
        bodyMeasures.handToHand = 1.6f;
        bodyMeasures.headToNeck = new Vector3(-0.0f, -0.05f, -0.1f);
        bodyMeasures.eyesHeight = 1.75f;
        bodyMeasures.neckHeight = 1.68f;
        bodyMeasures.shoulderCenter = new Vector3(-0.01739717f,1.38996f,-0.09712613f);
        bodyMeasures.shoulderLeft = new Vector3(-0.1768769f,1.38996f,-0.09712613f);
        bodyMeasures.shoulderRight = new Vector3(0.1284167f,1.38996f,-0.09712613f);
        bodyMeasures.wristLeft = new Vector3(0,0,0);
        bodyMeasures.wristRight = new Vector3(0,0,0);
        bodyMeasures.neck = new Vector3(-0.0242301f,1.577881f,-0.09712613f);
        bodyMeasures.avatarAnkleHeightLeftOffset = 0;
        bodyMeasures.avatarAnkleHeightRightOffset = 0;
        bodyMeasures.depthCenterHead = -0.09999999f;
        bodyMeasures.widthCenterHead = -9.313226E-09f;

        locationHead = new Vector3(0.0f, bodyMeasures.eyesHeight, 0.1f);
        locationHandLeft = new Vector3(-bodyMeasures.handToHand * 0.5f, 1.5f, 0.0f);
        locationHandRight = new Vector3(bodyMeasures.handToHand * 0.5f, 1.5f, 0.0f);
        locationPelvis = new Vector3(0.0f, bodyMeasures.rootHeight, -0.25f);
        locationFootLeft = new Vector3(-0.2f, bodyMeasures.footHeightLeft, bodyMeasures.footToAnkleLeft);
        locationFootRight = new Vector3(0.2f, bodyMeasures.footHeightRight, bodyMeasures.footToAnkleRight);
        TPose();
    }

    public void PlaceHead()
    {
        if (head)
        {
            head.transform.localPosition = locationHead;
            head.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        }
    }

    public void PlaceHandLeft()
    {
        if (handLeft)
        {
            handLeft.transform.localPosition = locationHandLeft;
            handLeft.transform.localEulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
        }
    }

    public void PlaceHandRight()
    {
        if (handRight)
        {
            handRight.transform.localPosition = locationHandRight;
            handRight.transform.localEulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
        }
    }

    public void PlacePelvis()
    {
        if (pelvis)
        {
            pelvis.transform.localPosition = locationPelvis;
            pelvis.transform.localEulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
        }
    }

    public void PlaceFootLeft()
    {
        if (footLeft)
        {
            footLeft.transform.localPosition = locationFootLeft;
            footLeft.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        }
    }

    public void PlaceFootRight()
    {
        if (footRight)
        {
            footRight.transform.localPosition = locationFootRight;
            footRight.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        }
    }

    public void TPose()
    {
        PlaceHead();
        PlaceHandLeft();
        PlaceHandRight();
        PlacePelvis();
        PlaceFootLeft();
        PlaceFootRight();
    }
}

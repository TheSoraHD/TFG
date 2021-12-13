using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;


public class VRUtils
{
    //static Offset between Origin and the tip of the HTC Controller
    private static Vector3 tipOffset = new Vector3(0.0f, -0.015f, -0.172f); 
	
    // Get the vector representing the position
    public static HmdVector3_t GetPosition(HmdMatrix34_t matrix)
    {
        HmdVector3_t vector;

        vector.v0 = matrix.m3; //matrix.m[0][3];
        vector.v1 = matrix.m7; //matrix.m[1][3];
        vector.v2 = matrix.m11; //matrix.m[2][3];

        return vector;
    }

    // Enable/disable SteamVR tracking on descendant GameObjects
    public static void EnableTracking(GameObject obj, bool flag)
    {
        EnableTrackingRecursive(obj, flag);

        // HMD
        if (obj.GetComponent<Camera>()) 
        {
            obj.GetComponent<Camera>().enabled = flag;
        }
    }

    private static void EnableTrackingRecursive(GameObject obj, bool flag)
    {
        if (obj.GetComponent<SteamVR_TrackedObject>())
        {
            obj.GetComponent<SteamVR_TrackedObject>().enabled = flag;
        }
        Transform current = obj.transform;
        for (int i = 0; i < current.childCount; ++i)
        {
            EnableTrackingRecursive(current.GetChild(i).gameObject, flag);
        }
    }

    // Get play area
    public static HmdQuad_t getPlayArea()
    {
        HmdQuad_t room = new HmdQuad_t();
        SteamVR_PlayArea.GetBounds(SteamVR_PlayArea.Size.Calibrated, ref room);
        return room;

        //Debug.Log("v0: " + room.vCorners0.v0 + "," + room.vCorners0.v1 + "," + room.vCorners0.v2 + "\n");
        //Debug.Log("v1: " + room.vCorners1.v0 + "," + room.vCorners1.v1 + "," + room.vCorners1.v2 + "\n");
        //Debug.Log("v2: " + room.vCorners2.v0 + "," + room.vCorners2.v1 + "," + room.vCorners2.v2 + "\n");
        //Debug.Log("v3: " + room.vCorners3.v0 + "," + room.vCorners3.v1 + "," + room.vCorners3.v2 + "\n");
    }

    // Hide Chaperone
    public static void hideChaperone()
    {
        EVRSettingsError e = EVRSettingsError.None;
        if (OpenVR.Settings != null)
        {
            OpenVR.Settings.SetFloat(OpenVR.k_pch_CollisionBounds_Section, OpenVR.k_pch_CollisionBounds_ColorGammaA_Int32, 0.1f, ref e);
            OpenVR.Settings.Sync(true, ref e);
        }
    }
	
	//Get the tip of the HTC Vive Controller, "Tip"-GameObject can be given or a new one will be created
    public static Vector3 getTipPositionController(GameObject controller)
    {          
		GameObject tip = new GameObject();
		
        tip.transform.position = controller.transform.position; //copy Pos+Ori
        tip.transform.rotation = controller.transform.rotation;
        tip.transform.Translate(tipOffset);                     //translate aligned with the current rotation
		
        return tip.transform.position;                                            
    }

	//This methods calculates the intersection of the rays/planes created by the controller pose
	public static Vector3 getShoulderIntersection(GameObject fronthand, GameObject sidehand)
	{
		Vector3 vec1 = fronthand.transform.position;
		Vector3 vec2 = sidehand.transform.position;


		//get the GO forward vector
		Vector3 vec1_ext = fronthand.transform.forward; //forward is relative to GO Orientation

		//       Vector3 vec2_ext = sidehand.transform.forward;
		Vector3 vec2_ext = Vector3.right; //we can help using the global x axis orientation (thus assuming perfect global alignment)

		// Use the floor projection to avoid the 3D. Later use the height of the "t-pose hand" (Easier to see if the user height is wrong)
		vec1.y = 0;
		vec1_ext.y = 0;
		vec2.y = 0;
		vec2_ext.y = 0;

		Vector3 intersection = new Vector3();
		Utils.LineLineIntersection(out intersection, vec1, vec1_ext, vec2, vec2_ext);

		intersection.y = (sidehand.transform.position.y+fronthand.transform.position.y )/2;
		return intersection;
	}
}
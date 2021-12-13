using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class AvatarController : MonoBehaviour 
{
    public AvatarDriver driver;
    public AvatarBody body;
    public bool ikActive = false;

    public GameObject axisNeckAvatar;
    public GameObject axisRootAvatar;

    public Quaternion ikQueryJointRotation(HumanBodyBones queryJoint)
    {
        Animator animator = GetComponent<Animator>();
        Transform t = animator.GetBoneTransform(queryJoint);
        return t.localRotation;            
    }

    public Vector3 ikQueryJointPosition(HumanBodyBones queryJoint)
    {
        Animator animator = GetComponent<Animator>();
        Transform t = animator.GetBoneTransform(queryJoint);
        return t.position;
    }

    public float ikQueryDistanceToJoint(HumanBodyBones queryJoint)
    {
        Animator animator = GetComponent<Animator>();
        Transform t = animator.GetBoneTransform(queryJoint);

        float dist1 = float.MaxValue;
        if (driver.handLeft)
        {
            dist1 = (driver.handLeft.transform.position - t.position).magnitude;
        }
        float dist2 = float.MaxValue;
        if (driver.handRight)
        {
            dist2 = (driver.handRight.transform.position - t.position).magnitude;
        }

        return Mathf.Min(dist1, dist2);
    }

    public void setupAxisNeckAvatar()
    {
        axisNeckAvatar = GameObject.Instantiate(Resources.Load("Prefabs/Joint", typeof(GameObject))) as GameObject;

        axisNeckAvatar.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        axisNeckAvatar.transform.localPosition = new Vector3(0.0f, 1.0f, 0.5f);

        GameObject sphere = axisNeckAvatar.transform.GetChild(0).gameObject;
        if (sphere.GetComponent<Renderer>() != null)
        {
            Renderer rend = sphere.GetComponent<Renderer>();
            rend.material.color = Color.red;
        }
    }

    public void setupAxisRootAvatar()
    {
        axisRootAvatar = GameObject.Instantiate(Resources.Load("Prefabs/Joint", typeof(GameObject))) as GameObject;

        axisRootAvatar.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        axisRootAvatar.transform.localPosition = new Vector3(0.0f, 1.0f, 1.5f);

        GameObject sphere = axisRootAvatar.transform.GetChild(0).gameObject;
        if (sphere.GetComponent<Renderer>() != null)
        {
            Renderer rend = sphere.GetComponent<Renderer>();
            rend.material.color = Color.green;
        }

    }
    public void locateAxisNeckAvatar(Vector3 position, Vector3 Yaxis)
    {
        axisNeckAvatar.transform.position = position;
        axisNeckAvatar.transform.localPosition = new Vector3(0.0f, 1.0f, 0.5f);
        axisNeckAvatar.transform.rotation = Quaternion.FromToRotation(Vector3.up, Yaxis);
    }

    public void locateAxisRootAvatar(Vector3 position, Vector3 Yaxis)
    {
        axisRootAvatar.transform.position = position;
        axisRootAvatar.transform.localPosition = new Vector3(0.0f, 1.0f, 1.5f);
        axisRootAvatar.transform.rotation = Quaternion.FromToRotation(Vector3.up, Yaxis);
    }

    public void recordTrackerInfo(Vector3 Yaxis)
    {
        Animator animator = GetComponent<Animator>();
        Vector3 pos_hips = animator.GetBoneTransform(HumanBodyBones.Hips).position;

        float distanceTrackerToRoot = Vector3.Distance(body.jointRoot.transform.position, driver.pelvis.transform.position);

        string line = System.DateTime.Now.ToString() + ",";

        line += body.jointNeck.transform.position.x + "," +  body.jointNeck.transform.position.y + "," + body.jointNeck.transform.position.z + ",";
        line += body.jointNeck.transform.eulerAngles.x + "," + body.jointNeck.transform.eulerAngles.y + "," + body.jointNeck.transform.eulerAngles.z + ",";
        line += body.jointRoot.transform.position.x + "," +  body.jointRoot.transform.position.y + "," + body.jointRoot.transform.position.z + ",";
        line += body.jointRoot.transform.eulerAngles.x + "," + body.jointRoot.transform.eulerAngles.y + "," + body.jointRoot.transform.eulerAngles.z + ",";
        line += Yaxis.x + "," + Yaxis.y + "," + Yaxis.z + ",";
        line += pos_hips.x + "," + pos_hips.y + "," + pos_hips.z + ",";

        // distance tracker root
        line+= distanceTrackerToRoot + "," + Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), Yaxis) + ",";
        line += System.Environment.NewLine;
        System.IO.File.AppendAllText(".log_trackers.csv", line);
        Debug.Log(line);
     }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SkeletonUtils
{
    // struct InternalTransform (can't instantiate a Transform without a GameObject)
    public struct InternalTransform
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
        public Vector3 position;
        public Matrix4x4 worldToLocalMatrixParent;

        public InternalTransform(Transform t)
        {
            if (t)
            {
                localPosition = new Vector3(t.localPosition.x, t.localPosition.y, t.localPosition.z);
                localRotation = new Quaternion(t.localRotation.x, t.localRotation.y, t.localRotation.z, t.localRotation.w);
                localScale = new Vector3(t.localScale.x, t.localScale.y, t.localScale.z);
                position = t.position;
                if (t.parent != null) worldToLocalMatrixParent = t.parent.worldToLocalMatrix;
                else worldToLocalMatrixParent = Matrix4x4.identity;
            }
            else
            {
                localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                localScale = new Vector3(1.0f, 1.0f, 1.0f);
                position = Vector3.zero;
                worldToLocalMatrixParent = Matrix4x4.identity;
            }   
        }

        public Vector3 getPosition()
        {
            return new Vector3(localPosition.x, localPosition.y, localPosition.z);
        }

        public Quaternion getRotation()
        {
            return new Quaternion(localRotation.x, localRotation.y, localRotation.z, localRotation.w);
        }

        public Vector3 getScale()
        {
            return new Vector3(localScale.x, localScale.y, localScale.z);
        }
    }

    // class SkeletonPose (i.e. a list of rotations for skeleton joints)
    public class SkeletonPose : IEnumerable<SkeletonPose.HumanBodyBoneQuaternionPair>
    {
        private Dictionary<HumanBodyBones, Quaternion> pose;

        public SkeletonPose()
        {
            pose = new Dictionary<HumanBodyBones, Quaternion>();
        }

        public void Add(HumanBodyBones name, Quaternion q)
        {
            pose.Add(name, q);
        }

        public Quaternion Get(HumanBodyBones name)
        {
            Quaternion q = Quaternion.identity;
            if (pose.TryGetValue(name, out q))
            {
                return q;
            }
            return Quaternion.identity;
        }
		
        public struct HumanBodyBoneQuaternionPair
        {
            public HumanBodyBones name;
            public Quaternion rotation;

            public HumanBodyBoneQuaternionPair(KeyValuePair<HumanBodyBones, Quaternion> pair)
            {
                name = pair.Key;
                rotation = pair.Value;
            }
        }

        public IEnumerator<HumanBodyBoneQuaternionPair> GetEnumerator()
        {
            foreach (KeyValuePair<HumanBodyBones, Quaternion> item in pose)
            {
                yield return new HumanBodyBoneQuaternionPair(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    // Finds the parent HumanBone in a list, given the humanName of a HumanBone
    public static bool GetParentHumanBone(List<HumanBone> bones, string humanName, ref HumanBone parent)
    {
        string parentName = "";
        if (humanName == "Hips")
        {
            parentName = "";
        }
        else if (humanName == "Spine")
        {
            parentName = "Hips";
        }
        else if (humanName == "Chest")
        {
            parentName = "Spine";
        }
        else if (humanName == "UpperChest")
        {
            parentName = "Chest";
        }
        else if (humanName == "Neck")
        {
            parentName = "UpperChest";
        }
        else if (humanName == "Head")
        {
            parentName = "Neck";
        }
        if (humanName == "LeftShoulder")
        {
            parentName = "UpperChest";
        }
        else if (humanName == "LeftUpperArm")
        {
            parentName = "LeftShoulder";
        }
        else if (humanName == "LeftLowerArm")
        {
            parentName = "LeftUpperArm";
        }
        else if (humanName == "LeftHand")
        {
            parentName = "LeftLowerArm";
        }
        else if (humanName == "LeftUpperLeg")
        {
            parentName = "Hips";
        }
        else if (humanName == "LeftLowerLeg")
        {
            parentName = "LeftUpperLeg";
        }
        else if (humanName == "LeftFoot")
        {
            parentName = "LeftLowerLeg";
        }
        else if (humanName == "LeftToes")
        {
            parentName = "LeftFoot";
        }
        else if (humanName == "RightShoulder")
        {
            parentName = "UpperChest";
        }
        else if (humanName == "RightUpperArm")
        {
            parentName = "RightShoulder";
        }
        else if (humanName == "RightLowerArm")
        {
            parentName = "RightUpperArm";
        }
        else if (humanName == "RightHand")
        {
            parentName = "RightLowerArm";
        }
        else if (humanName == "RightUpperLeg")
        {
            parentName = "Hips";
        }
        else if (humanName == "RightLowerLeg")
        {
            parentName = "RightUpperLeg";
        }
        else if (humanName == "RightFoot")
        {
            parentName = "RightLowerLeg";
        }
        else if (humanName == "RightToes")
        {
            parentName = "RightFoot";
        }
        else
        {
            parentName = "";
        }

        // Unrecognised bone or it does not have any parent
        if (parentName == "")
        {
            return false;
        }

        // Parent is not in the list
        if (!bones.Exists(x => (x.humanName == parentName)))
        {
            return false;
        }

        // Return parent
        parent = bones.Find(x => (x.humanName == parentName));
        return true;
    }

    // Finds the boneName of a HumanBone in a list, given its humanName
    public static bool GetBoneNameByHumanName(List<HumanBone> bones, string humanName, ref string boneName)
    {
        // HumanBone not in list
        if (!bones.Exists(x => (x.humanName == humanName)))
        {
            return false;
        }

        // Return the boneName
        boneName = bones.Find(x => (x.humanName == humanName)).boneName;
        return true;
    }

    // Finds the humanName of a HumanBone in a list, given its boneName
    public static bool GetHumanNameByBoneName(List<HumanBone> bones, string boneName, ref string humanName)
    {
        // HumanBone not in list
        if (!bones.Exists(x => (x.boneName == boneName)))
        {
            return false;
        }

        // Return the humanName
        humanName = bones.Find(x => (x.boneName == boneName)).humanName;
        return true;
    }
        
    // Extract skeleton pose from a GameObject
    public static SkeletonPose GetSkeletonPose(GameObject obj, List<HumanBone> human)
    {
        SkeletonPose pose = new SkeletonPose();

        foreach (HumanBodyBones humanName in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            string boneName = "";
            if (!SkeletonUtils.GetBoneNameByHumanName(human, humanName.ToString(), ref boneName))
            {
                continue;
            }

            Transform foundTransform = Utils.FindDescendants(obj.transform, boneName);
            if (!foundTransform)
            {
                continue;
            }

            Quaternion r = foundTransform.localRotation;
            pose.Add(humanName, r);
        }

        return pose;
    }

    // Extract skeleton sizes from a GameObject
    public static Dictionary<HumanBodyBones, float> GetSkeletonSizes(GameObject obj, List<HumanBone> human, float scale = 1.0f)
    {
        Dictionary<HumanBodyBones, float> sizes = new Dictionary<HumanBodyBones, float>();

        foreach (HumanBodyBones humanName in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            string boneName = "";
            if (!SkeletonUtils.GetBoneNameByHumanName(human, humanName.ToString(), ref boneName))
            {
                continue;
            }

            Transform foundTransform = Utils.FindDescendants(obj.transform, boneName);
            if (!foundTransform)
            {
                continue;
            }

            float size = foundTransform.localPosition.magnitude * scale;
            sizes.Add(humanName, size);
        }

        return sizes;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class VRPlayerMovement : MonoBehaviour
{
    public float speed = 1.0f;
    public SteamVR_Action_Vector2 input;
    public Transform hmdTransform; //TO-DO: VR Fallback

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = hmdTransform.TransformDirection(new Vector3(input.axis.x, 0, input.axis.y));
        transform.position += speed * Time.deltaTime * Vector3.ProjectOnPlane(direction, Vector3.up);
    }
}

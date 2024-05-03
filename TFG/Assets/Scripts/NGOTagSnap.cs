using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NGOTagSnap : NetworkBehaviour
{
    public string tagToSnap;
    public Vector3 positionOffset;
    public Quaternion localRotation;

    [Rpc(SendTo.Everyone)]
    public void TagSnapRpc(Vector3 pos)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        transform.position = pos + positionOffset;
        transform.rotation = localRotation;
    }
}

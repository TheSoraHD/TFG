using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEditor;

public class NGOWheel : NetworkBehaviour
{
    public string tagToSnap;
    public Vector3 positionOffset;
    public Quaternion localRotation;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == tagToSnap) {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            transform.position = collision.transform.position + positionOffset;
            transform.rotation = localRotation;
        }
    }
}
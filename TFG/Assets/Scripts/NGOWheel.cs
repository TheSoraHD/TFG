using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEditor;

[RequireComponent(typeof(NGOTagSnap))]
public class NGOWheel : NetworkBehaviour
{
    private NGOTagSnap m_snap;
    public float drillTimer = 0.0f;
    public bool snapped = false;

    private void Start()
    {
        m_snap = GetComponent<NGOTagSnap>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == m_snap.tagToSnap && drillTimer > 100.0f) {
            m_snap.TagSnapRpc(collision.transform.position);
            snapped = true;
        }
    }
}
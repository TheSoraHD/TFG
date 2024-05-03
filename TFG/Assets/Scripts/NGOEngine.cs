using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEditor;

[RequireComponent(typeof(NGOTagSnap))]
public class NGOEngine : NetworkBehaviour
{

    private NGOTagSnap m_snap;

    private void Start()
    {
        m_snap = GetComponent<NGOTagSnap>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == m_snap.tagToSnap)
            m_snap.TagSnapRpc(collision.transform.position);
    }
}
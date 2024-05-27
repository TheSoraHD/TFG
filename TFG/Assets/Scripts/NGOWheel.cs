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
    private float drillTimer = 0.0f;
    private bool isInSlot;

    public bool snapped = false;

    private void Start()
    {
        m_snap = GetComponent<NGOTagSnap>();
        isInSlot = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!snapped) {
            if (collision.gameObject.tag == "Drill" && isInSlot) {
                drillTimer += Time.deltaTime;
            }
            if (collision.gameObject.tag == m_snap.tagToSnap) {
                Debug.Log("Wheel inside the slot!");
                isInSlot = true;
                if (drillTimer > 100.0f) {
                    m_snap.TagSnapRpc(collision.transform.position);
                    snapped = true;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == m_snap.tagToSnap)
        {
            isInSlot = false;
            Debug.Log("Wheel outside the slot!");
        }
    }
}
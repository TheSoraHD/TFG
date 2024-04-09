using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NGOWheel : NetworkBehaviour
{
    public Collider carSlotCollider;
    public bool colorSpawnedObject = true;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == carSlotCollider)
        {
            ContactPoint contact = collision.contacts[0];
            RaycastHit hit;

            float backTrackLength = 1f;
            Ray ray = new Ray(contact.point - (-contact.normal * backTrackLength), -contact.normal);
            if (collision.collider.Raycast(ray, out hit, 2))
            {

            }
            Debug.DrawRay(ray.origin, ray.direction, Color.cyan, 5, true);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PaintbrushMechanic : NetworkBehaviour
{

    public AudioSource sound;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "SpaceshipPart")
        {
            collision.collider.GetComponent<NGOSpaceshipPart>().ChangeColorRpc();

            sound.Play();
        }
    }
}

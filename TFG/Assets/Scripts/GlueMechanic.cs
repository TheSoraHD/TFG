using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GlueMechanic : NetworkBehaviour
{

    public AudioSource sound;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "SpaceshipPart") Mark(collision.gameObject, false);
        else if (collision.gameObject.tag == "Spaceship") Mark(collision.gameObject, true);
    }

    // Mark function only activates if the gameObject to mark is an spaceship part or the spaceship itself.
    void Mark(GameObject o, bool isParent)
    {
        NGOSpaceshipPart sp;
        if (isParent) sp = o.transform.Find("Body").GetComponent<NGOSpaceshipPart>();
        else sp = o.GetComponent<NGOSpaceshipPart>();

        if (!sp.marked)
        {
            sp.marked = true;
            sound.Play();
        }
    }
}

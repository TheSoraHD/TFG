using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class IntroButton : MonoBehaviour
{
    public bool pushed;

    // Start is called before the first frame update
    void Start()
    {
        pushed = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.root.tag == "Player") pushed = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HanoiPlatformCollider : MonoBehaviour
{
    // platform identifier
    public int platformID;

    // pieceID on the platform
    // pieceAttached = -1 means there's no piece on the platform
    public int pieceAttached = -1;

    public AudioSource place;
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "HanoiPiece")
        {
            HanoiPiece hp = collision.gameObject.GetComponent<HanoiPiece>();
            // if there's already a piece on the platform
            if (pieceAttached != -1) hp.Reset();
            else
            {
                collision.transform.position = new Vector3(gameObject.transform.position.x, 1.25f, gameObject.transform.position.z);

                // update hanoi piece status
                hp.platformAttached = platformID;
                hp.last_position = collision.transform.position;
                hp.above = -1;
                hp.below = -1;
                hp.numselected = 0;

                pieceAttached = hp.numPlayers;

                if (!hp.start && !hp.reset) place.Play();
                if (hp.start) hp.start = false;
                if (hp.reset) hp.reset = false;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "HanoiPiece")
        {
            HanoiPiece hp = collision.gameObject.GetComponent<HanoiPiece>();
            if (pieceAttached == hp.numPlayers)
            {
                pieceAttached = -1;
            }
        }
    }
}

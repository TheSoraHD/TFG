using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HanoiPlaneCollider : MonoBehaviour
{
    
    public int planeID;

    // number of hanoi pieces colliding at the same time with the plane
    // it always should be 0 or 1
    public bool piece;

    public AudioSource place;


    // Start is called before the first frame update
    void Start()
    {
        //if (planeID == 1) piece = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "HanoiPiece")
        {
            HanoiPiece hp = collision.gameObject.GetComponent<HanoiPiece>();
            // if there is already a piece touching the plane
            if (piece)
            {
                hp.Reset();
            }
            else
            {
                collision.transform.position = new Vector3(gameObject.transform.position.x, 1.25f, gameObject.transform.position.z);

                // update hanoi piece status
                hp.planeAttached = planeID;
                hp.last_position = collision.transform.position;
                hp.above = -1;
                hp.below = -1;
                hp.numselected = 0;

                piece = true;

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
            piece = false;
        }
    }
}

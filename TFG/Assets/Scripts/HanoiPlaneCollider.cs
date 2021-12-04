using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HanoiPlaneCollider : MonoBehaviour
{

    // number of hanoi pieces colliding at the same time with the plane
    // it always should be 0 or 1
    public bool num_pieces;
    public int planeID;

    // Start is called before the first frame update
    void Start()
    {
        if (planeID == 1) num_pieces = true;
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
            if (num_pieces)
            {
                hp.numselected = 0;
                collision.transform.position = hp.last_position;
            }
            else
            {

                collision.transform.position = new Vector3(gameObject.transform.position.x, (float) 0.76, gameObject.transform.position.z);


                // update hanoi piece status
                hp.planeAttached = planeID;
                hp.last_position = collision.transform.position;
                hp.above = -1;
                hp.below = -1;
                hp.numselected = 0;


                num_pieces = true;
            }
            //Debug.Log(gameObject.name + " " + collision.gameObject.name);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "HanoiPiece")
        {
            HanoiPiece hp = collision.gameObject.GetComponent<HanoiPiece>();

            //Debug.Log(gameObject.name + " " + collision.gameObject.name);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "HanoiPiece")
        {
            num_pieces = false;
            //Debug.Log(gameObject.name + " " + collision.gameObject.name);
        }
    }
}

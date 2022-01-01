using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HanoiPiece : MonoBehaviour
{
    // num players needed to move the piece
    public int numPlayers;
    // num current players selecting the piece
    public int numselected;
    // plane which piece is attached to 
    public int planeAttached;

    public int above;
    public int below;

    public float speed;

    public AudioSource place;
    public AudioSource fail;

    // position of the piece when its attached to a plane
    public Vector3 last_position;

    // Start is called before the first frame update
    void Start()
    {
        last_position = gameObject.transform.position;
        above = -1;
        planeAttached = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (canBeMoved())
        {
            //Debug.Log("BITCH");
        }
    }

    // Manage cases when two hanoi pieces collide 
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "HanoiPiece")
        {
            HanoiPiece hp = collision.gameObject.GetComponent<HanoiPiece>();

            // this will always be performed from the smallest piece of the collision
            if (numPlayers < hp.numPlayers)
            {
                //Debug.Log(gameObject.name);

                // if the piece is below, then the biggest piece will return to its last platform
                if (gameObject.transform.position.y < collision.transform.position.y)
                {
                    Debug.Log("Piece fail 1");
                    fail.Play();
                    hp.Reset();
                }
                // update if the biggest piece is fixed to the platform and no other piece is above
                else if (hp.numselected < hp.numPlayers)
                {

                    if (hp.above == -1)
                    {
                        gameObject.transform.position = new Vector3(collision.transform.position.x, collision.transform.position.y + 1.0f, collision.transform.position.z);

                        // update biggest piece status
                        hp.above = numPlayers;

                        // update smallest piece status
                        planeAttached = hp.planeAttached;
                        last_position = gameObject.transform.position;
                        above = -1;
                        below = hp.numPlayers;
                        numselected = 0;

                        Debug.Log("Piece placed");
                        place.Play();
                    }
                    else
                    {
                        Debug.Log("Piece fail 2");
                        fail.Play();
                        Reset();
                    }
                }
            }
        }
        // in case the piece is collisioning with the floor, just return to its last platform
        else if (collision.gameObject.tag == "Floor")
        {
            Debug.Log("Piece fail 3");
            fail.Play();
            Reset();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // update status
        if (collision.gameObject.tag == "HanoiPiece")
        {
            above = -1;
        }
    }

    bool canBeMoved()
    {
        return numselected == numPlayers && above == -1;
    }

    [PunRPC]
    void DeactivateMovement()
    {
        Destroy(gameObject.GetComponent<PhotonRigidbodyView>());
        Destroy(gameObject.GetComponent<Rigidbody>());
    }

    // returns the piece to the last position and disables its movement until it is picked up again
    public void Reset()
    {
        numselected = 0;
        gameObject.transform.position = last_position;
    }
}

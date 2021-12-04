using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HanoiPiece : MonoBehaviour
{
    // num players needed to move the piece
    public int numPlayers;
    // num current players selecting the piece
    public int numselected;
    // plane which piece is attached to 
    public int planeAttached;

    public float speed;

    // position of the piece when its attached to a plane
    public Vector3 last_position;

    // Start is called before the first frame update
    void Start()
    {
        last_position = gameObject.transform.position;
        planeAttached = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (canBeMoved())
        {
            Debug.Log("BITCH");
        }
    }

    // Manage cases when two hanoi pieces collide 
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "HanoiPlane")
        {
            Debug.Log(gameObject.name);
            // it will always perform the collision from the piece below if it is not in movement
            if (collision.transform.position.y > gameObject.transform.position.y && numselected < numPlayers)
            {
                Debug.Log(gameObject.name);

                HanoiPiece hp = collision.gameObject.GetComponent<HanoiPiece>();

                // if the piece above is bigger, then this piece will return to its original platform
                if (hp.numPlayers > numPlayers)
                {
                    hp.numselected = 0;
                    collision.transform.position = hp.last_position;
                }
                else
                {
                    collision.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 1.0f, gameObject.transform.position.z);

                    // update hanoi piece status
                    hp.planeAttached = planeAttached;
                    hp.last_position = collision.transform.position;
                    hp.numselected = 0;
                }
            }

        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "HanoiPlane")
        {

        }
    }

    bool canBeMoved()
    {
        return numselected == numPlayers;
    }

    void Select()
    {

    }

}

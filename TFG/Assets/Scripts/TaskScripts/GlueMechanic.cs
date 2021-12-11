using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlueMechanic : MonoBehaviour
{

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "SpaceshipPart")
        {
            if (IsParent(collision.gameObject)) Mark(collision.gameObject, true);
            else Mark(collision.gameObject, false);
        }
    }

    // This function checks whether the gameObject is a combination of spaceship parts or not 
    bool IsParent(GameObject o)
    {
        bool res = true;
        for (int i = 0; i < o.transform.childCount; ++i)
            if (o.transform.GetChild(i).tag != "SpaceshipPart") res = false;

        return res;
    }

    // Mark function only activates if the gameObject to mark is an spaceship part.
    void Mark(GameObject o, bool isParent)
    {
        SpaceshipPart sp;
        if (isParent) sp = o.transform.Find("body").GetComponent<SpaceshipPart>();
        else sp = o.GetComponent<SpaceshipPart>();
        sp.marked = true;
    }

}

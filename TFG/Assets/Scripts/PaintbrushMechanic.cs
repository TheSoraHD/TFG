using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintbrushMechanic : MonoBehaviour
{

    public Material[] material;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "SpaceshipPart")
        {
            Debug.Log(collision.collider.gameObject.transform.parent.name);

            SpaceshipPart sp = collision.collider.transform.parent.gameObject.GetComponent<SpaceshipPart>();
            Renderer rend = collision.collider.gameObject.GetComponent<Renderer>();

            ++sp.material_state;
            if (sp.material_state == material.Length) sp.material_state = 0;
            rend.sharedMaterial = material[sp.material_state];

        }
    }


}

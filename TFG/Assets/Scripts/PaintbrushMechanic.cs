using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PaintbrushMechanic : MonoBehaviour
{

    public PhotonView photonView;
    public Material[] material;

    // Start is called before the first frame update
    void Start()
    {
        photonView = gameObject.GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "SpaceshipPart")
        {
            photonView.RPC("ChangeColor", RpcTarget.All, collision.gameObject.name);
        }
    }

    [PunRPC]
    void ChangeColor(string name)
    {
        Debug.Log(name);
        GameObject obj = GameObject.Find(name);

        SpaceshipPart sp = obj.GetComponent<SpaceshipPart>();
        Renderer rend = obj.transform.GetChild(0).gameObject.GetComponent<Renderer>();

        ++sp.material_state;
        if (sp.material_state == material.Length) sp.material_state = 0;
        rend.sharedMaterial = material[sp.material_state];
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CastlePart : MonoBehaviour
{
    public GameObject platform_object;
    public GameObject[] requirements;

    public PhotonView photonView;

    public AudioSource audio;

    // status of the CastlePiece
    // state = 0 --> not displayed on platform
    // state = 1 --> displayed on platform
    // state = 2 --> placed
    public int state;

    // Start is called before the first frame update
    void Start()
    {
        state = 0;
    }

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(platform_object.transform.position, transform.position);
        if (state == 0)
        {
            if (dist < 6.0f && CheckRequirements())
            {
                platform_object.SetActive(true);
                state = 1;
            }
        }
        else if (state == 1)
        {
            if (dist >= 6.0f)
            {
                platform_object.SetActive(false);
                state = 0;
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                photonView.RPC("ChangeToState2", RpcTarget.All);
            }
        }
    }

    bool CheckRequirements()
    {
        bool res = true;
        for (int i = 0; i < requirements.Length; ++i)
        {
            res = res && requirements[i].GetComponent<CastlePart>().state == 2;
        }
        return res;
    }

    [PunRPC]
    void ChangeToState2()
    {
        audio.Play();
        state = 2;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        if (gameObject.name == "castle_crown") DisableCastleCrown();
        else
        {
            gameObject.GetComponent<Renderer>().enabled = false;
            gameObject.GetComponent<Collider>().enabled = false;
        }
    }

    void DisableCastleCrown()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).GetComponent<Renderer>().enabled = false;
            transform.GetChild(i).GetComponent<Collider>().enabled = false;
        }
    }
}

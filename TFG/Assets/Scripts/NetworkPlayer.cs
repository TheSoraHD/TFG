using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviour
{
    public Transform model;
    private PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        //model = GameObject.Find("PlayerAvatar").transform;
        photonView = GetComponent<PhotonView>();
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            //gameObject.SetActive(false);
            //MapPosition(model);
        }
    }

    void MapPosition(Transform target)
    {

    }
}

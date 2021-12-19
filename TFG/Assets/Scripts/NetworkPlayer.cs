using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviour
{
    public Transform model;
    
    private PhotonView photonView;

    private Transform modelPlayer;

    // Start is called before the first frame update
    void Start()
    {
        //model = GameObject.Find("PlayerAvatar").transform;
        photonView = GetComponent<PhotonView>();
        DontDestroyOnLoad(gameObject);

        Transform player = GameObject.Find("/Cube").transform;
        modelPlayer = GameObject.Find("/Cube/Model").transform;

        player.position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            model.gameObject.SetActive(false);

            MapPosition(model, modelPlayer);
        }
    }

    void MapPosition(Transform target, Transform rigTrasnform)
    {
        //transform.position = rigTrasnform.position;
        target.position = rigTrasnform.position;
    }
}

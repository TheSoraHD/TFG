using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviour
{
    public Transform model;
    
    private PhotonView photonView;

    private Transform modelPlayer;

    public PhotonView levelLoader;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        DontDestroyOnLoad(gameObject);

        Transform player = GameObject.Find("/Cube").transform;
        modelPlayer = GameObject.Find("/Cube/Model").transform;

        player.position = transform.position;

        levelLoader = GameObject.Find("_LevelLoader").GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            model.gameObject.SetActive(false);

            MapPosition(model, modelPlayer);
        }

        if (photonView.Owner.IsMasterClient) levelLoader.RPC("LevelUpdate", RpcTarget.All);

        if (Input.GetKeyDown(KeyCode.E) && photonView.Owner.IsMasterClient) levelLoader.RPC("ChangePlatformActivity", RpcTarget.All);
    }

    void MapPosition(Transform target, Transform rigTrasnform)
    {
        //transform.position = rigTrasnform.position;
        target.position = rigTrasnform.position;
    }
}

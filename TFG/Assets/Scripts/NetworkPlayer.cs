using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviour
{
    public Transform model;
    
    private PhotonView photonView;
    private Transform modelPlayer;

    public LoadLevel levelLoader;

    public Material materialAssigned;


    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        photonView.Owner.TagObject = gameObject;
        DontDestroyOnLoad(gameObject);

        Transform player = GameObject.Find("/Cube").transform;
        modelPlayer = GameObject.Find("/Cube/Model").transform;

        player.position = transform.position;

        levelLoader = GameObject.Find("_LevelLoader").GetComponent<LoadLevel>();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            model.gameObject.SetActive(false);

            MapPosition(model, modelPlayer);
        }

        if (photonView.Owner.IsMasterClient)
        {
            if (!levelLoader.platformActive) levelLoader.CheckConditions();
            else levelLoader.LevelUpdate();
        }

    }

    void MapPosition(Transform target, Transform rigTrasnform)
    {
        //transform.position = rigTrasnform.position;
        target.position = rigTrasnform.position;
    }
}

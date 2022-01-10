using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviour
{
    public Transform model;
    
    private PhotonView photonView;
    private Transform modelPlayer;

    public LevelLoader levelLoader;

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

        levelLoader = GameObject.Find("_LevelLoader").GetComponent<LevelLoader>();
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

            if (Input.GetKeyDown(KeyCode.Alpha0)) levelLoader.ChangeLevel(0);
            if (Input.GetKeyDown(KeyCode.Alpha1)) levelLoader.ChangeLevel(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) levelLoader.ChangeLevel(2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) levelLoader.ChangeLevel(3);
            if (Input.GetKeyDown(KeyCode.Alpha4)) levelLoader.ChangeLevel(4);
        }

    }

    void MapPosition(Transform target, Transform rigTrasnform)
    {
        //transform.position = rigTrasnform.position;
        target.position = rigTrasnform.position;
    }
}

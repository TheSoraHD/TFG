using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NGONetworkPlayer : NetworkBehaviour
{

    public Transform playerRoot;
    public Transform playerHead;
    public Transform playerLeftHand;
    public Transform playerRightHand;

    public Renderer[] meshToDisable;


    public Transform model;
    public Transform modelPlayer;
    public NGOLevelLoader levelLoader;
    //private PhotonView photonView;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            foreach (var item in meshToDisable)
            {
                item.enabled = false;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //photonView = GetComponent<PhotonView>();
        //photonView.Owner.TagObject = gameObject;
        DontDestroyOnLoad(gameObject);

        //TO-DO: QUÉ COJONES?
        //Transform player = GameObject.Find("/NetworkPlayer").transform;
        //modelPlayer = GameObject.Find("/NetworkPlayer/Model").transform;
        //player.position = transform.position;

        levelLoader = GameObject.Find("_LevelLoader").GetComponent<NGOLevelLoader>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            /*
            playerRoot.position = VRRigReferences.Singleton.playerRoot.position;
            playerRoot.rotation = VRRigReferences.Singleton.playerRoot.rotation;

            playerHead.position = VRRigReferences.Singleton.playerHead.position;
            playerHead.rotation = VRRigReferences.Singleton.playerHead.rotation;

            playerLeftHand.position = VRRigReferences.Singleton.playerLeftHand.position;
            playerLeftHand.rotation = VRRigReferences.Singleton.playerLeftHand.rotation;

            playerRightHand.position = VRRigReferences.Singleton.playerRightHand.position;
            playerRightHand.rotation = VRRigReferences.Singleton.playerRightHand.rotation;
            */
        }
        /*
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
        */
    }
    void MapPosition(Transform target, Transform rigTransform)
    {
        target.position = rigTransform.position;
        target.rotation = rigTransform.rotation;

    }

    private Material GetMaterial(int index)
    {
        if (index == 1) return Resources.Load<Material>("Materials/Yellow");
        else if (index == 2) return Resources.Load<Material>("Materials/Red");
        else return Resources.Load<Material>("Materials/Blue");
    }
}


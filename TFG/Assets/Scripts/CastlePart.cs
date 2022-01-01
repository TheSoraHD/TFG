﻿using System.Collections;
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
    // state = 0 --> idle
    // state = 1 --> picked up and displayed on platform
    // state = 2 --> placed
    public int state;

    // Start is called before the first frame update
    void Start()
    {
        state = 0;
        //audio = GetComponent<AudioSource>();
        //photonView.RPC("AssignPartToPlayer", RpcTarget.All);
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
        else
        {
            if (dist >= 6.0f)
            {
                platform_object.SetActive(false);
                state = 0;
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                audio.Play();
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
        state = 2;
        gameObject.SetActive(false);
    }

    [PunRPC]
    void AssignPartToPlayer()
    {
        Renderer rend;
        if (gameObject.name == "castle_crown") rend = transform.GetChild(0).gameObject.GetComponent<Renderer>();
        else rend = gameObject.GetComponent<Renderer>();

        foreach (Player player in PhotonNetwork.PlayerList)
        {

            Material playerMat = GetMaterial((int)player.CustomProperties["Material"]);
            Debug.Log(gameObject.name + " " + playerMat);
            if (rend.sharedMaterial == playerMat)
            {
                Debug.Log(gameObject.name + " " + player.UserId);
                photonView.TransferOwnership(player);
            }
            break;
        }
        
        //GameObject playerOBJ = PhotonNetwork.LocalPlayer.TagObject as GameObject;
        //NetworkPlayer player = playerOBJ.GetComponent<NetworkPlayer>();

        //if (rend.sharedMaterial == player.materialAssigned) photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
    }

    private Material GetMaterial(int index)
    {
        if (index == 1) return Resources.Load<Material>("Materials/Yellow");
        else if (index == 2) return Resources.Load<Material>("Materials/Red");
        else return Resources.Load<Material>("Materials/Blue");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using VRTK;

public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    private GameObject spawnedPlayerPrefab;


    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        spawnedPlayerPrefab = PhotonNetwork.Instantiate("VRPlayer", transform.position, transform.rotation);
        spawnedPlayerPrefab.transform.GetChild(0).GetComponent<SDK_InputSimulator>().enabled = true;
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.Destroy(spawnedPlayerPrefab);
    }
}

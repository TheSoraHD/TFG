using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    private GameObject spawnedPlayerPrefab;


    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Vector3 position = new Vector3(transform.position.x, 2.75f, transform.position.z);
        spawnedPlayerPrefab = PhotonNetwork.Instantiate("NetworkPlayer", position, transform.rotation);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.Destroy(spawnedPlayerPrefab);
    }
}

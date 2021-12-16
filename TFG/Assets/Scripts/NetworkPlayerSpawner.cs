using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    private GameObject spawnedPlayerPrefab;
    
    public Vector3[] Spawnpoints = new[] { new Vector3(13.0f, 2.0f, 10.0f), new Vector3(13.0f, 2.0f, -10.0f), new Vector3(-13.0f, 2.0f, 10.0f) };


    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        spawnedPlayerPrefab = PhotonNetwork.Instantiate("Cube", Spawnpoints[(int) PhotonNetwork.CurrentRoom.PlayerCount - 1], transform.rotation);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.Destroy(spawnedPlayerPrefab);
    }
}

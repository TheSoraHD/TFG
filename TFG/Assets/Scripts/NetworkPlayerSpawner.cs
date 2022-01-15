using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    private GameObject spawnedPlayerPrefab;

    public Vector3[] Spawnpoints = new[] { new Vector3(13.0f, 0.25f, 10.0f), new Vector3(13.0f, 0.25f, -10.0f), new Vector3(-13.0f, 0.25f, 10.0f) };

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Hashtable hash = new Hashtable();
        hash.Add("Material", (int)PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        spawnedPlayerPrefab = PhotonNetwork.Instantiate("PreFabs/NetworkPlayer", Spawnpoints[(int) PhotonNetwork.CurrentRoom.PlayerCount - 1], transform.rotation);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.Destroy(spawnedPlayerPrefab);
    }
}

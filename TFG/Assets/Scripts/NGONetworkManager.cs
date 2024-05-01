using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.IO;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;

public class NGONetworkManager : NetworkManager
{
    // instance
    public static NGONetworkManager instance;

    public string IP = "127.0.0.1";

    // Custom properties
    public const string MAP_PROP_KEY = "map";

    void Awake()
    {
        // if an instance already exists and it's not this one - destroy us
        if (instance != null && instance != this)
            gameObject.SetActive(false);
        else {
            // set the instance
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }


    public void OnConnectedToMaster()
    {
            /*
        Debug.Log("Connected To Server.");
        base.OnConnectedToMaster();

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 3;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        roomOptions.CustomRoomPropertiesForLobby = new string[] { MAP_PROP_KEY };
        roomOptions.CustomRoomProperties = new Hashtable { { MAP_PROP_KEY, 0 } };


        PhotonNetwork.JoinOrCreateRoom("Room 1", roomOptions, TypedLobby.Default);
            */
    }

    public void OnJoinedRoom()
    {
        /*
        Debug.Log("Joined a Room.");
        base.OnJoinedRoom();
        */
    }

    //public override void OnPlayerEnteredRoom(Player newPlayer)
    public void OnPlayerEnteredRoom()
    {
        /*
        Debug.Log("A new player joined the room.");
        base.OnPlayerEnteredRoom(newPlayer);
        */
    }

    public void LoadLevel(int level)
    {
        if (IsServer) {
            //Get Level Name From Build Index
            string path = SceneUtility.GetScenePathByBuildIndex(level);
            int slash = path.LastIndexOf('/');
            string name = path.Substring(slash + 1);
            int dot = name.LastIndexOf('.');

            SceneManager.LoadScene(name.Substring(0, dot), UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    public void Create()
    {
        Singleton.StartHost();
    }

    public void Join()
    {
        Singleton.GetComponent<UnityTransport>().SetConnectionData(IP, 7777);
        Singleton.StartClient();
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.Networking;
using Unity.Netcode;

public class RadioButtonSystem : MonoBehaviour
{
    public GameObject ballPrefab, maniquinPrefab, humanPrefab;
    public GameObject networkManager;
    ToggleGroup toggleGroup;

    private void Start()
    {
        toggleGroup = GetComponent<ToggleGroup>();
        ApplyModel();
    }

    public void ApplyModel()
    {
        Toggle toggle = toggleGroup.ActiveToggles().FirstOrDefault();
        Debug.Log(toggle.name);
        switch (toggle.name)
        {
            case "ToggleBall":
                Debug.Log("Ball selected");
                networkManager.GetComponent<NetworkManager>().NetworkConfig.PlayerPrefab = ballPrefab;
                break;
            case "ToggleManiquin":
                Debug.Log("Maniquin selected");
                networkManager.GetComponent<NetworkManager>().NetworkConfig.PlayerPrefab = maniquinPrefab;
                break;
            case "ToggleHuman":
                Debug.Log("Human selected");
                networkManager.GetComponent<NetworkManager>().NetworkConfig.PlayerPrefab = humanPrefab;
                break;
            default:
                break;
        }
        networkManager.GetComponent<NetworkManager>().NetworkConfig.ForceSamePrefabs = true;
    }
}

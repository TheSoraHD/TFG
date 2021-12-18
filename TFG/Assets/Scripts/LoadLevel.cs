using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LoadLevel : MonoBehaviour
{

    public int nextLevel;
    public bool passLevel;
    public NetworkManager networkManager;

    private Vector3 minBoundingBox;
    private Vector3 maxBoundingBox;

    private bool level_loaded;

    // Start is called before the first frame update
    void Start()
    {
        passLevel = false;
        level_loaded = false;
        minBoundingBox = gameObject.GetComponent<Collider>().bounds.min;
        maxBoundingBox = gameObject.GetComponent<Collider>().bounds.max;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (passLevel && networkManager.IsMasterClient() && !level_loaded)
        {
            networkManager.LoadLevel(nextLevel);
            level_loaded = true;
        }
        else CheckPlayersPosition();
    }

    public void CheckPlayersPosition()
    {
        var photonViews = Object.FindObjectsOfType<PhotonView>();

        bool playersReady = true;

        foreach (var view in photonViews)
        {
            if (view.Owner != null)
            {
                Vector3 position = view.gameObject.transform.position;

                // check player position inside passLevel platform
                bool insideBox = false;
                if (minBoundingBox.x < position.x && position.x < maxBoundingBox.x && minBoundingBox.z < position.z && position.z < maxBoundingBox.z) insideBox = true;

                playersReady &= insideBox;
            }
        }

        passLevel = playersReady && photonViews.Length == 1;
    }
}

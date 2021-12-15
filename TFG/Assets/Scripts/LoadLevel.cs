using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LoadLevel : MonoBehaviour
{

    public int nextLevel;
    public bool passLevel;
    public NetworkManager networkManager;

    private Vector3 minBoundingBox;
    private Vector3 maxBoundingBox;

    // Start is called before the first frame update
    void Start()
    {
        passLevel = false;
        minBoundingBox = gameObject.GetComponent<Collider>().bounds.min;
        maxBoundingBox = gameObject.GetComponent<Collider>().bounds.max;
    }

    // Update is called once per frame
    void Update()
    {
        if (passLevel) networkManager.LoadLevel(nextLevel);
        else CheckPlayersPosition();
    }

    public void CheckPlayersPosition()
    {
        PhotonView[] photonViews = Object.FindObjectsOfType<PhotonView>();
        bool playersReady = true;

        if (photonViews.length == 3)
        {
            for (int i = 0; i < photonViews.length; ++i)
            {
                if (photonViews[i].owner != null)
                {
                    Vector3 position = photonViews[i].gameObject.transform.position;
                    if (position.x < minBoundingBox.x || maxBoundingBox.x < position.x || position.z < minBoundingBox.z || maxBoundingBox.z < position.z) playersReady = false;
                }
            }

            if (playersReady) passLevel = true;
        }
    }

}

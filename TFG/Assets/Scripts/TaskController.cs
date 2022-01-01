using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TaskController : MonoBehaviour
{
    // instance
    public static TaskController instance;

    public int currentLevel;
    private bool firstTime;


    public PhotonView levelLoader;
    public PhotonView photonView;

    void Awake()
    {
        // if an instance already exissts and it's not this one - destroy us
        if (instance != null && instance != this)
            gameObject.SetActive(false);
        else
        {
            // set the instance
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        currentLevel = 0;
        firstTime = true;
    }

    public void CheckConditions()
    {
        if (currentLevel == 0)
        {
            IntroButton button = GameObject.Find("Button").GetComponent<IntroButton>();
            if (button.pushed && firstTime)
            {
                Debug.Log("Get activated.");
                levelLoader.RPC("ActivatePlatform", RpcTarget.All, 1);
                firstTime = false;
            }
        }
        else if (currentLevel == 1)
        {
            GameObject spaceship = GameObject.Find("Spaceship");
            if (spaceship != null && spaceship.transform.childCount == 3)
            {
                if (CheckSpaceshipColor(spaceship) && firstTime)
                {
                    Debug.Log("Get activated.");
                    levelLoader.RPC("ActivatePlatform", RpcTarget.All, 2);
                    photonView.RPC("DestroySpaceship", RpcTarget.All, "Spaceship");
                    firstTime = false;
                }
            }
        }
        else if (currentLevel == 2)
        {
            GameObject plane = GameObject.Find("Plane3");
            GameObject[] HanoiPieces = GameObject.FindGameObjectsWithTag("HanoiPiece");

            if (plane != null && HanoiPieces.Length > 0)
            {
                if (CheckHanoiPieces(plane, HanoiPieces) && firstTime)
                {
                    Debug.Log("Get activated.");
                    levelLoader.RPC("ActivatePlatform", RpcTarget.All, 3);
                    firstTime = false;
                    //bigHP.gameObject.GetComponent<PhotonView>().RPC("DeactivateMovement", RpcTarget.All);
                    //mediumHP.gameObject.GetComponent<PhotonView>().RPC("DeactivateMovement", RpcTarget.All);
                    //smallHP.gameObject.GetComponent<PhotonView>().RPC("DeactivateMovement", RpcTarget.All);
                }
            }
        }
        else if (currentLevel == 3)
        {
            CastlePart[] castleParts = Resources.FindObjectsOfTypeAll<CastlePart>();

            if (castleParts.Length > 0)
            {
                if (CheckCastleParts(castleParts) && firstTime)
                {
                    Debug.Log("Get activated.");
                    levelLoader.RPC("ActivatePlatform", RpcTarget.All, 4);
                    firstTime = false;
                }
            }
        }
    }

    public void ResetFirstTime()
    {
        firstTime = true;
    }

    public void IncrementLevel()
    {
        ++currentLevel;
    }

    // checks that the spaceship parts have their proper value
    bool CheckSpaceshipColor(GameObject spaceship)
    {
        return true;
        bool res = true;
        for (int i = 0; i < spaceship.transform.childCount; ++i)
        {
            SpaceshipPart sp = spaceship.transform.GetChild(i).GetComponent<SpaceshipPart>();
            Renderer rend = spaceship.transform.GetChild(i).GetChild(0).gameObject.GetComponent<Renderer>();
            res &= sp.materialAssigned == rend.sharedMaterial;
        }
        return res;
    }

    // checks that all the hanoi pieces are in the goal plane
    bool CheckHanoiPieces(GameObject plane, GameObject[] HanoiPieces)
    {
        HanoiPlaneCollider planeColl = plane.GetComponent<HanoiPlaneCollider>();

        bool res = true;
        foreach (GameObject hanoiPiece in HanoiPieces)
        {
            res &= hanoiPiece.GetComponent<HanoiPiece>().planeAttached == planeColl.planeID;
        }

        return res;
    }

    // checks that all castle pieces are placed in the platform
    bool CheckCastleParts(CastlePart[] CastleParts)
    {
        bool res = true;
        foreach (CastlePart castlePart in CastleParts)
        {
            res &= castlePart.state == 2;
        }
        return res;
    }

    [PunRPC]
    void DestroySpaceship(string name)
    {
        GameObject spaceship = GameObject.Find(name);
        Destroy(spaceship);
    }
}

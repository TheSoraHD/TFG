using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TaskController : MonoBehaviour
{
    // instance
    public static TaskController instance;

    [SerializeField]
    private int currentLevel;
    [SerializeField]
    private bool firstTime;

    public PhotonView levelLoader;
    public PhotonView photonView;

    public AudioSource win;

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
                levelLoader.RPC("ActivatePlatform", RpcTarget.All, 1);
                photonView.RPC("PlayLevelClear", RpcTarget.All);
                firstTime = false;
            }
        }
        else if (currentLevel == 1)
        {
            GameObject spaceship = GameObject.Find("Spaceship");
            if (spaceship != null && spaceship.transform.childCount == 6)
            {
                if (CheckSpaceshipColor(spaceship) && firstTime)
                {
                    levelLoader.RPC("ActivatePlatform", RpcTarget.All, 2);
                    photonView.RPC("DestroySpaceship", RpcTarget.All, "Spaceship");
                    photonView.RPC("PlayLevelClear", RpcTarget.All);
                    firstTime = false;
                }
            }
        }
        else if (currentLevel == 2)
        {
            GameObject platform = GameObject.Find("/Platform3");
            GameObject[] HanoiPieces = GameObject.FindGameObjectsWithTag("HanoiPiece");

            if (platform != null && HanoiPieces.Length > 0)
            {
                if (CheckHanoiPieces(platform, HanoiPieces) && firstTime)
                {
                    levelLoader.RPC("ActivatePlatform", RpcTarget.All, 3);
                    photonView.RPC("PlayLevelClear", RpcTarget.All);
                    firstTime = false;
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
                    levelLoader.RPC("ActivatePlatform", RpcTarget.All, 4);
                    photonView.RPC("PlayLevelClear", RpcTarget.All);
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
        bool res = true;
        for (int i = 0; i < spaceship.transform.childCount; ++i)
        {
            GameObject piece = spaceship.transform.GetChild(i).gameObject;
            SpaceshipPart sp = piece.GetComponent<SpaceshipPart>();
            Renderer rend = piece.GetComponent<Renderer>();
            res &= sp.materialAssigned.name == rend.sharedMaterial.name;
        }
        return res;
    }

    // checks that all the hanoi pieces are in the goal plane
    bool CheckHanoiPieces(GameObject platform, GameObject[] HanoiPieces)
    {
        HanoiPlatformCollider platformColl = platform.GetComponent<HanoiPlatformCollider>();
        bool res = true;
        foreach (GameObject hanoiPiece in HanoiPieces)
        {
            res &= hanoiPiece.GetComponent<HanoiPiece>().platformAttached == platformColl.platformID;
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

    [PunRPC]
    void PlayLevelClear()
    {
        win.Play();
    }

    public void SetCurrentLevel(int lvl)
    {
        currentLevel = lvl;
    }
}

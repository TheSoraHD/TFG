using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NGOTaskController : NetworkBehaviour
{
    // instance
    public static NGOTaskController instance;
    public AudioSource win;

    [SerializeField]
    private int currentLevel;
    [SerializeField]
    private bool firstTime;

    //public PhotonView levelLoader, photonView;

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
                //levelLoader.ActivatePlatform(1);
                //photonView.PlayLevelClear();
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
                    //levelLoader.ActivatePlatform(2);
                    //photonView.PlayLevelClear();
                    //photonView.DestroySpaceship("Spaceship");
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
                    //levelLoader.ActivatePlatform(3);
                    //photonView.PlayLevelClear();
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
                    //levelLoader.ActivatePlatform(4);
                    //photonView.PlayLevelClear();
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

    [Rpc(SendTo.Everyone)]
    void DestroySpaceshipRpc(string name)
    {
        GameObject spaceship = GameObject.Find(name);
        Destroy(spaceship);
    }

    [Rpc(SendTo.Everyone)]
    void PlayLevelClearRpc()
    {
        win.Play();
    }

    public void SetCurrentLevel(int lvl)
    {
        currentLevel = lvl;
    }
}

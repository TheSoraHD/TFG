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

    public NGOLevelLoader levelLoader;

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
        switch (currentLevel)
        {
            case 0: //INTRO
                IntroButton button = GameObject.Find("Button").GetComponent<IntroButton>();
                if (button.pushed && firstTime)
                    LevelCleared(currentLevel);
                break;

            case 1: //SPACESHIP
                GameObject spaceship = GameObject.Find("Spaceship");
                if (spaceship != null && spaceship.transform.childCount == 6) {
                    if (CheckSpaceshipColor(spaceship) && firstTime) {
                        LevelCleared(currentLevel);
                        Destroy(spaceship);
                    }
                }
                break;

            case 2: //HANOI
                GameObject platform = GameObject.Find("/Platform3");
                GameObject[] HanoiPieces = GameObject.FindGameObjectsWithTag("HanoiPiece");

                if (platform != null && HanoiPieces.Length > 0) {
                    if (CheckHanoiPieces(platform, HanoiPieces) && firstTime)
                        LevelCleared(currentLevel);
                }
                break;

            case 3: //CASTLE
                NGOCastlePart[] castleParts = Resources.FindObjectsOfTypeAll<NGOCastlePart>();

                if (castleParts.Length > 0) {
                    if (CheckCastleParts(castleParts) && firstTime)
                        LevelCleared(currentLevel);
                }
                break;

            case 4: //CAR
                NGOWheel[] wheels = GameObject.FindObjectsOfType<NGOWheel>();
                NGOEngine engine = GameObject.FindObjectOfType<NGOEngine>();

                if (CheckCarTasks(wheels, engine) && firstTime)
                    LevelCleared(currentLevel);
                break;

            default:
                break;
        }
    }
    private void LevelCleared(int currentLevel)
    {
        levelLoader.ActivatePlatformRpc(currentLevel + 1);
        PlayLevelClearRpc();
        firstTime = false;
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
            NGOSpaceshipPart sp = piece.GetComponent<NGOSpaceshipPart>();
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
    bool CheckCastleParts(NGOCastlePart[] CastleParts)
    {
        bool res = true;
        foreach (NGOCastlePart castlePart in CastleParts)
        {
            res &= castlePart.state == 2;
        }
        return res;
    }

    bool CheckCarTasks(NGOWheel[] wheels, NGOEngine engine)
    {
        return (wheels[0].snapped && wheels[1].snapped && engine.snapped);
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

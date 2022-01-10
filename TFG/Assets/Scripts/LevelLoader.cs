using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LevelLoader : MonoBehaviour
{

    // instance
    public static LevelLoader instance;

    public int nextLevel;
    public bool passLevel;
    public NetworkManager networkManager;
    public TaskController taskController;

    public GameObject platform;
    public bool platformActive;
    private Vector3 minBoundingBox;
    private Vector3 maxBoundingBox;
    
    [SerializeField]
    private bool level_loaded;


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
        nextLevel = 1;
        passLevel = false;

        GetPlatformMaxMinCoord();
        InitPlatform();
        DontDestroyOnLoad(platform);
    }

    public bool CheckPlayersPosition()
    {
        var photonViews = Object.FindObjectsOfType<PhotonView>();

        bool playersReady = true;

        foreach (var view in photonViews)
        {
            if (view.Owner != null && view.gameObject.tag == "Player")
            {
                Vector3 position = view.gameObject.transform.GetChild(0).position;

                // check player position inside passLevel platform
                bool insideBox = false;
                if (minBoundingBox.x < position.x && position.x < maxBoundingBox.x && minBoundingBox.z < position.z && position.z < maxBoundingBox.z) insideBox = true;
                
                playersReady &= insideBox;
            }
        }

        return playersReady;
    }

    void GetPlatformMaxMinCoord()
    {
        Collider col = platform.GetComponent<Collider>();
        minBoundingBox = col.bounds.min;
        maxBoundingBox = col.bounds.max;
        col.enabled = false;
    }

    void InitPlatform()
    {
        platform.SetActive(false);
        platformActive = false;
        level_loaded = false;
        taskController.ResetFirstTime();
    }

    IEnumerator InitPlatformCoroutine()
    {
        StartCoroutine("InitPlatform");
        yield return new WaitForSeconds(2);
    }

    public void LevelUpdate()
    {
        passLevel = CheckPlayersPosition();

        if (passLevel && !level_loaded)
        {
            level_loaded = true;
            networkManager.LoadLevel(nextLevel);
            taskController.IncrementLevel();
            StartCoroutine("InitPlatformCoroutine");
        }
    }

    public void CheckConditions()
    {
        taskController.CheckConditions();
    }

    [PunRPC]
    void ActivatePlatform(int nextlvl)
    {
        platform.SetActive(true);
        platformActive = true;
        nextLevel = nextlvl;
    }

    public void ChangeLevel(int lvl)
    {
        level_loaded = true;
        networkManager.LoadLevel(lvl);
        taskController.SetCurrentLevel(lvl);
        StartCoroutine("InitPlatformCoroutine");
    }
}

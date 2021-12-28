using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LoadLevel : MonoBehaviour
{

    // instance
    public static LoadLevel instance;

    public int nextLevel;
    public bool passLevel;
    public NetworkManager networkManager;
    public TaskController taskController;

    public GameObject platform;
    private Vector3 minBoundingBox;
    private Vector3 maxBoundingBox;

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
        level_loaded = false;

        InitPlatform();
        DontDestroyOnLoad(platform);
    }

    // Update is called once per frame
    void Update()
    {

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

    void InitPlatform()
    {
        Collider col = platform.GetComponent<Collider>();
        minBoundingBox = col.bounds.min;
        maxBoundingBox = col.bounds.max;
        level_loaded = false;
        taskController.ResetFirstTime();
        platform.SetActive(false);
    }

    IEnumerator InitPlatformCoroutine()
    {
        StartCoroutine("InitPlatform");
        yield return new WaitForSeconds(2);
    }

    [PunRPC]
    void LevelUpdate()
    {
        if (platform.activeInHierarchy)
        {
            passLevel = CheckPlayersPosition();

            if (passLevel && !level_loaded)
            {
                networkManager.LoadLevel(nextLevel);
                nextLevel = (nextLevel + 1) % 4;
                level_loaded = true;
                StartCoroutine("InitPlatformCoroutine");
            }
        }
    }

    [PunRPC]
    void ChangePlatformActivity()
    {
        if (!platform.activeInHierarchy) platform.SetActive(true);
        else platform.SetActive(false);
    }

    [PunRPC]
    void ActivatePlatform()
    {
        platform.SetActive(true);
    }
}

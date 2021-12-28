using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TaskController : MonoBehaviour
{
    public int currentLevel;
    private bool firstTime;
    public PhotonView levelLoader;
    
    public Material[] SpaceshipMaterials; 

    // Start is called before the first frame update
    void Start()
    {
        currentLevel = 0;
        firstTime = true;
        levelLoader = GameObject.Find("_LevelLoader").GetComponent<PhotonView>();
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentLevel == 0)
        {
            IntroButton button = GameObject.Find("Button").GetComponent<IntroButton>();
            if (button.pushed && firstTime)
            {
                levelLoader.RPC("ActivatePlatform", RpcTarget.All);
                firstTime = false;
            }
        }
        else if (currentLevel == 1)
        {
            GameObject spaceship = GameObject.Find("Spaceship");
            if (spaceship != null && spaceship.transform.childCount == 6)
            {
                bool correct = CheckSpaceshipColor(spaceship);
                levelLoader.RPC("ActivatePlatform", RpcTarget.All);
                Destroy(spaceship);
                ++currentLevel;
            }
        }
        else if (currentLevel == 2)
        {

        }
        else if (currentLevel == 3)
        {

        }
    }

    public void ResetFirstTime()
    {
        firstTime = false;
    }

    bool CheckSpaceshipColor(GameObject spaceship)
    {
        return true;
    }

}

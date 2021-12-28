using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class IntroButton : MonoBehaviour
{

    public bool pushed;
    public PhotonView levelLoader;

    private bool firstTime;

    // Start is called before the first frame update
    void Start()
    {
        pushed = false;
        firstTime = true;
        levelLoader = GameObject.Find("_LevelLoader").GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pushed && firstTime)
        {
            levelLoader.RPC("ActivatePlatform", RpcTarget.All);
            firstTime = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.transform.root.name);
        if (collision.transform.root.tag == "Player") pushed = true;
    }
}

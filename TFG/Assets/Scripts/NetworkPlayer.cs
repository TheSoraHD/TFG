using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using VRTK;

public class NetworkPlayer : MonoBehaviour
{
    public Transform VRTK_model;
    private PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        VRTK_model = GameObject.Find("/VRTK_SDKSetup/[VRSimulator_CameraRig]/Player").transform;
        VRTK_model.parent.GetComponent<SDK_InputSimulator>().enabled = true;
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            gameObject.SetActive(false);
            //MapPosition(model);
            transform.position = VRTK_model.position;
            transform.rotation = VRTK_model.rotation;
        }
    }

    void MapPosition(Transform target)
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpaceshipPart : MonoBehaviour
{
    public int partID;
    public bool isBody;
    public bool marked;
    public Vector3 relative_position;

    public int material_state;
    public Material materialAssigned;


    // Start is called before the first frame update
    void Start()
    {
        marked = false;
        material_state = 0;
    }

    void OnCollisionEnter(Collision collision)
    {
        // checks if the collision is an spaceship part
        if (collision.gameObject.tag == "SpaceshipPart")
        {
            // if the spaceship part is the parent and the merge is possible, this gameObject joins to the parent
            if (IsParent(collision.gameObject))
            {
                if (MergeIsPossible(collision.gameObject, true))
                    gameObject.GetComponent<PhotonView>().RPC("JoinToSpaceship", RpcTarget.All, collision.gameObject.name);
            }
            // if the spaceship part is not the parent and the merge is possible, then both objects merge into a spaceship parent object
            // it also assures the merge only is activated from the lowest partID
            else if (MergeIsPossible(collision.gameObject, false) && partID < collision.gameObject.GetComponent<SpaceshipPart>().partID)
                gameObject.GetComponent<PhotonView>().RPC("Merge", RpcTarget.All, collision.gameObject.name);
            else marked = false;
        }
    }

    // checks whether the gameObject is a combination of spaceship parts or not 
    bool IsParent(GameObject o)
    {
        bool res = true;
        for (int i = 0; i < o.transform.childCount; ++i)
            if (o.transform.GetChild(i).tag != "SpaceshipPart") res = false;

        return res;
    }

    // checks if merge is possible when there are two objects marked
    bool MergeIsPossible(GameObject o, bool isParent)
    {
        SpaceshipPart sp;
        if (isParent) sp = o.transform.Find("body").GetComponent<SpaceshipPart>();
        else sp = o.GetComponent<SpaceshipPart>();
        return marked && sp.marked && isBody != sp.isBody;
    }

    [PunRPC]
    // merge function merges self with the gameobject passed by parameter into a new gameobject
    void Merge(string name)
    {
        GameObject o = GameObject.Find(name);

        // Destroying self & o rigidbodies in order to avoid bugs with multiple rigidbodies
        Destroy(gameObject.GetComponent<PhotonRigidbodyView>());
        Destroy(gameObject.GetComponent<Rigidbody>());
        Destroy(o.GetComponent<PhotonRigidbodyView>());
        Destroy(o.GetComponent<Rigidbody>());

        // creation of parent
        GameObject spaceship = new GameObject("Spaceship", typeof(Rigidbody), typeof(PhotonView), typeof(PhotonRigidbodyView));
        spaceship.tag = "SpaceshipPart";
        spaceship.transform.position = Vector3.Lerp(gameObject.transform.position, o.transform.position, (float) 0.5);

        // config parent photon properties
        ConfigPhotonView(spaceship);

        // assigning self & o to new parent
        gameObject.transform.parent = spaceship.transform;
        gameObject.transform.localPosition = relative_position;
        Destroy(gameObject.GetComponent<PhotonView>());
        marked = false;

        SpaceshipPart sp = o.GetComponent<SpaceshipPart>();
        o.transform.parent = spaceship.transform;
        o.transform.localPosition = sp.relative_position;
        Destroy(o.GetComponent<PhotonView>());
        sp.marked = false;

        // enable parent's rigidbody
        Rigidbody spaceship_rb = spaceship.GetComponent<Rigidbody>();
        spaceship_rb.freezeRotation = true;
        spaceship_rb.isKinematic = false;


    }

    void ConfigPhotonView(GameObject spaceship)
    {
        PhotonView spaceship_pv = spaceship.GetComponent<PhotonView>();
        spaceship_pv.ViewID = 20;
        spaceship_pv.OwnershipTransfer = OwnershipOption.Takeover;
        spaceship_pv.TransferOwnership(PhotonNetwork.MasterClient);
        spaceship_pv.observableSearch = PhotonView.ObservableSearch.AutoFindAll;
        spaceship_pv.FindObservables();

        PhotonRigidbodyView prv = spaceship.GetComponent<PhotonRigidbodyView>();
        prv.m_SynchronizeAngularVelocity = true;
        prv.m_TeleportEnabled = true;
    }

    [PunRPC]
    // joins this gameObject to the spaceship object passed by parameter
    void JoinToSpaceship(string name)
    {
        GameObject spaceship = GameObject.Find(name);

        // Destroying self rigidbody in order to avoid bugs with multiple rigidbodies
        Destroy(gameObject.GetComponent<PhotonRigidbodyView>());
        Destroy(gameObject.GetComponent<Rigidbody>());

        // updating spaceship position to the point of collision
        spaceship.transform.position = Vector3.Lerp(gameObject.transform.position, spaceship.transform.position, (float)0.5);

        // assigning self to spaceship
        gameObject.transform.parent = spaceship.transform;
        gameObject.transform.localPosition = relative_position;
        Destroy(gameObject.GetComponent<PhotonView>());
        marked = false;

        // unmarking body's spaceship
        spaceship.transform.Find("body").GetComponent<SpaceshipPart>().marked = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpaceshipPart : MonoBehaviour
{
    // part attributes
    public int partID;
    public bool isBody;
    public bool marked;
    public Vector3 relative_position;

    public AudioSource hit;
    public AudioSource fail;

    // color attributes
    [SerializeField]
    public int material_state;
    [SerializeField]
    private Material[] materials = new Material[4];
    public Material materialAssigned;


    // Start is called before the first frame update
    void Start()
    {
        marked = false;
        material_state = 0;
        InitMaterials();
    }

    void OnCollisionEnter(Collision collision)
    {
        // checks if the collision is an spaceship part
        if (collision.gameObject.tag == "SpaceshipPart")
        {
            // if the merge is possible, then both objects merge into a spaceship parent object
            // it also assures the merge only is activated from the lowest partID
            SpaceshipPart col_sp = collision.gameObject.GetComponent<SpaceshipPart>();
            if (partID < col_sp.partID)
            {
                if (MergeIsPossible(collision.gameObject, false))
                    gameObject.GetComponent<PhotonView>().RPC("Merge", RpcTarget.All, collision.gameObject.name);
                else
                {
                    fail.Play();
                    marked = false;
                    col_sp.marked = false;
                }
            }
        }
        else if (collision.gameObject.tag == "Spaceship")
        {
            // if collision is with the spaceship parent and the merge is possible, this gameObject joins to the parent
            if (MergeIsPossible(collision.gameObject, true))
                gameObject.GetComponent<PhotonView>().RPC("JoinToSpaceship", RpcTarget.All, collision.gameObject.name);
            else
            {
                fail.Play();
                marked = false;
                collision.transform.Find("Body").GetComponent<SpaceshipPart>().marked = false;
            }
        }
    }

    // checks if merge is possible when there are two objects marked
    bool MergeIsPossible(GameObject o, bool isParent)
    {
        SpaceshipPart sp;
        if (isParent) sp = o.transform.Find("Body").GetComponent<SpaceshipPart>();
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
        spaceship.tag = "Spaceship";
        spaceship.transform.position = Vector3.Lerp(gameObject.transform.position, o.transform.position, (float) 0.5);

        // config parent photon properties
        ConfigPhotonView(spaceship);

        // assigning self & o to new parent
        gameObject.transform.parent = spaceship.transform;
        gameObject.transform.localPosition = relative_position;
        //Destroy(gameObject.GetComponent<PhotonView>());
        marked = false;

        SpaceshipPart sp = o.GetComponent<SpaceshipPart>();
        o.transform.parent = spaceship.transform;
        o.transform.localPosition = sp.relative_position;
        //Destroy(o.GetComponent<PhotonView>());
        sp.marked = false;

        // enable parent's rigidbody
        Rigidbody spaceship_rb = spaceship.GetComponent<Rigidbody>();
        spaceship_rb.freezeRotation = true;
        spaceship_rb.isKinematic = false;

        hit.Play();
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
        spaceship.transform.Find("Body").GetComponent<SpaceshipPart>().marked = false;

        hit.Play();
    }

    void InitMaterials()
    {
        materials.SetValue(Resources.Load<Material>("Materials/default"), 0);
        materials.SetValue(Resources.Load<Material>("Materials/Yellow"), 1);
        materials.SetValue(Resources.Load<Material>("Materials/Red"), 2);
        materials.SetValue(Resources.Load<Material>("Materials/Blue"), 3);
    }

    [PunRPC]
    void ChangeColor()
    {
        Renderer rend = gameObject.GetComponent<Renderer>();

        material_state = (material_state + 1) % 4;
        rend.sharedMaterial = materials[material_state];
    }
}

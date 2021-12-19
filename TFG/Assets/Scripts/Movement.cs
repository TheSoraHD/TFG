using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = transform.position;
        if (Input.GetKey(KeyCode.W)) position.z += 0.05f;
        if (Input.GetKey(KeyCode.S)) position.z -= 0.05f;
        if (Input.GetKey(KeyCode.A)) position.x -= 0.05f;
        if (Input.GetKey(KeyCode.D)) position.x += 0.05f;

        transform.position = position;
    }
}

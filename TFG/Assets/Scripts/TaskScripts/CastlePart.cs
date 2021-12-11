using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastlePart : MonoBehaviour
{

    public GameObject platform_object;
    public GameObject[] requirements;

    // status of the CastlePiece
    // state = 0 --> idle
    // state = 1 --> picked up and displayed on platform
    // state = 2 --> placed
    public int state;

    // Start is called before the first frame update
    void Start()
    {
        state = 0;
    }

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(platform_object.transform.position, transform.position);
        if (state == 0)
        {
            if (dist < 6.0f && CheckRequirements())
            {
                platform_object.SetActive(true);
                state = 1;
            }
        }
        else
        {
            if (dist >= 6.0f)
            {
                platform_object.SetActive(false);
                state = 0;
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                state = 2;
                gameObject.SetActive(false);
            }
        }
    }

    bool CheckRequirements()
    {
        bool res = true;
        for (int i = 0; i < requirements.Length; ++i)
        {
            res = res && requirements[i].GetComponent<CastlePart>().state == 2;
        }
        return res;
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;

public class IPUpdater : MonoBehaviour
{
    public NGONetworkManager networkManager;

    public void IPUpdate()
    {
        networkManager.IP = GetComponent<TMP_InputField>().text;
    }
}

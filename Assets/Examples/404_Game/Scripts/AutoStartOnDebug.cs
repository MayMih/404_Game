using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoStartOnDebug : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (Debug.isDebugBuild)
        {
            GameObject.FindObjectOfType<UIScript>()?.Restart();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

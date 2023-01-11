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

}

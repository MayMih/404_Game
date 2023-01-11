using UnityEngine;

public class SkinLoader : MonoBehaviour
{
    private const string RESOURCE_FOLDER_PATH = "eggs";
    //@"Easter_UI\eggs";

    private Sprite[] skins;

    public Sprite GetRandomSkin()
    {
        return skins[Random.Range(0, skins.Length)];
    }

    // Start is called before the first frame update
    void Start()
    {
        skins = Resources.LoadAll<Sprite>(RESOURCE_FOLDER_PATH);
        foreach (var t in skins)
        {
            Debug.Log(t.name);
        }
    }

}

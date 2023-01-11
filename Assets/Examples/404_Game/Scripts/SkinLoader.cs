using UnityEngine;

public class SkinLoader : MonoBehaviour
{
    /// <summary>
    /// Каталог внутри каталога Assets\Resources
    /// </summary>
    private const string RESOURCE_FOLDER_PATH = "eggs";

    private Sprite[] skins;

    public Sprite GetRandomSkin()
    {
        return skins[Random.Range(0, skins.Length)];
    }

    // Start is called before the first frame update
    private void Awake()
    {
        skins = Resources.LoadAll<Sprite>(RESOURCE_FOLDER_PATH);
    }

}

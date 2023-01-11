using UnityEngine;
using UnityEngine.UI;

public class OnSliderMove : MonoBehaviour
{
    [SerializeField] private GameObject objectSpawner;

    private Slider slider;
    private ObjectCreatorArea[] creators;

    private void Start()
    {
        slider = gameObject.GetComponent<Slider>();
        creators = GameObject.FindObjectsOfType<ObjectCreatorArea>();
        ApplyTime();
    }

    /// <summary>
    /// Метод применяет значение частоты (промежутка времени) между генерацией объектов 
    /// </summary>
    public void ApplyTime()
    {
        foreach (ObjectCreatorArea area in creators)
        {
            area.SpawnInterval = slider.value;
        }        
    }
}

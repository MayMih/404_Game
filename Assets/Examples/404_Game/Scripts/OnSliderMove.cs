using UnityEngine;
using UnityEngine.UI;

public class OnSliderMove : MonoBehaviour
{
    [SerializeField] private GameObject objectSpawner;

    private Slider slider;
    //private ObjectCreatorArea[] creators;
    private UIScript ui;

    private void Start()
    {
        slider = gameObject.GetComponent<Slider>();
        //creators = GameObject.FindObjectsOfType<ObjectCreatorArea>();
        ui = GameObject.FindObjectOfType<UIScript>();
        ApplyTime();
    }

    /// <summary>
    /// Метод применяет значение частоты (промежутка времени) между генерацией объектов 
    /// </summary>
    public void ApplyTime()
    {
        //foreach (ObjectCreatorArea area in creators)
        //{
        //    area.SpawnInterval = slider.value;
        //}        
        ui.SwitchInterval = slider.value;
    }
}

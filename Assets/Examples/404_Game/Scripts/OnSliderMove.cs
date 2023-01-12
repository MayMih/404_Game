using UnityEngine;
using UnityEngine.UI;

public class OnSliderMove : MonoBehaviour
{
    private Slider slider;
    private UIScript ui;

    private void Start()
    {
        slider = gameObject.GetComponent<Slider>();
        ui = GameObject.FindObjectOfType<UIScript>();
        ApplyTime();
    }

    /// <summary>
    /// Метод применяет значение частоты (промежутка времени) между генерацией объектов 
    /// </summary>
    public void ApplyTime()
    {
        ui.SwitchInterval = slider.value;
    }
}

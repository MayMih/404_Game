using System.Linq;

using UnityEngine;
using UnityEngine.UI;

public class OnSliderMove : MonoBehaviour
{
    [SerializeField] private GameObject objectSpawner;

    private Slider slider;


    private void Start()
    {
        slider = gameObject.GetComponent<Slider>();
    }

    /// <summary>
    /// Метод применяет значение частоты (промежутка времени) между генерацией объектов 
    /// </summary>
    public void ApplyTime()
    {
        //Debug.Log("Slider moved! " + slider.value);
        objectSpawner.GetComponents<ObjectCreatorArea>().ToList().ForEach(area =>
            area.SpawnInterval = slider.value
        );        
    }
}

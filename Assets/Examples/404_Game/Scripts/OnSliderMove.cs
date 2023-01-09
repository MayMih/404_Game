using System.Collections;
using System.Collections.Generic;
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
    /// Метод применяет значение слайдера к скорости падения объектов (как новых из генератора, так и существующих)
    /// </summary>
    public void ApplySpeed()
    {
        objectSpawner.GetComponents<ObjectCreatorArea>().ToList().ForEach(area => {
            area.SetObjectSpeed(slider.value);
            GameObject.FindGameObjectsWithTag(area.prefabToSpawn.tag).ToList().ForEach(x =>
                x.GetComponent<ObjectMovement>().FallSpeed = slider.value);
            }
        );        
        //Debug.Log("Slider moved! " + slider.value);
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

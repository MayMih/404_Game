using UnityEngine;

public class DestroyAnotherObjectOnTouch : MonoBehaviour
{
    [Header("Prefab, копия которого будет уничтожаться")]
    [SerializeField] private GameObject objectToDestroy;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var target = collision.gameObject;
        //Debug.LogWarning(this.name + " detected collision with : " + target.name + " at time: " + Time.time);
        if (target.CompareTag(objectToDestroy.tag))
        {
            Destroy(target);
        }
    }
}

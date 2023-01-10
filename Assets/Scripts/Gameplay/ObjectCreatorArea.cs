using UnityEngine;
using System.Collections;
using System.Linq;

[AddComponentMenu("Playground/Gameplay/Object Creator Area")]
[RequireComponent(typeof(BoxCollider2D))]
public class ObjectCreatorArea : MonoBehaviour
{
	[Header("Object creation")]
	// The object to spawn
	// WARNING: take if from the Project panel, NOT the Scene/Hierarchy!
	public GameObject prefabToSpawn;

	private UIScript ui;

	[Header("Промежуток времени в секундах между генерацией объектов")]
	public float SpawnInterval = 2;
    [Header("Модификатор промежутка времени между генерацией объектов")]
    [SerializeField] private int SpawnIntervalCoef = 1;

    private BoxCollider2D boxCollider2D;

	void Start()
	{
		ui = GameObject.FindObjectOfType<UIScript>();
		boxCollider2D = GetComponent<BoxCollider2D>();
		StartCoroutine(SpawnObject());
	}

	// This will spawn an object, and then wait some time, then spawn another...
	IEnumerator SpawnObject()
	{
		while(true)
		{
			if (!ui?.IsGameOver ?? true)
			{
				// определяем разворот птицы-несушки
				var sign = transform.right == Vector3.right ? 1 : -1;
				var pos = new Vector3(transform.position.x + sign * 3 * boxCollider2D.bounds.extents.x / 2, 
					  transform.position.y, transform.position.z);
                GameObject newObject = Instantiate<GameObject>(prefabToSpawn, pos, transform.rotation);				
			}
            // Wait for some time before spawning another object
            yield return new WaitForSeconds(SpawnInterval * SpawnIntervalCoef);
		}
	}
}

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

	public float ObjectMovementSpeed = 0.005f;

	[Header("Промежуток времени в секундах между генерацией объектов")]
	public float SpawnInterval = 2;
    [Header("Модификатор промежутка времени между генерацией объектов")]
    [SerializeField] private int SpawnIntervalCoef = 1;

    private BoxCollider2D boxCollider2D;

	private float partOfSpawnAreaWidth, minX, maxX;

	void Start()
	{
		ui = GameObject.FindObjectOfType<UIScript>();
		boxCollider2D = GetComponent<BoxCollider2D>();
		partOfSpawnAreaWidth = boxCollider2D.size.x / 3;
		minX = partOfSpawnAreaWidth - boxCollider2D.size.x;
		maxX = boxCollider2D.size.x - partOfSpawnAreaWidth;
		StartCoroutine(SpawnObject());
	}

	public void SetObjectSpeed(float speed)
	{
		ObjectMovementSpeed = speed;
	}


	// This will spawn an object, and then wait some time, then spawn another...
	IEnumerator SpawnObject()
	{
		while(true)
		{
			if (!ui?.IsGameOver ?? true)
			{
				// Generate the new object
				GameObject newObject = Instantiate<GameObject>(prefabToSpawn);
                newObject.GetComponent<ObjectMovement>().FallSpeed = ObjectMovementSpeed;
                newObject.GetComponents<IExternalAudioPlayable>().ToList().ForEach(x => 
					x.Player = ui.GetComponent<AudioSource>()
				);
				float randomX = transform.position.x + Random.Range(minX, maxX);
				float randomY = Random.Range(-boxCollider2D.size.y, boxCollider2D.size.y);
				newObject.transform.position = new Vector2(randomX, randomY + transform.position.y);
			}
            // Wait for some time before spawning another object
            yield return new WaitForSeconds(SpawnInterval * SpawnIntervalCoef);
		}
	}
}

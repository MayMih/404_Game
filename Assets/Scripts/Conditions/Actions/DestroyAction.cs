using UnityEngine;

[AddComponentMenu("Playground/Actions/Destroy Action")]
public class DestroyAction : Action, IExternalAudioPlayable
{
	//who gets destroyed in the collision?
	public Enums.Targets target = Enums.Targets.ObjectThatCollided;
	// assign an effect (explosion? particles?) or object to be created (instantiated) when the one gets destroyed
	public GameObject[] deathEffects;
	[SerializeField] private AudioClip deathSound;	
	/// <summary>
	/// Назначается создателем объекта
	/// </summary>
	/// <remarks>
	/// Если не указан, будет использовано значение из <see cref="UIScript"/>
	/// </remarks>
    public AudioSource Player { get; set; }

    private void Start()
    {
		if (Player == null)
		{
			Player = GameObject.FindObjectOfType<UIScript>()?.GetComponent<AudioSource>();
		}
    }

    //OtherObject is null when this Action is called from a Condition that is not collision-based
    public override bool ExecuteAction(GameObject otherObject)
	{
		if (deathEffects != null)
		{
			foreach (var effect in deathEffects)
			{
				GameObject newObject = Instantiate<GameObject>(effect);
				//move the effect depending on who needs to be destroyed
				Vector3 otherObjectPos = (otherObject == null) ? this.transform.position : otherObject.transform.position;
				newObject.transform.position = (target == Enums.Targets.ObjectThatCollided) ? otherObjectPos : transform.position;
			}
		}
        //remove the GameObject from the scene (destroy)
        if (target == Enums.Targets.ObjectThatCollided)
		{
			if(otherObject != null)
			{
				Destroy(otherObject);
			}
		}
		else
		{            
            Destroy(gameObject);
		}
        Player?.PlayOneShot(deathSound, 1);
        return true; 
	}
}

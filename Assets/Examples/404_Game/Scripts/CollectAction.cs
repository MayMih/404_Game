using UnityEngine;

[AddComponentMenu("Playground/Actions/Collect Action")]
public class CollectAction : Action, IExternalAudioPlayable
{
    // assign an effect (explosion? particles?) or object to be created (instantiated) when the one gets destroyed
    [SerializeField] private GameObject collectEffect;
    public int lifeImpact;
    public int scoreImpact;
    [SerializeField] private AudioClip collectSound;
    /// <summary>
	/// Назначается создателем объекта
	/// </summary>
    public AudioSource Player { get; set; }


    public override bool ExecuteAction(GameObject other)
    {
        if (collectEffect != null)
        {
            Instantiate<GameObject>(collectEffect, transform.position, transform.rotation);
        }
        Player?.PlayOneShot(collectSound, 1);
        Destroy(gameObject);
        return base.ExecuteAction(other);
    }
}

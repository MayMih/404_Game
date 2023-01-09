using UnityEngine;

public class ObjectMovement : MonoBehaviour
{
    public float FallSpeed { get; set; }

    /// <summary>
    /// ћетод передвижени€ тела с посто€нной скоростью
    /// </summary>
    void Update()
    {
        transform.Translate(Vector3.down * FallSpeed, Space.World);
    }

}

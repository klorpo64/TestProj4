using UnityEngine;

public class CarMover : MonoBehaviour
{
    public float speed = 20f;

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.gameplayFrozen)
            return;

        transform.position += Vector3.left * speed * Time.deltaTime;
    }
}

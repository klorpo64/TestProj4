using UnityEngine;

public class CarMover : MonoBehaviour
{
    public float speed = 20f;

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;
    }
}

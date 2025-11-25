using UnityEngine;

public class CarMoverRight : MonoBehaviour
{
    public float speed = 20f;

    void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;
    }
}

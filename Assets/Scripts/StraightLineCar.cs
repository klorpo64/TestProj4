using UnityEngine;

public class StraightLineCar : MonoBehaviour
{
    public float speed = 50f;

    void Update()
    {
        // Move west (negative X in world space)
        transform.position += Vector3.left * speed * Time.deltaTime;
    }
}

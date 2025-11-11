using UnityEngine;

public class PlayerWaterGravity : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Start ignoring gravity
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            // Player entered the water zone enable gravity
            rb.isKinematic = false;
            rb.useGravity = true;
            Debug.Log("Player entered water zone — gravity ON");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            // Player left the water zone disable gravity
            rb.isKinematic = true;
            rb.useGravity = false;
            Debug.Log("Player left water zone — gravity OFF");
        }
    }
}

using UnityEngine;

public class CarPassSound : MonoBehaviour
{
    public Transform player;
    public AudioSource carPassSound;
    public float triggerDistance = 20f;
    private bool hasPlayed = false;

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < triggerDistance && !hasPlayed)
        {
            carPassSound.PlayOneShot(carPassSound.clip);
            hasPlayed = true;
        }

        // Reset when car gets far again, if you want it to be reusable
        if (distance > triggerDistance * 1.5f)
        {
            hasPlayed = false;
        }
    }
}

using UnityEngine;

public class CarTriggerHit : MonoBehaviour
{
    public float launchForce = 10f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerRagdoll player = other.GetComponentInParent<PlayerRagdoll>();
        if (player != null)
        {
            // Calculate direction away from car
            Vector3 direction = (other.transform.position - transform.position).normalized;

            // Apply launch force
            player.ActivateRagdoll(direction * launchForce);
        }
    }
}

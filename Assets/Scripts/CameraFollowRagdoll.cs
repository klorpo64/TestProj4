using UnityEngine;

public class CameraFollowRagdoll : MonoBehaviour
{
    public Transform target;        // Player root or ragdoll root
    public Vector3 offset = new Vector3(0, 5, -10);
    public float smoothSpeed = 5f;

    private Transform ragdollRoot;
    private bool followRagdoll = false;

    void LateUpdate()
    {
        Transform currentTarget = followRagdoll && ragdollRoot != null ? ragdollRoot : target;
        if (currentTarget == null) return;

        Vector3 desiredPosition = currentTarget.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(currentTarget);
    }

    // Call this when the player ragdolls
    public void StartFollowingRagdoll(Transform ragdollTransform)
    {
        ragdollRoot = ragdollTransform;
        followRagdoll = true;
    }
}

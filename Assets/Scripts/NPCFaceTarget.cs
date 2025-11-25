using UnityEngine;

public class NPCFaceTarget : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 5f;
    private bool shouldFaceTarget = false;

    void Update()
    {
        if (shouldFaceTarget && target != null)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0f; // only horizontal rotation

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    public void FaceTarget()
    {
        shouldFaceTarget = true;
    }
}

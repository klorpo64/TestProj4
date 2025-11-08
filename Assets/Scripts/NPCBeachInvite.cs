using UnityEngine;

public class NPCBeachInvite : MonoBehaviour
{
    public Transform doorDirection;
    public float turnSpeed = 180f;

    public void TurnTowardDoor()
    {
        StartCoroutine(TurnCoroutine());
    }

    private System.Collections.IEnumerator TurnCoroutine()
    {
        Vector3 targetDir = doorDirection.position - transform.position;
        Quaternion targetRot = Quaternion.LookRotation(targetDir);

        while (Quaternion.Angle(transform.rotation, targetRot) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                turnSpeed * Time.deltaTime
            );
            yield return null;
        }
    }
}


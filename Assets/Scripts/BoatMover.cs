using UnityEngine;
using System.Collections;

public class BoatMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2f;
    private bool movingToB = true;
    private bool isSinking = false;

    [Header("Bob Settings")]
    public float bobHeight = 0.25f;
    public float bobSpeed = 1.5f;
    private float randomOffset;
    private float baseY;

    [Header("Sinking Settings")]
    public float shakeDuration = 2.5f;
    public float shakeMagnitude = 0.1f;
    public float sinkSpeed = 1f;
    public float sinkDepth = 3f;

    void Start()
    {
        baseY = transform.position.y;
        randomOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        if (isSinking) return;

        MoveBetweenPoints();
        BobOnWaves();
    }

    void MoveBetweenPoints()
    {
        if (!pointA || !pointB) return;

        Transform target = movingToB ? pointB : pointA;

        // Move toward the target
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // Face the target horizontally only
        Vector3 direction = target.position - transform.position;
        direction.y = 0f; // Keep it level
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 2f);
        }

        // Check if reached the target
        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            if (movingToB && !isSinking)
            {
                StartCoroutine(CrashAndSink());
            }
            else if (!isSinking)
            {
                movingToB = !movingToB;
            }
        }
    }

    void BobOnWaves()
    {
        float newY = baseY + Mathf.Sin(Time.time * bobSpeed + randomOffset) * bobHeight;
        Vector3 pos = transform.position;
        pos.y = newY;
        transform.position = pos;
    }

    IEnumerator CrashAndSink()
    {
        isSinking = true;

        Vector3 originalPos = transform.position;
        Quaternion originalRot = transform.rotation;

        // Shake
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float z = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.position = originalPos + new Vector3(x, 0, z);
            transform.rotation = originalRot;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset position and rotation
        transform.position = originalPos;
        transform.rotation = originalRot;

        // Sink straight down
        float targetY = originalPos.y - sinkDepth;
        while (transform.position.y > targetY)
        {
            transform.position += Vector3.down * sinkSpeed * Time.deltaTime;
            yield return null;
        }
    }
}

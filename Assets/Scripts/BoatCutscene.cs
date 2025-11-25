using UnityEngine;
using System.Collections;

public class BoatCutscene : MonoBehaviour
{
    [Header("Path Points")]
    public Transform startPoint;
    public Transform middlePoint;
    public Transform endPoint;

    [Header("Timing")]
    public float toMiddleDuration = 2f;
    public float pauseTime = 1f;
    public float toEndDuration = 2f;

    [Header("Movement Curve")]
    public AnimationCurve easeCurve;

    [Header("Bob & Tilt")]
    public float bobHeight = 0.1f;
    public float bobSpeed = 1.5f;
    public float tiltAmount = 2f;   // X-axis rotation
    public float swayAmount = 1f;   // Z-axis rotation
    public float swaySpeed = 1f;

    [Header("Yaw Settings")]
    public float baseYaw = 90f;     // Base direction the boat faces
    public float yawAmount = 5f;    // Oscillation around base yaw
    public float yawSpeed = 1f;

    private Vector3 targetPathPos;
    private float bobTimer;

    void Start()
    {
        targetPathPos = startPoint.position;
        StartCoroutine(BoatSequence());
    }

    void Update()
    {
        bobTimer += Time.deltaTime;

        // Bobbing
        float y = Mathf.Sin(bobTimer * bobSpeed) * bobHeight;

        // Rotations
        float tilt = Mathf.Sin(bobTimer * bobSpeed) * tiltAmount;
        float sway = Mathf.Sin(bobTimer * swaySpeed) * swayAmount;
        float yaw = baseYaw + Mathf.Sin(bobTimer * yawSpeed) * yawAmount;

        // Apply position
        transform.position = new Vector3(
            targetPathPos.x,
            targetPathPos.y + y,
            targetPathPos.z
        );

        // Apply rotation
        transform.rotation = Quaternion.Euler(tilt, yaw, sway);
    }

    IEnumerator BoatSequence()
    {
        yield return MoveSmooth(startPoint.position, middlePoint.position, toMiddleDuration);
        yield return new WaitForSeconds(pauseTime);
        yield return MoveSmooth(middlePoint.position, endPoint.position, toEndDuration);
    }

    IEnumerator MoveSmooth(Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            float normalized = t / duration;
            float curveValue = easeCurve.Evaluate(normalized);

            targetPathPos = Vector3.Lerp(from, to, curveValue);

            t += Time.deltaTime;
            yield return null;
        }

        yield return SmoothFinish(to);
    }

    IEnumerator SmoothFinish(Vector3 finalPos)
    {
        Vector3 velocity = Vector3.zero;
        float smoothTime = 0.25f;

        while (Vector3.Distance(targetPathPos, finalPos) > 0.001f)
        {
            targetPathPos = Vector3.SmoothDamp(
                targetPathPos,
                finalPos,
                ref velocity,
                smoothTime
            );

            yield return null;
        }

        targetPathPos = finalPos;
    }
}

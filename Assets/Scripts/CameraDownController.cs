using UnityEngine;
using System.Collections;

public class CameraDownController : MonoBehaviour
{
    [Header("Camera Path")]
    public Transform startPoint;    // Where the camera starts
    public Transform endPoint;      // Where the camera ends

    [Header("Movement")]
    public float moveDuration = 3f; // How long it takes to reach endPoint
    public AnimationCurve easeCurve; // Optional easing

    private Vector3 targetPos;

    void Start()
    {
        targetPos = startPoint.position;
        transform.position = targetPos;
        StartCoroutine(MoveCamera());
    }

    void Update()
    {
        // Optional: if you want smooth updates outside coroutine
        transform.position = targetPos;
    }

    IEnumerator MoveCamera()
    {
        float t = 0f;

        while (t < moveDuration)
        {
            float normalized = t / moveDuration;
            float curveValue = easeCurve.Evaluate(normalized);

            targetPos = Vector3.Lerp(startPoint.position, endPoint.position, curveValue);

            t += Time.deltaTime;
            yield return null;
        }

        // Ensure it ends exactly at endPoint
        targetPos = endPoint.position;
    }
}

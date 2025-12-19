using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // For the fade image

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
    public float tiltAmount = 2f;
    public float swayAmount = 1f;
    public float swaySpeed = 1f;

    [Header("Yaw Settings")]
    public float baseYaw = 90f;
    public float yawAmount = 5f;
    public float yawSpeed = 1f;

    [Header("Scene Settings")]
    public string islandSceneName = "Islands";

    [Header("Fade Settings")]
    public Image fadeImage;           // Assign a fullscreen black UI Image
    public float fadeDuration = 1f;   // Duration of fade-to-black

    private Vector3 targetPathPos;
    private float bobTimer;

    void Start()
    {
        targetPathPos = startPoint.position;

        // Make sure fadeImage is fully transparent at start
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(0, 0, 0, 0);
        }

        StartCoroutine(BoatSequence());
    }

    void Update()
    {
        bobTimer += Time.deltaTime;

        float y = Mathf.Sin(bobTimer * bobSpeed) * bobHeight;
        float tilt = Mathf.Sin(bobTimer * bobSpeed) * tiltAmount;
        float sway = Mathf.Sin(bobTimer * swaySpeed) * swayAmount;
        float yaw = baseYaw + Mathf.Sin(bobTimer * yawSpeed) * yawAmount;

        transform.position = new Vector3(
            targetPathPos.x,
            targetPathPos.y + y,
            targetPathPos.z
        );

        transform.rotation = Quaternion.Euler(tilt, yaw, sway);
    }

    IEnumerator BoatSequence()
    {
        yield return MoveSmooth(startPoint.position, middlePoint.position, toMiddleDuration);
        yield return new WaitForSeconds(pauseTime);
        yield return MoveSmooth(middlePoint.position, endPoint.position, toEndDuration);

        // Wait 1 second at the end
        yield return new WaitForSeconds(1f);

        // Start fade and scene switch
        if (fadeImage != null)
            yield return StartCoroutine(FadeToBlack());

        SceneManager.LoadScene(islandSceneName);
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

    IEnumerator FadeToBlack()
    {
        float elapsed = 0f;
        Color startColor = fadeImage.color;
        Color targetColor = new Color(0, 0, 0, 1);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = Color.Lerp(startColor, targetColor, elapsed / fadeDuration);
            yield return null;
        }

        fadeImage.color = targetColor;
    }
}
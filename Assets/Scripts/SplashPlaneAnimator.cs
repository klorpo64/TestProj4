using UnityEngine;

public class SplashPlaneAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public float minWidth = 1f;      // Normal width of splash
    public float maxWidth = 3f;      // Maximum width when splash grows
    public float growSpeed = 5f;     // How fast it grows
    public float pauseTime = 0.05f;  // Short pause before resetting

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        originalScale.x = minWidth;  // Set starting width
        transform.localScale = originalScale;

        StartCoroutine(AnimateSplash());
    }

    System.Collections.IEnumerator AnimateSplash()
    {
        while (true)
        {
            // Grow the width rapidly
            float t = 0f;
            Vector3 targetScale = originalScale;
            targetScale.x = maxWidth;

            while (t < 1f / growSpeed)
            {
                t += Time.deltaTime * growSpeed;
                float width = Mathf.Lerp(minWidth, maxWidth, t);
                transform.localScale = new Vector3(width, originalScale.y, originalScale.z);
                yield return null;
            }

            // Snap back to normal
            transform.localScale = new Vector3(minWidth, originalScale.y, originalScale.z);

            // Optional short pause
            yield return new WaitForSeconds(pauseTime);
        }
    }
}

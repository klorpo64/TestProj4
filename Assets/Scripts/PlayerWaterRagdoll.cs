using UnityEngine;
using System.Collections;
using DialogueEditor;  // Dialogue Editor namespace

public class PlayerWaterRagdoll : MonoBehaviour
{
    [Header("Player Setup")]
    public Rigidbody mainRigidbody;
    public MonoBehaviour movementScript;
    public Animator animator;
    public Rigidbody[] ragdollBodies;

    [Header("Water Settings")]
    public Transform waterCenter;

    [Header("Hop Settings")]
    public float hopHeight = 2f;
    public float hopDuration = 0.8f;
    private bool isHopping = false;
    private Vector3 hopStartPos;
    private Vector3 hopTargetPos;
    private float hopTimer = 0f;

    [Header("Upward Splash Settings")]
    public float upwardLaunchForce = 300f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip splashSound;
    private bool splashPlayed = false;
    private bool ragdollActive = false;

    [Header("UI Settings")]
    public CanvasGroup drownedTextCanvasGroup;
    public float fadeDuration = 2f;

    [Header("Dialogue Settings")]
    public NPCConversation drownConversation;

    void Start()
    {
        mainRigidbody.isKinematic = true;
        mainRigidbody.useGravity = false;
        SetRagdollState(false);

        if (drownedTextCanvasGroup != null)
            drownedTextCanvasGroup.alpha = 0f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water") && !isHopping && !ragdollActive)
        {
            StartHop();
        }
    }

    void StartHop()
    {
        isHopping = true;
        hopStartPos = transform.position;
        hopTargetPos = waterCenter.position;
        hopTimer = 0f;

        if (animator != null)
        {
            if (animator.HasParameterOfType("Hop", AnimatorControllerParameterType.Trigger))
                animator.SetTrigger("Hop");
        }

        if (movementScript != null)
            movementScript.enabled = false;
    }

    void Update()
    {
        if (isHopping)
        {
            hopTimer += Time.deltaTime;
            float t = Mathf.Clamp01(hopTimer / hopDuration);
            float heightOffset = 4f * hopHeight * t * (1 - t);
            transform.position = Vector3.Lerp(hopStartPos, hopTargetPos, t) + Vector3.up * heightOffset;

            Vector3 lookDir = (hopTargetPos - hopStartPos).normalized;
            if (lookDir.sqrMagnitude > 0.01f)
                transform.forward = lookDir;

            if (!splashPlayed && t >= 0.5f)
            {
                splashPlayed = true;
                if (audioSource && splashSound)
                    audioSource.PlayOneShot(splashSound);
            }

            if (t >= 1f)
            {
                isHopping = false;
                ActivateRagdollAndSplash();
            }
        }
    }

    void ActivateRagdollAndSplash()
    {
        ragdollActive = true;

        if (animator != null)
            animator.enabled = false;

        mainRigidbody.isKinematic = false;
        mainRigidbody.useGravity = true;

        SetRagdollState(true);
        UpwardLaunch();

        StartCoroutine(ShowDrownedAndDialogue());
    }

    void SetRagdollState(bool enabled)
    {
        foreach (var rb in ragdollBodies)
        {
            rb.isKinematic = !enabled;
            rb.useGravity = enabled;
        }
    }

    void UpwardLaunch()
    {
        Vector3 forceDir = Vector3.up + new Vector3(Random.Range(-0.1f, 0.1f), 0f, Random.Range(-0.1f, 0.1f));
        foreach (var rb in ragdollBodies)
        {
            rb.AddForce(forceDir.normalized * upwardLaunchForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 50f, ForceMode.Impulse);
        }
        mainRigidbody.AddForce(Vector3.up * (upwardLaunchForce * 0.5f), ForceMode.Impulse);
    }

    private IEnumerator ShowDrownedAndDialogue()
    {
        yield return new WaitForSeconds(3f);

        if (drownedTextCanvasGroup != null)
            yield return StartCoroutine(FadeCanvasGroup(drownedTextCanvasGroup, 0f, 1f, fadeDuration));

        yield return new WaitForSeconds(0.5f);

        if (drownConversation != null)
        {
            ConversationManager.Instance.StartConversation(drownConversation);
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsed = 0f;
        cg.alpha = start;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        cg.alpha = end;
    }
}

public static class AnimatorExtensions
{
    public static bool HasParameterOfType(this Animator self, string name, AnimatorControllerParameterType type)
    {
        foreach (var param in self.parameters)
        {
            if (param.type == type && param.name == name)
                return true;
        }
        return false;
    }
}

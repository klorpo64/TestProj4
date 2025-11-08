using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerDance : MonoBehaviour
{
    private Animator animator;
    private InputAction danceAction;
    private PlayerMovement movement;
    private Coroutine faceCameraRoutine;


    private IEnumerator FaceCamera()
    {
        Transform cam = Camera.main.transform;
        Vector3 targetDirection = -cam.forward;
        targetDirection.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        float t = 0f;
        float duration = 0.25f; // tweak for smoothness

        Quaternion startRot = transform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.rotation = Quaternion.Slerp(startRot, targetRotation, t);
            yield return null;
        }
    }


    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();

        var playerInput = GetComponent<PlayerInput>();
        danceAction = playerInput.actions.FindAction("Dance");
    }

    private void OnEnable()
    {
        if (danceAction != null)
        {
            danceAction.performed += OnDanceStarted;
            danceAction.canceled += OnDanceStopped;
            danceAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (danceAction != null)
        {
            danceAction.performed -= OnDanceStarted;
            danceAction.canceled -= OnDanceStopped;
            danceAction.Disable();
        }
    }

    private void OnDanceStarted(InputAction.CallbackContext ctx)
    {
        animator.SetBool("IsDancing", true);

        if (movement != null)
            movement.enabled = false;

        if (faceCameraRoutine != null)
            StopCoroutine(faceCameraRoutine);

        faceCameraRoutine = StartCoroutine(FaceCamera());
    }

    private void OnDanceStopped(InputAction.CallbackContext ctx)
    {
        animator.SetBool("IsDancing", false);

        if (movement != null)
            movement.enabled = true;

        if (faceCameraRoutine != null)
            StopCoroutine(faceCameraRoutine);
    }


}

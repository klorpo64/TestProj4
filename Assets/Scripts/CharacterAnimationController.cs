using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator; // assign in Inspector

    public void SetTalking(bool talking)
    {
        if (animator == null) return; // safety check
        animator.SetBool("IsTalking", talking);
        Debug.Log("Set IsTalking = " + talking);
    }
}

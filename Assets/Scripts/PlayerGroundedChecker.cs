using UnityEngine;

public class PlayerGroundedChecker : MonoBehaviour
{
    private CharacterController controller;

    public bool IsGrounded => controller != null && controller.isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
}
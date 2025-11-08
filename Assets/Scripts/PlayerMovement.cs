using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;

    private float movementX;
    private float movementY;

    public float speed = 5f;
    public float rotationSpeed = 10f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(movementX, 0.0f, movementY);
        float currentSpeed = movement.magnitude;

        animator.SetFloat("Speed", currentSpeed);

        if (currentSpeed > 0.01f)
        {
            // Move player using CharacterController
            controller.Move(movement.normalized * speed * Time.fixedDeltaTime);

            // Rotate toward direction of movement
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        if (animator.GetBool("IsDancing"))
        {
            Vector3 lookDirection = Camera.main.transform.forward;
            lookDirection.y = 0;
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
        Vector3 pos = transform.position;
        pos.y = -0.111f;
        transform.position = pos;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Animator animator;

    private float movementX;
    private float movementY;
    
    public float speed = 5f;
    public float rotationSpeed = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
        // Create a movement vector from input
        Vector3 movement = new Vector3(movementX, 0.0f, movementY);

        // Calculate the actual speed magnitude
        float currentSpeed = movement.magnitude;

        // Update animation parameter (so Speed > 0.01 plays run animation)
        GetComponent<Animator>().SetFloat("Speed", currentSpeed);

        // Only move if we have significant input
        if (currentSpeed > 0.01f)
        {
            // Move player
            transform.Translate(movement.normalized * speed * Time.fixedDeltaTime, Space.World);

            // Smoothly rotate player in the direction of movement
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        if (animator.GetBool("IsDancing"))
        {
            // Keep facing the camera
            Vector3 lookDirection = Camera.main.transform.forward;
            lookDirection.y = 0;
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public Animator animator;
    public CharacterController controller;

    public Transform Cam;

    public CameraController cameraController;

    private Vector3 playerVelocity;
    private bool groundedPlayer;
    public float playerSpeed = 2f;
    public float runSpeed = 4f;
    public float jumpSpeed = 1.5f;
    public float gravityValue = -10f;
    public float turnSmoothness;

    public float turnSmoothing = 0.1f;
    public float speedDampTime = 0.1f;
    public float groundCheckDistance = 0.3f; // Adjust this to match the terrain

    void Update()
    {
        // Use Raycast to check if grounded and ensure player stays grounded on slopes
        groundedPlayer = controller.isGrounded || IsGroundedByRaycast();

        // Reset vertical velocity if grounded and not jumping
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        float h = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool isFirstPerson = cameraController.isFirstPerson;
        bool isTransitioning = cameraController.isTransitioning;

        if (isFirstPerson || isTransitioning)
        {
            h = 0;
            z = Mathf.Clamp(z, 0, 1);
        }

        Vector3 move = new Vector3(h, 0, z).normalized;

        if (move.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg + Cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothness, turnSmoothing);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : playerSpeed;
            controller.Move(moveDirection * speed * Time.deltaTime);
        }
        else
        {
            controller.Move(Vector3.zero);
        }

        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : playerSpeed;
        float movementSpeed = Mathf.Clamp(move.magnitude * targetSpeed, 0f, targetSpeed);
        animator.SetFloat("speed", movementSpeed, speedDampTime, Time.deltaTime);

        // Jump logic with gravity control
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y = Mathf.Sqrt(jumpSpeed * -2.0f * gravityValue); // Set jump velocity
            groundedPlayer = false; // Temporarily set grounded to false for jump to take effect
        }

        // Apply gravity continuously
        playerVelocity.y += gravityValue * Time.deltaTime;

        // If grounded and not jumping, keep the player grounded on slopes
        if (groundedPlayer && playerVelocity.y <= 0)
        {
            StickToGround();
        }

        // Move character based on velocity (including vertical movement from gravity and jumps)
        controller.Move(playerVelocity * Time.deltaTime);

        // Update animator states based on grounded status
        animator.SetBool("fall", !groundedPlayer);

        if (move.magnitude > 0 && Input.GetKey(KeyCode.LeftShift))
        {
            animator.SetBool("run", true);
        }
        else
        {
            animator.SetBool("run", false);
        }
    }

    private bool IsGroundedByRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance + controller.skinWidth))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            return slopeAngle <= controller.slopeLimit;
        }
        return false;
    }

    private void StickToGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance + controller.skinWidth))
        {
            Vector3 targetPosition = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            controller.Move(targetPosition - transform.position);
        }
    }
}

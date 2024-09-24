using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public Animator animator;
    public CharacterController controller;

    public Transform Cam;

    private Vector3 playerVelocity;
    private bool groundedPlayer;
    public float playerSpeed = 2f;
    public float runSpeed = 4f;
    public float jumpSpeed = 1.5f;
    public float gravityValue = -10f;
    public float turnSmoothness;

    public float turnSmoothing = 0.1f;
    public float speedDampTime = 0.1f;  // Damping time for smooth transitions

    void Update()
    {
        groundedPlayer = controller.isGrounded;

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        // Get movement inputs
        float h = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0, z).normalized;

        // If player is moving
        if (move.magnitude >= 0.1f)
        {
            // Player Direction
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg + Cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothness, turnSmoothing);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Apply correct speed: Run if Shift is held and player is moving, otherwise walk
            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : playerSpeed;
            controller.Move(moveDirection * speed * Time.deltaTime);
        }
        else
        {
            // When no input is detected, stop the character from moving immediately.
            controller.Move(Vector3.zero);
        }

        // Animator sync: Adjust speed parameter based on actual movement magnitude with damping
        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : playerSpeed;
        float movementSpeed = Mathf.Clamp(move.magnitude * targetSpeed, 0f, targetSpeed);
        animator.SetFloat("speed", movementSpeed, speedDampTime, Time.deltaTime);  // Smooth transition using damping

        // Jump
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpSpeed * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        if (groundedPlayer)
        {
            animator.SetBool("fall", false);
        }
        else
        {
            animator.SetBool("fall", true);
        }

        // Only set run animation if movement is present and the player is pressing shift
        if (move.magnitude > 0 && Input.GetKey(KeyCode.LeftShift))
        {
            animator.SetBool("run", true);
        }
        else
        {
            animator.SetBool("run", false);  // Stop run animation when there's no movement, even if Shift is held
        }
    }
}

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
    private bool wasGroundedLastFrame = true; // Track previous grounded state
    public float playerSpeed = 2f;
    public float runSpeed = 4f;
    public float jumpSpeed = 1.5f;
    public float gravityValue = -10f;
    public float turnSmoothness;
    public float turnSmoothing = 0.1f;
    public float speedDampTime = 0.1f;
    public float groundCheckDistance = 0.3f;

    // Audio
    public AudioSource audioSource;
    public AudioClip walkClip;
    public AudioClip jumpClip;
    public AudioClip landClip;

    private bool isSprinting = false;
    private bool isWalkingSoundPlaying = false;
    // Variables for jump cooldown
    private float jumpCooldown = 1.5f;
    private float lastJumpTime = -1.5f;

    void Update()
    {
        groundedPlayer = controller.isGrounded || IsGroundedByRaycast();

        // Play landing sound immediately when grounded after a jump
        if (groundedPlayer && !wasGroundedLastFrame)
        {
            PlaySound(landClip);
        }

        // Update the grounded state tracker
        wasGroundedLastFrame = groundedPlayer;

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
            isSprinting = Input.GetKey(KeyCode.LeftShift);
            float speed = isSprinting ? runSpeed : playerSpeed;
            controller.Move(moveDirection * speed * Time.deltaTime);

            // Play walking sound, adjusting for sprinting speed
            if (!isWalkingSoundPlaying)
            {
                audioSource.clip = walkClip;
                audioSource.pitch = isSprinting ? 1.5f : 1f;
                audioSource.loop = true;
                audioSource.Play();
                isWalkingSoundPlaying = true;
            }
        }
        else
        {
            if (isWalkingSoundPlaying)
            {
                audioSource.Stop();
                isWalkingSoundPlaying = false;
            }
        }

        float targetSpeed = isSprinting ? runSpeed : playerSpeed;
        float movementSpeed = Mathf.Clamp(move.magnitude * targetSpeed, 0f, targetSpeed);
        animator.SetFloat("speed", movementSpeed, speedDampTime, Time.deltaTime);

        // Jump logic with cooldown
        if (Input.GetButtonDown("Jump") && groundedPlayer && IsGroundedByRaycast() && Time.time - lastJumpTime > jumpCooldown)
        {
            playerVelocity.y = Mathf.Sqrt(jumpSpeed * -2.0f * gravityValue);
            groundedPlayer = false;
            PlaySound(jumpClip); // Play jump sound immediately
            lastJumpTime = Time.time; // Update last jump time
        }

        playerVelocity.y += gravityValue * Time.deltaTime;

        if (groundedPlayer && playerVelocity.y <= 0)
        {
            StickToGround();
        }

        controller.Move(playerVelocity * Time.deltaTime);
        animator.SetBool("fall", !groundedPlayer);

        if (move.magnitude > 0 && isSprinting)
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
            if (slopeAngle > controller.slopeLimit)
            {
                return false;
            }
            return true;
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

    private void PlaySound(AudioClip clip)
    {
        audioSource.Stop();
        audioSource.clip = clip;

        if (clip == walkClip)
        {
            audioSource.pitch = isSprinting ? 1.5f : 1f; // Adjust pitch for walking sound based on sprint
            audioSource.loop = true; // Loop only for walking sound
        }
        else
        {
            audioSource.pitch = 1f; // Ensure standard pitch for non-walking sounds like jump and land
            audioSource.loop = false;
        }

        audioSource.Play();
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Ensure landing sound plays immediately on contact
        if (!groundedPlayer && controller.isGrounded && hit.normal.y > 0.5f)
        {
            PlaySound(landClip);
        }
    }
}
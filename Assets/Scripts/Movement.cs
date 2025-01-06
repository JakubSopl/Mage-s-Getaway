using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    public float groundCheckDistance = 0.3f;

    // Audio
    public AudioSource audioSource;
    public AudioClip walkClip;
    public AudioClip jumpClip;
    public AudioClip landClip;

    private bool isSprinting = false;

    private float jumpCooldown = 1.5f;
    private float lastJumpTime = -1.5f;

    // Respawn
    public Transform respawnPoint; // Starting respawn point
    public Transform respawnPoint2; // New respawn point for FinalCheckpoint
    public Image fadeImage; // Assign the blue image here
    public float fadeDuration = 0.2f;

    private bool isRespawning = false;
    private bool finalCheckpointReached = false; // Track if the final checkpoint is reached

    // Detection area
    public float detectionRadius = 0.1f; // Radius for detecting "Ocean" area

    void Update()
    {
        groundedPlayer = controller.isGrounded || IsGroundedByRaycast();

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
        }

        float targetSpeed = isSprinting ? runSpeed : playerSpeed;
        float movementSpeed = Mathf.Clamp(move.magnitude * targetSpeed, 0f, targetSpeed);
        animator.SetFloat("speed", movementSpeed, speedDampTime, Time.deltaTime);

        if (Input.GetButtonDown("Jump") && groundedPlayer && IsGroundedByRaycast() && Time.time - lastJumpTime > jumpCooldown)
        {
            playerVelocity.y = Mathf.Sqrt(jumpSpeed * -2.0f * gravityValue);
            groundedPlayer = false;
            lastJumpTime = Time.time;
        }

        if (groundedPlayer)
        {
            if (playerVelocity.y < 0)
            {
                StickToGround();
                playerVelocity.y = 0; // Reset pádové rychlosti pøi kontaktu se zemí
            }
        }
        else
        {
            // Pøidání plynulé gravitace, pokud hráè není na zemi
            playerVelocity.y += gravityValue * Time.deltaTime;

            // Omezíme maximální rychlost pádu, aby byl pád pøirozenìjší
            float maxFallSpeed = -20f; // Nastavte maximální rychlost pádu
            playerVelocity.y = Mathf.Max(playerVelocity.y, maxFallSpeed);
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ocean") && !isRespawning)
        {
            StartCoroutine(Respawn());
        }
        else if (other.CompareTag("FinalCheckpoint"))
        {
            SetNewRespawnPoint();
        }
    }

    private void SetNewRespawnPoint()
    {
        if (respawnPoint2 != null)
        {
            respawnPoint = respawnPoint2;
            finalCheckpointReached = true;
            Debug.Log("Final checkpoint reached. Respawn point updated.");
        }
    }

    private IEnumerator Respawn()
    {
        if (isRespawning) yield break; // Prevent multiple calls
        isRespawning = true; // Set the flag

        Debug.Log("Respawning player...");

        // Fade-in while the player is falling
        Color fadeColor = fadeImage.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            fadeColor.a = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            fadeImage.color = fadeColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadeColor.a = 1f;
        fadeImage.color = fadeColor;

        // Respawn logic: Move the player to the respawn point
        Debug.Log("Resetting player position...");
        Vector3 resetPosition = respawnPoint.position;
        controller.enabled = false; // Temporarily disable controller for position reset
        transform.position = resetPosition; // Directly move the player
        controller.enabled = true; // Re-enable the controller after position reset
        playerVelocity = Vector3.zero; // Reset velocity to avoid sliding

        // Fade-out logic after respawn
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            fadeColor.a = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            fadeImage.color = fadeColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadeColor.a = 0f;
        fadeImage.color = fadeColor;

        isRespawning = false; // Allow future respawns
        Debug.Log("Respawn complete.");
    }
}

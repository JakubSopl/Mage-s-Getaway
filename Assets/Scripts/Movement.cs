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

    // Stabilizace detekce zemì
    private float groundTimeThreshold = 0.2f; // Doba, po kterou hráè musí být ve vzduchu, aby se poèítal jako padající
    private float lastGroundedTime; // Poslední èas, kdy byl hráè na zemi

    // Lepení k zemi
    public float stickToGroundForce = -5f; // Dodateèná síla pro pøilepení k zemi
    public int groundCheckRayCount = 5; // Poèet Raycastù pro detekci schodù

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
    public Image fadeImage; // Assign the fade image here
    public float fadeDuration = 0.2f;

    private bool isRespawning = false;
    private bool finalCheckpointReached = false; // Track if the final checkpoint is reached

    void Update()
    {
        // Zjištìní, zda je hráè na zemi
        bool isGroundedNow = IsGroundedByMultipleRaycasts() || (controller.isGrounded && !IsInTrigger());

        // Pokud je hráè na zemi, aktualizujeme èas
        if (isGroundedNow)
        {
            lastGroundedTime = Time.time;
            groundedPlayer = true;
        }
        else
        {
            // Používáme èasovou toleranci, aby pøechod na padání nebyl okamžitý
            groundedPlayer = Time.time - lastGroundedTime <= groundTimeThreshold;
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
            isSprinting = Input.GetKey(KeyCode.LeftShift);
            float speed = isSprinting ? runSpeed : playerSpeed;
            controller.Move(moveDirection * speed * Time.deltaTime);
        }

        float targetSpeed = isSprinting ? runSpeed : playerSpeed;
        float movementSpeed = Mathf.Clamp(move.magnitude * targetSpeed, 0f, targetSpeed);
        animator.SetFloat("speed", movementSpeed, speedDampTime, Time.deltaTime);

        // Skok s cooldownem
        if (Input.GetButtonDown("Jump") && groundedPlayer && Time.time - lastJumpTime > jumpCooldown)
        {
            playerVelocity.y = Mathf.Sqrt(jumpSpeed * -2.0f * gravityValue);
            groundedPlayer = false;
            lastJumpTime = Time.time;
        }

        // Gravitaèní logika
        if (groundedPlayer)
        {
            if (playerVelocity.y < 0)
            {
                playerVelocity.y = stickToGroundForce; // Lepení k zemi
            }
        }
        else
        {
            playerVelocity.y += gravityValue * Time.deltaTime;
            playerVelocity.y = Mathf.Max(playerVelocity.y, -20f); // Omezíme maximální rychlost pádu
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

    private bool IsGroundedByMultipleRaycasts()
    {
        float step = controller.radius / (groundCheckRayCount - 1);
        for (int i = 0; i < groundCheckRayCount; i++)
        {
            Vector3 origin = transform.position + new Vector3(-controller.radius + step * i, 0, 0);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCheckDistance + controller.skinWidth))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (slopeAngle <= controller.slopeLimit && !hit.collider.isTrigger)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsInTrigger()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.1f);
        foreach (var collider in colliders)
        {
            if (collider.isTrigger && collider.CompareTag("Ocean"))
            {
                return true;
            }
        }
        return false;
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

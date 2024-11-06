using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepSound : MonoBehaviour
{
    public AudioClip concreteClip;
    public AudioClip dirtClip;
    public AudioClip woodClip;
    public AudioClip waterClip;
    public float walkStepInterval = 0.5f;
    public float runStepInterval = 0.3f;

    private AudioSource audioSource;
    private Movement movementScript;
    private float footstepTimer = 0f;
    private AudioClip currentClip;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        movementScript = GetComponent<Movement>();

        if (movementScript == null)
        {
            Debug.LogError("FootstepSound: Movement script not found on the player!");
        }
    }

    void Update()
    {
        // Check if player is grounded and moving based on input
        if (movementScript == null || !movementScript.controller.isGrounded || !IsPlayerMovingWithInput())
        {
            StopFootstepSound();
            return; // Exit if the player is not grounded or not moving with input
        }

        // Check the ground type and play footstep sound if moving
        CheckGroundType();
        PlayFootstepSound();
    }

    bool IsPlayerMovingWithInput()
    {
        // Check for horizontal input directly from the player's movement keys
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Return true if there is significant input
        return Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;
    }

    void CheckGroundType()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, movementScript.groundCheckDistance + movementScript.controller.skinWidth))
        {
            // Set the appropriate footstep clip based on ground tag
            switch (hit.collider.tag)
            {
                case "Concrete":
                    currentClip = concreteClip;
                    break;
                case "Dirt":
                    currentClip = dirtClip;
                    break;
                case "Wood":
                    currentClip = woodClip;
                    break;
                case "Water":
                    currentClip = waterClip;
                    break;
                default:
                    currentClip = null;
                    break;
            }
        }
        else
        {
            currentClip = null;
        }
    }

    void PlayFootstepSound()
    {
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float stepInterval = isRunning ? runStepInterval : walkStepInterval;

        // Only play the footstep sound if timer has elapsed and we have a valid clip
        if (footstepTimer <= 0f && currentClip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f); // Add pitch variation for realism
            audioSource.PlayOneShot(currentClip);
            footstepTimer = stepInterval;
        }

        footstepTimer -= Time.deltaTime; // Decrease timer over time
    }

    void StopFootstepSound()
    {
        // Stop any ongoing footstep sound if the player is idle
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}

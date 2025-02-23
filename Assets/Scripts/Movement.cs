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

    // Stabilizace detekce zem�
    private float groundTimeThreshold = 0.2f; // Doba, po kterou hr�� mus� b�t ve vzduchu, aby se po��tal jako padaj�c�
    private float lastGroundedTime; // Posledn� �as, kdy byl hr�� na zemi

    // Lepen� k zemi
    public float stickToGroundForce = -5f; // Dodate�n� s�la pro p�ilepen� k zemi
    public int groundCheckRayCount = 5; // Po�et Raycast� pro detekci schod�

    public AudioSource audioSource;
    public AudioClip footstepsClip;
    public AudioClip fallingClip;
    public AudioClip idleClip;

    private bool isSprinting = false;

    private float jumpCooldown = 1.5f;
    private float lastJumpTime = -1.5f;

    // Respawn
    public Transform respawnPoint; // Starting respawn point
    public Transform respawnPoint2; // New respawn point for FinalCheckpoint
    public Image fadeImage; // Assign the fade image here
    public float fadeDuration = 0.2f;

    private bool isRespawning = false;

    // P�idan� prom�nn� pro re�im boje
    public bool isInBattle = false;
    private bool wasGroundedLastFrame;
    private bool isFalling = false;
    private bool isIdlePlaying = false;
    private bool isFootstepsPlaying = false;
    private float lastSoundTime = 0f;
    private float idleDelay = 1f; // Idle zvuk se p�ehraje a� po 1s ne�innosti

    void Update()
    {

        if (isInBattle) return; // Disable movement in battle mode

        // Zji�t�n�, zda je hr�� na zemi
        bool isGroundedNow = IsGroundedByMultipleRaycasts() || (controller.isGrounded && !IsInTrigger());

        if (isGroundedNow)
        {
            lastGroundedTime = Time.time;
            groundedPlayer = true;
        }
        else
        {
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

        // Skok zvuk
        if (Input.GetButtonDown("Jump") && groundedPlayer && Time.time - lastJumpTime > jumpCooldown)
        {
            playerVelocity.y = Mathf.Sqrt(jumpSpeed * -2.0f * gravityValue);
            groundedPlayer = false;
            lastJumpTime = Time.time;
            SoundManager.Instance.PlaySound("Jump");
        }

        // Gravita�n� logika
        if (groundedPlayer)
        {
            if (playerVelocity.y < 0)
            {
                playerVelocity.y = stickToGroundForce; // Lepen� k zemi
            }
        }
        else
        {
            playerVelocity.y += gravityValue * Time.deltaTime;
            playerVelocity.y = Mathf.Max(playerVelocity.y, -20f); // Omez�me maxim�ln� rychlost p�du

            if (!isGroundedNow && !audioSource.isPlaying)
            {
                Debug.Log("Playing falling sound");
                audioSource.clip = fallingClip;
                audioSource.loop = false;
                audioSource.Play();
            }
            else if (isGroundedNow && audioSource.isPlaying && audioSource.clip == fallingClip)
            {
                Debug.Log("Stopping falling sound");
                audioSource.Stop();
            }
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

        HandleFootsteps(movementSpeed);
        HandleLanding();
        HandleFallingSound();
        HandleIdleSound();
    }

    private Coroutine fadeOutRoutine;
    private float fallingVolume = 0.2f; // Po��te�n� hlasitost
    private float maxFallingVolume = 1f; // Maxim�ln� hlasitost
    private float fallingTime = 0f; // Jak dlouho hr�� pad�

    private void HandleFallingSound()
    {
        bool isGrounded = IsGroundedByMultipleRaycasts() || (controller.isGrounded && !IsInTrigger());

        if (!isGrounded)
        {
            if (!isFalling || audioSource.clip != fallingClip)
            {
                Debug.Log("Playing Falling Sound");
                audioSource.clip = fallingClip;
                audioSource.loop = true;
                audioSource.volume = fallingVolume;
                audioSource.Play();
                isFalling = true;
                fallingTime = 0f;

                if (fadeOutRoutine != null)
                {
                    StopCoroutine(fadeOutRoutine);
                    fadeOutRoutine = null;
                }
            }
            fallingTime += Time.deltaTime;
            float volumeIncrease = Mathf.Clamp01(fallingTime / 2f);
            audioSource.volume = Mathf.Lerp(fallingVolume, maxFallingVolume, volumeIncrease);
        }
        else
        {
            if (isFalling && audioSource.clip == fallingClip)
            {
                Debug.Log("Fading out Falling Sound");
                fadeOutRoutine = StartCoroutine(SmoothFadeOutFallingSound(1f));
                isFalling = false;
            }
        }
    }

    private IEnumerator SmoothFadeOutFallingSound(float duration)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            audioSource.volume = startVolume * Mathf.Pow(1 - progress, 2);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = 1f; //  RESET hlasitosti na 1 po dopadu
    }

    private void HandleIdleSound()
    {
        bool isMoving = animator.GetFloat("speed") > 0;
        bool isOtherSoundPlaying = isFootstepsPlaying || isFalling ||
                                   (audioSource.isPlaying && audioSource.clip != idleClip);
        float currentTime = Time.time;

        if (groundedPlayer && !isMoving && !isOtherSoundPlaying)
        {
            if (!isIdlePlaying)
            {
                Debug.Log("Playing Idle Sound");

                //  V�dy stopni p�edchoz� zvuk, aby nebyl blokov�n
                audioSource.Stop();
                audioSource.clip = idleClip;
                audioSource.loop = true;
                audioSource.volume = 1f;
                audioSource.Play();

                isIdlePlaying = true;
                isFootstepsPlaying = false;
                isFalling = false;
            }
        }
        else
        {
            if (isIdlePlaying && (isMoving || isOtherSoundPlaying))
            {
                Debug.Log("Stopping Idle Sound");
                audioSource.Stop();
                isIdlePlaying = false;
                lastSoundTime = currentTime;
            }
        }
    }

    private void HandleFootsteps(float speed)
    {
        if (isInBattle)
        {
            if (audioSource.isPlaying && isFootstepsPlaying)
            {
                Debug.Log("Stopping Footsteps Sound (Battle)");
                audioSource.Stop();
                isFootstepsPlaying = false;
            }
            return;
        }

        if (groundedPlayer && speed > 0)
        {
            float targetPitch = isSprinting ? 0.8f : 0.5f; // Lep�� p�echod mezi ch�z� a b�hem

            if (!isFootstepsPlaying || audioSource.clip != footstepsClip)
            {
                Debug.Log("Playing Footsteps Sound");
                audioSource.Stop();
                audioSource.clip = footstepsClip;
                audioSource.loop = true;
                audioSource.volume = 1.0f;
                audioSource.pitch = targetPitch;
                audioSource.Play();

                isFootstepsPlaying = true;
            }
            else if (audioSource.isPlaying && Mathf.Abs(audioSource.pitch - targetPitch) > 0.05f)
            {
                // Pokud se zm�n� rychlost, uprav�me plynule pitch
                audioSource.pitch = targetPitch;
            }
            else if (!audioSource.isPlaying)
            {
                Debug.Log("Restarting Footsteps Sound");
                audioSource.Play();
            }
        }
        else
        {
            if (isFootstepsPlaying && audioSource.clip == footstepsClip)
            {
                Debug.Log("Stopping Footsteps Sound (Idle or Jumping)");
                audioSource.Stop();
                audioSource.clip = null;
                isFootstepsPlaying = false;
            }
        }

        // Pokud hr�� dopadne na zem a st�le dr�� pohyb, obnov�me zvuk
        if (groundedPlayer && !isFootstepsPlaying && speed > 0)
        {
            Debug.Log("Resuming Footsteps Sound after Landing");
            audioSource.clip = footstepsClip;
            audioSource.loop = true;
            audioSource.volume = 1.0f;
            audioSource.pitch = isSprinting ? 1.0f : 0.75f;
            audioSource.Play();
            isFootstepsPlaying = true;
        }
    }
    private void HandleLanding()
    {
        bool isGroundedNow = IsGroundedByMultipleRaycasts() || (controller.isGrounded && !IsInTrigger());

        if (isGroundedNow)
        {
            lastGroundedTime = Time.time;
            groundedPlayer = true;

            // P�ehraje zvuk dopadu, pokud hr�� p�edt�m nebyl na zemi
            if (!wasGroundedLastFrame)
            {
                SoundManager.Instance.PlaySound("Land");
            }
        }
        else
        {
            groundedPlayer = Time.time - lastGroundedTime <= groundTimeThreshold;
        }

        wasGroundedLastFrame = groundedPlayer; // Ulo�� aktu�ln� stav pro dal�� sn�mek
    }

    public void EnterBattleMode(Vector3 battlePosition)
    {
        isInBattle = true;

        // Deaktivace pohybu a p�em�st�n� hr��e na pozici
        controller.enabled = false; // Temporarily disable controller
        transform.position = battlePosition;
        controller.enabled = true; // Re-enable controller

        // Reset animac�
        animator.SetFloat("speed", 0);
        animator.SetBool("run", false);

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void ExitBattleMode()
    {
        isInBattle = false;

        // Pohyb a dal�� akce jsou op�t povoleny
        animator.SetFloat("speed", 0);
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public Animator animator;
    public CharacterController controller;

    public Transform Cam;

    // Pøidej veøejnou promìnnou pro CameraController
    public CameraController cameraController;

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

        // Ovìøíme, jestli jsme v režimu první osoby
        bool isFirstPerson = cameraController.isFirstPerson;

        // Pokud jsme v první osobì, povolíme pouze dopøedný pohyb pomocí klávesy "W"
        if (isFirstPerson)
        {
            h = 0; // Zakážeme horizontální pohyb (A a D)
            z = Mathf.Clamp(z, 0, 1); // Povolíme pouze dopøedný pohyb (W), zakážeme pohyb vzad (S)
        }

        Vector3 move = new Vector3(h, 0, z).normalized;

        // Pokud se hráè pohybuje
        if (move.magnitude >= 0.1f)
        {
            // Smìr pohybu hráèe
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg + Cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothness, turnSmoothing);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Aplikujeme správnou rychlost: Bìh pokud je zmáèknuto Shift, jinak chùze
            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : playerSpeed;
            controller.Move(moveDirection * speed * Time.deltaTime);
        }
        else
        {
            // Když není detekován žádný vstup, zastavíme postavu okamžitì
            controller.Move(Vector3.zero);
        }

        // Animator sync: Adjust speed parameter based on actual movement magnitude with damping
        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : playerSpeed;
        float movementSpeed = Mathf.Clamp(move.magnitude * targetSpeed, 0f, targetSpeed);
        animator.SetFloat("speed", movementSpeed, speedDampTime, Time.deltaTime);  // Smooth transition using damping

        // Skok
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpSpeed * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        // Animace pádu
        if (groundedPlayer)
        {
            animator.SetBool("fall", false);
        }
        else
        {
            animator.SetBool("fall", true);
        }

        // Nastavení animace bìhu, pokud se hráè pohybuje a drží Shift
        if (move.magnitude > 0 && Input.GetKey(KeyCode.LeftShift))
        {
            animator.SetBool("run", true);
        }
        else
        {
            animator.SetBool("run", false);  // Zastavení bìhu, pokud není pohyb
        }
    }
}

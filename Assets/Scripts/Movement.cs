using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public Animator animator;
    public CharacterController controller;

    public Transform Cam;

    // P�idej ve�ejnou prom�nnou pro CameraController
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

        // Ov���me, jestli jsme v re�imu prvn� osoby
        bool isFirstPerson = cameraController.isFirstPerson;

        // Pokud jsme v prvn� osob�, povol�me pouze dop�edn� pohyb pomoc� kl�vesy "W"
        if (isFirstPerson)
        {
            h = 0; // Zak�eme horizont�ln� pohyb (A a D)
            z = Mathf.Clamp(z, 0, 1); // Povol�me pouze dop�edn� pohyb (W), zak�eme pohyb vzad (S)
        }

        Vector3 move = new Vector3(h, 0, z).normalized;

        // Pokud se hr�� pohybuje
        if (move.magnitude >= 0.1f)
        {
            // Sm�r pohybu hr��e
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg + Cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothness, turnSmoothing);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Aplikujeme spr�vnou rychlost: B�h pokud je zm��knuto Shift, jinak ch�ze
            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : playerSpeed;
            controller.Move(moveDirection * speed * Time.deltaTime);
        }
        else
        {
            // Kdy� nen� detekov�n ��dn� vstup, zastav�me postavu okam�it�
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

        // Animace p�du
        if (groundedPlayer)
        {
            animator.SetBool("fall", false);
        }
        else
        {
            animator.SetBool("fall", true);
        }

        // Nastaven� animace b�hu, pokud se hr�� pohybuje a dr�� Shift
        if (move.magnitude > 0 && Input.GetKey(KeyCode.LeftShift))
        {
            animator.SetBool("run", true);
        }
        else
        {
            animator.SetBool("run", false);  // Zastaven� b�hu, pokud nen� pohyb
        }
    }
}

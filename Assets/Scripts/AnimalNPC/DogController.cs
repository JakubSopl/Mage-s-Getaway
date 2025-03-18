using UnityEngine;
using System.Collections;

public class DogBehavior : MonoBehaviour
{
    public Animator dogAnimator;       // Animator psa
    public Transform player;           // Transform hráèe
    public AudioSource audioSource;    // Audio source pro pøehrávání zvukù (mùže být nepøiøazen)
    public AudioClip barkSound;        // Zvuk štìkání

    public float angryDistance = 5.0f; // Vzdálenost, pøi které se pes naštve
    public float lookDistance = 15.0f; // Vzdálenost, pøi které se pes otáèí za hráèem
    public float rotationSpeed = 2.0f; // Rychlost otáèení psa smìrem k hráèi

    private bool isAngryLoopActive = false;   // Kontroluje, zda je smyèka aktivní
    private Coroutine angryLoopCoroutine = null;
    private bool isBarking = false;           // Kontroluje, zda právì hraje zvuk štìkání

    void Start()
    {
        dogAnimator.SetBool("isSitting", true);
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= angryDistance && !isAngryLoopActive)
        {
            isAngryLoopActive = true;
            dogAnimator.SetBool("isSitting", false);
            angryLoopCoroutine = StartCoroutine(AngryTailLoop());

            // **Spustí zvuk pouze, pokud ještì nehraje**
            if (!isBarking && audioSource != null && barkSound != null)
            {
                audioSource.clip = barkSound;
                audioSource.loop = true; //  Zvuk se bude opakovat
                audioSource.Play();
                isBarking = true;
            }
        }
        else if (distance > angryDistance && isAngryLoopActive)
        {
            isAngryLoopActive = false;
            if (angryLoopCoroutine != null)
            {
                StopCoroutine(angryLoopCoroutine);
                angryLoopCoroutine = null;
            }

            // Pes **ještì nepøestává** štìkat, dokud se nevrátí do idlu
            dogAnimator.SetTrigger("isWigglingTail");
            StartCoroutine(WaitAndSit());
        }

        // Otáèení psa smìrem k hráèi
        if (distance <= lookDistance)
        {
            RotateTowardsPlayer();
        }
    }

    IEnumerator AngryTailLoop()
    {
        while (isAngryLoopActive)
        {
            dogAnimator.SetTrigger("isAngry");
            yield return new WaitForSeconds(0.5f);
            dogAnimator.SetTrigger("isWigglingTail");
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator WaitAndSit()
    {
        yield return new WaitForSeconds(1f);
        dogAnimator.SetBool("isSitting", true);

        //  **Zvuk se zastaví až pøi pøechodu do sedící animace (idle)**
        if (isBarking && audioSource != null)
        {
            audioSource.Stop();
            isBarking = false;
        }
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}

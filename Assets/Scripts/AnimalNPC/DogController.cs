using UnityEngine;
using System.Collections;

public class DogBehavior : MonoBehaviour
{
    public Animator dogAnimator;       // Animator psa
    public Transform player;           // Transform hr��e
    public AudioSource audioSource;    // Audio source pro p�ehr�v�n� zvuk� (m��e b�t nep�i�azen)
    public AudioClip barkSound;        // Zvuk �t�k�n�

    public float angryDistance = 5.0f; // Vzd�lenost, p�i kter� se pes na�tve
    public float lookDistance = 15.0f; // Vzd�lenost, p�i kter� se pes ot��� za hr��em
    public float rotationSpeed = 2.0f; // Rychlost ot��en� psa sm�rem k hr��i

    private bool isAngryLoopActive = false;   // Kontroluje, zda je smy�ka aktivn�
    private Coroutine angryLoopCoroutine = null;
    private bool isBarking = false;           // Kontroluje, zda pr�v� hraje zvuk �t�k�n�

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

            // **Spust� zvuk pouze, pokud je�t� nehraje**
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

            // Pes **je�t� nep�est�v�** �t�kat, dokud se nevr�t� do idlu
            dogAnimator.SetTrigger("isWigglingTail");
            StartCoroutine(WaitAndSit());
        }

        // Ot��en� psa sm�rem k hr��i
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

        //  **Zvuk se zastav� a� p�i p�echodu do sed�c� animace (idle)**
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

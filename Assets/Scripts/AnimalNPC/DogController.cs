using UnityEngine;
using System.Collections;

public class DogBehavior : MonoBehaviour
{
    public Animator dogAnimator;       // Animator psa
    public Transform player;           // Transform hr��e

    public float angryDistance = 5.0f;        // Nastaven� vzd�lenosti, p�i kter� se pes stane na�tvan�m
    public float lookDistance = 15.0f;        // Nastaven� vzd�lenosti, p�i kter� se pes ot��� za hr��em
    public float rotationSpeed = 2.0f; // Rychlost ot��en� psa sm�rem k hr��i

    private bool isAngryLoopActive = false;   // Kontroluje, zda je smy�ka aktivn�
    private Coroutine angryLoopCoroutine = null;

    void Start()
    {
        // Nastav� psa do sed�c�ho stavu na za��tku
        dogAnimator.SetBool("isSitting", true);
    }

    void Update()
    {
        // Vypo��tejte vzd�lenost mezi hr��em a psem
        float distance = Vector3.Distance(player.position, transform.position);

        // Zkontroluje, zda je hr�� bl�zko na to, aby pes za�al b�t na�tvan�
        if (distance <= angryDistance && !isAngryLoopActive)
        {
            isAngryLoopActive = true;
            dogAnimator.SetBool("isSitting", false); // Zru�� sed�c� stav
            angryLoopCoroutine = StartCoroutine(AngryTailLoop());
        }
        else if (distance > angryDistance && isAngryLoopActive)
        {
            isAngryLoopActive = false;
            if (angryLoopCoroutine != null)
            {
                StopCoroutine(angryLoopCoroutine);
                angryLoopCoroutine = null;
            }

            // Nastav� psa zp�t do sed�c�ho stavu po m�v�n� ocasem
            dogAnimator.SetTrigger("isWigglingTail"); // Zavrt� ocasem jednou
            StartCoroutine(WaitAndSit()); // Po zavrt�n� se posad�
        }

        // Pes se ot��� za hr��em, pokud je hr�� ve vzd�lenosti 15 jednotek nebo m�n�
        if (distance <= lookDistance)
        {
            RotateTowardsPlayer();
        }
    }

    IEnumerator AngryTailLoop()
    {
        while (isAngryLoopActive)
        {
            // Spust� animaci na�tv�n�
            dogAnimator.SetTrigger("isAngry");
            yield return new WaitForSeconds(0.5f); // Kr�tk� pauza mezi p�echody

            // Spust� animaci m�v�n� ocasem
            dogAnimator.SetTrigger("isWigglingTail");
            yield return new WaitForSeconds(0.5f); // Kr�tk� pauza mezi p�echody
        }
    }

    IEnumerator WaitAndSit()
    {
        // Po�k� na dokon�en� m�v�n� ocasem, ne� se posad�
        yield return new WaitForSeconds(1f);
        dogAnimator.SetBool("isSitting", true); // Pes se posad�
    }

    void RotateTowardsPlayer()
    {
        // Sm�r od psa k hr��i
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Zamez� ot��en� ve vertik�ln�m sm�ru

        // C�lov� rotace sm�rem k hr��i
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Hladk� p�echod rotace k c�lov� rotaci
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}

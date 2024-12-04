using UnityEngine;
using System.Collections;

public class DogBehavior : MonoBehaviour
{
    public Animator dogAnimator;       // Animator psa
    public Transform player;           // Transform hráèe

    public float angryDistance = 5.0f;        // Nastavení vzdálenosti, pøi které se pes stane naštvaným
    public float lookDistance = 15.0f;        // Nastavení vzdálenosti, pøi které se pes otáèí za hráèem
    public float rotationSpeed = 2.0f; // Rychlost otáèení psa smìrem k hráèi

    private bool isAngryLoopActive = false;   // Kontroluje, zda je smyèka aktivní
    private Coroutine angryLoopCoroutine = null;

    void Start()
    {
        // Nastaví psa do sedícího stavu na zaèátku
        dogAnimator.SetBool("isSitting", true);
    }

    void Update()
    {
        // Vypoèítejte vzdálenost mezi hráèem a psem
        float distance = Vector3.Distance(player.position, transform.position);

        // Zkontroluje, zda je hráè blízko na to, aby pes zaèal být naštvaný
        if (distance <= angryDistance && !isAngryLoopActive)
        {
            isAngryLoopActive = true;
            dogAnimator.SetBool("isSitting", false); // Zruší sedící stav
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

            // Nastaví psa zpìt do sedícího stavu po mávání ocasem
            dogAnimator.SetTrigger("isWigglingTail"); // Zavrtí ocasem jednou
            StartCoroutine(WaitAndSit()); // Po zavrtìní se posadí
        }

        // Pes se otáèí za hráèem, pokud je hráè ve vzdálenosti 15 jednotek nebo ménì
        if (distance <= lookDistance)
        {
            RotateTowardsPlayer();
        }
    }

    IEnumerator AngryTailLoop()
    {
        while (isAngryLoopActive)
        {
            // Spustí animaci naštvání
            dogAnimator.SetTrigger("isAngry");
            yield return new WaitForSeconds(0.5f); // Krátká pauza mezi pøechody

            // Spustí animaci mávání ocasem
            dogAnimator.SetTrigger("isWigglingTail");
            yield return new WaitForSeconds(0.5f); // Krátká pauza mezi pøechody
        }
    }

    IEnumerator WaitAndSit()
    {
        // Poèká na dokonèení mávání ocasem, než se posadí
        yield return new WaitForSeconds(1f);
        dogAnimator.SetBool("isSitting", true); // Pes se posadí
    }

    void RotateTowardsPlayer()
    {
        // Smìr od psa k hráèi
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Zamezí otáèení ve vertikálním smìru

        // Cílová rotace smìrem k hráèi
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Hladký pøechod rotace k cílové rotaci
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}

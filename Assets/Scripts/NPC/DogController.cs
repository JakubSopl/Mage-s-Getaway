using UnityEngine;
using System.Collections;

public class DogBehavior : MonoBehaviour
{
    public Animator dogAnimator;       // Animator psa
    public Transform player;           // Transform hráèe

    public float angryDistance;        // Nastavení vzdálenosti, pøi které se pes stane naštvaným

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

        if (distance <= angryDistance && !isAngryLoopActive)
        {
            // Pokud je hráè blízko a smyèka není aktivní, zaène smyèku naštvání/mávání ocasem
            isAngryLoopActive = true;
            dogAnimator.SetBool("isSitting", false); // Zruší sedící stav
            angryLoopCoroutine = StartCoroutine(AngryTailLoop());
        }
        else if (distance > angryDistance && isAngryLoopActive)
        {
            // Pokud je hráè daleko a smyèka je aktivní, zastaví smyèku
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
}

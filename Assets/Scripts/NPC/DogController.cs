using UnityEngine;
using System.Collections;

public class DogBehavior : MonoBehaviour
{
    public Animator dogAnimator;       // Animator psa
    public Transform player;           // Transform hr��e

    public float angryDistance;        // Nastaven� vzd�lenosti, p�i kter� se pes stane na�tvan�m

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

        if (distance <= angryDistance && !isAngryLoopActive)
        {
            // Pokud je hr�� bl�zko a smy�ka nen� aktivn�, za�ne smy�ku na�tv�n�/m�v�n� ocasem
            isAngryLoopActive = true;
            dogAnimator.SetBool("isSitting", false); // Zru�� sed�c� stav
            angryLoopCoroutine = StartCoroutine(AngryTailLoop());
        }
        else if (distance > angryDistance && isAngryLoopActive)
        {
            // Pokud je hr�� daleko a smy�ka je aktivn�, zastav� smy�ku
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
}

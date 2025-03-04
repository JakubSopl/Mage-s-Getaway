using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndPortalTrigger : MonoBehaviour
{
    public Image fadeImage; // Pøipojit Image komponentu
    public float fadeDuration = 5f; // Doba trvání efektu
    public GameObject wonMenu; // Pøipojit GameObject vítìzného menu

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EndPortal"))
        {
            StartCoroutine(FadeToBlackAndShowMenu());
        }
    }

    private IEnumerator FadeToBlackAndShowMenu()
    {
        float elapsedTime = 0f;
        Color startColor = fadeImage.color;
        startColor.a = 0f; // Zajištìní, že zaèínáme z plné prùhlednosti
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f); // Plná neprùhlednost

        fadeImage.color = startColor; // Nastavení výchozí barvy

        // Fade-out efekt rovnomìrnì po celou dobu
        while (elapsedTime < fadeDuration)
        {
            fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.SmoothStep(0f, 1f, elapsedTime / fadeDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ujistit se, že barva je plnì èerná
        fadeImage.color = endColor;

        // Zobrazit vítìzné menu
        wonMenu.SetActive(true);
    }
}

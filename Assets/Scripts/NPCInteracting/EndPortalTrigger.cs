using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndPortalTrigger : MonoBehaviour
{
    public Image fadeImage; // P�ipojit Image komponentu
    public float fadeDuration = 5f; // Doba trv�n� efektu
    public GameObject wonMenu; // P�ipojit GameObject v�t�zn�ho menu

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
        startColor.a = 0f; // Zaji�t�n�, �e za��n�me z pln� pr�hlednosti
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f); // Pln� nepr�hlednost

        fadeImage.color = startColor; // Nastaven� v�choz� barvy

        // Fade-out efekt rovnom�rn� po celou dobu
        while (elapsedTime < fadeDuration)
        {
            fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.SmoothStep(0f, 1f, elapsedTime / fadeDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ujistit se, �e barva je pln� �ern�
        fadeImage.color = endColor;

        // Zobrazit v�t�zn� menu
        wonMenu.SetActive(true);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class EndPortalTrigger : MonoBehaviour
{
    public Image fadeImage; // P�ipojit Image komponentu
    public float fadeDuration = 1.5f; // Doba trv�n� efektu

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EndPortal"))
        {
            StartCoroutine(FadeToBlackAndLoadMenu());
        }
    }

    private IEnumerator FadeToBlackAndLoadMenu()
    {
        float elapsedTime = 0f;
        Color startColor = fadeImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f); // Pln� nepr�hlednost

        // Fade-out efekt
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeImage.color = Color.Lerp(startColor, endColor, elapsedTime / fadeDuration);
            yield return null;
        }

        // Na�ten� sc�ny s menu
        SceneManager.LoadScene(0);
    }
}

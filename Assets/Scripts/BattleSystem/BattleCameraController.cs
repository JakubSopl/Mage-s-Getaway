using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BattleCameraController : MonoBehaviour
{
    public Camera thirdPersonCamera;
    public Camera battleCamera;
    public UnitController unitController;
    public Image fadeImage; // UI obrazovka pro fade efekt

    public float fadeInDuration = 0.75f;  // Del�� fade in (ztmaven�)
    public float fadeOutDuration = 1.0f;  // Pomal� fade out (odtmaven�)

    public void EnterBattleMode(Transform battleCameraPosition)
    {
        StartCoroutine(FadeToBattle(battleCameraPosition));
    }

    private IEnumerator FadeToBattle(Transform battleCameraPosition)
    {
        Debug.Log("Starting longer fade to battle mode...");

        // Fade in (ztmaven� - 0.75s) za�ne d��ve
        yield return StartCoroutine(FadeScreen(1, fadeInDuration * 0.75f));

        // P�epnut� do battle m�du
        unitController.EnterBattleMode();

        if (battleCamera == null || thirdPersonCamera == null)
        {
            Debug.LogError("BattleCamera or ThirdPersonCamera is not assigned!");
            yield break;
        }

        battleCamera.transform.position = battleCameraPosition.position;
        battleCamera.transform.rotation = battleCameraPosition.rotation;

        thirdPersonCamera.gameObject.SetActive(false);
        battleCamera.gameObject.SetActive(true);

        Debug.Log("Camera switched to battle mode.");

        // Pomal� fade out (odtmaven� - 1s)
        yield return StartCoroutine(FadeScreen(0, fadeOutDuration));
    }

    public void ExitBattleMode()
    {
        StartCoroutine(FadeFromBattle());
    }

    private IEnumerator FadeFromBattle()
    {
        Debug.Log("Starting fade from battle mode...");

        // Fade in (ztmaven� - 0.75s) za�ne d��ve
        yield return StartCoroutine(FadeScreen(1, fadeInDuration * 0.75f));

        // P�epnut� zp�t na t�et� osobu
        unitController.ExitBattleMode();

        battleCamera.gameObject.SetActive(false);
        thirdPersonCamera.gameObject.SetActive(true);

        Debug.Log("Camera switched back to third person mode.");

        // Pomal� fade out (odtmaven� - 1s)
        yield return StartCoroutine(FadeScreen(0, fadeOutDuration));
    }

    private IEnumerator FadeScreen(float targetAlpha, float duration)
    {
        float startAlpha = fadeImage.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, newAlpha);
            yield return null;
        }

        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, targetAlpha);
    }
}
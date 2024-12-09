using System.Collections;
using TMPro;
using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    [SerializeField] private GameObject chatBubblePrefab; // Prefab bubliny
    [SerializeField] private string npcText; // Text pro bublinu
    private GameObject activeChatBubble; // Aktivn� instance bubliny
    [SerializeField] private Transform cameraTransform; // Kamera hr��e
    [SerializeField] private float typingSpeed; // Rychlost psan� textu
    [SerializeField] private float minFontSize; // Minim�ln� velikost textu
    [SerializeField] private float maxFontSize; // Maxim�ln� velikost textu

    private void Update()
    {
        // Check visibility logic if the bubble exists
        if (activeChatBubble != null)
        {
            Canvas bubbleCanvas = activeChatBubble.GetComponentInChildren<Canvas>();
            if (bubbleCanvas != null)
            {
                // Direction from the bubble to the camera
                Vector3 toCamera = (cameraTransform.position - activeChatBubble.transform.position).normalized;

                // Dot product determines if the camera is in front or behind
                float dotProduct = Vector3.Dot(activeChatBubble.transform.forward, toCamera);

                // Enable or disable the canvas
                bubbleCanvas.enabled = dotProduct > 0; // Enable when in front, disable when behind
            }
        }
    }

    public void Interact()
    {
        // Pokud ji� existuje bublina, zni� ji
        if (activeChatBubble != null)
        {
            Destroy(activeChatBubble);
        }

        Vector3 spawnPosition = transform.position + Vector3.up * 3 + Vector3.back * 0.5f;
        activeChatBubble = Instantiate(chatBubblePrefab, spawnPosition, Quaternion.identity);

        // Najdi TextMeshProUGUI v prefab bubliny
        TMPro.TextMeshProUGUI textComponent = activeChatBubble.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textComponent != null)
        {
            // Nastav automatick� p�izp�soben� textu
            textComponent.enableAutoSizing = true;
            textComponent.fontSizeMin = minFontSize;
            textComponent.fontSizeMax = maxFontSize;

            StartCoroutine(TypeText(textComponent, npcText));
        }
        else
        {
            Debug.LogError("TextMeshProUGUI nebyl nalezen v prefab bubliny!");
        }

        // Nastav bublinu jako d�t� NPC, aby se pohybovala s n�m
        activeChatBubble.transform.SetParent(transform);
    }

    private IEnumerator TypeText(TMPro.TextMeshProUGUI textComponent, string text)
    {
        textComponent.text = ""; // Vyma� existuj�c� text
        foreach (char letter in text)
        {
            textComponent.text += letter; // P�idej p�smeno
            yield return new WaitForSeconds(typingSpeed); // Po�kej p�ed p�id�n�m dal��ho
        }

        // P�izp�sob text podle velikosti bubliny
        textComponent.ForceMeshUpdate(); // Zajist� p�epo�et velikosti textu
    }
}

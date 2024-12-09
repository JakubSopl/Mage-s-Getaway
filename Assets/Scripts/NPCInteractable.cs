using System.Collections;
using TMPro;
using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    [SerializeField] private GameObject chatBubblePrefab; // Prefab bubliny
    [SerializeField] private string npcText; // Text pro bublinu
    private GameObject activeChatBubble; // Aktivní instance bubliny
    [SerializeField] private Transform cameraTransform; // Kamera hráèe
    [SerializeField] private float typingSpeed; // Rychlost psaní textu
    [SerializeField] private float minFontSize; // Minimální velikost textu
    [SerializeField] private float maxFontSize; // Maximální velikost textu

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
        // Pokud již existuje bublina, zniè ji
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
            // Nastav automatické pøizpùsobení textu
            textComponent.enableAutoSizing = true;
            textComponent.fontSizeMin = minFontSize;
            textComponent.fontSizeMax = maxFontSize;

            StartCoroutine(TypeText(textComponent, npcText));
        }
        else
        {
            Debug.LogError("TextMeshProUGUI nebyl nalezen v prefab bubliny!");
        }

        // Nastav bublinu jako dítì NPC, aby se pohybovala s ním
        activeChatBubble.transform.SetParent(transform);
    }

    private IEnumerator TypeText(TMPro.TextMeshProUGUI textComponent, string text)
    {
        textComponent.text = ""; // Vymaž existující text
        foreach (char letter in text)
        {
            textComponent.text += letter; // Pøidej písmeno
            yield return new WaitForSeconds(typingSpeed); // Poèkej pøed pøidáním dalšího
        }

        // Pøizpùsob text podle velikosti bubliny
        textComponent.ForceMeshUpdate(); // Zajistí pøepoèet velikosti textu
    }
}

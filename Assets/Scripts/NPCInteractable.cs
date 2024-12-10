using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class NPCInteractable : MonoBehaviour
{
    [SerializeField] private GameObject chatBubblePrefab; // Prefab bubliny
    [SerializeField] private List<string> npcTexts; // První list textù
    [SerializeField] private List<string> secondaryNpcTexts; // Druhý list textù
    [SerializeField] private Animator npcAnimator; // Animator pro NPC
    private GameObject activeChatBubble; // Aktivní instance bubliny
    [SerializeField] private Transform cameraTransform; // Kamera hráèe
    [SerializeField] private Transform playerTransform; // Transform hráèe
    [SerializeField] private float typingSpeed; // Rychlost psaní textu
    [SerializeField] private float minFontSize; // Minimální velikost textu
    [SerializeField] private float maxFontSize; // Maximální velikost textu
    private int currentTextIndex = 0; // Aktuální index textu v prvním listu
    private int currentSecondaryTextIndex = 0; // Aktuální index textu ve druhém listu
    private bool isUsingSecondaryTexts = false; // Flag, který urèuje, zda se používá druhý list textù
    private bool isTalking = false; // Lokální stav mluvení

    private void Update()
    {
        if (activeChatBubble != null)
        {
            // Udržuj bublinu orientovanou na kameru hráèe
            activeChatBubble.transform.LookAt(cameraTransform);
            activeChatBubble.transform.rotation = Quaternion.Euler(0, activeChatBubble.transform.rotation.eulerAngles.y, 0);
        }
    }

    public void Interact()
    {
        if (!isTalking)
        {
            StartTalking();
        }

        if (activeChatBubble != null)
        {
            Destroy(activeChatBubble);
        }

        // Otoè NPC smìrem k hráèi
        FacePlayer();

        if (isUsingSecondaryTexts)
        {
            if (currentSecondaryTextIndex < secondaryNpcTexts.Count)
            {
                ShowBubble(secondaryNpcTexts[currentSecondaryTextIndex]);
                currentSecondaryTextIndex++;
            }
            else
            {
                EndConversation();
            }
        }
        else
        {
            if (currentTextIndex < npcTexts.Count)
            {
                ShowBubble(npcTexts[currentTextIndex]);
                currentTextIndex++;
            }
            else
            {
                EndConversation();
                isUsingSecondaryTexts = true; // Switch to the second list for the next interaction
            }
        }
    }

    private void StartTalking()
    {
        isTalking = true;
        if (npcAnimator != null)
        {
            npcAnimator.SetBool("isTalking", true);
        }
    }

    private void ShowBubble(string text)
    {
        if (chatBubblePrefab == null)
            return;

        // Spawn bubliny nad NPC
        Vector3 spawnPosition = transform.position + Vector3.up * 3;
        activeChatBubble = Instantiate(chatBubblePrefab, spawnPosition, Quaternion.identity);

        TMPro.TextMeshProUGUI textComponent = activeChatBubble.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.enableAutoSizing = true;
            textComponent.fontSizeMin = minFontSize;
            textComponent.fontSizeMax = maxFontSize;

            StartCoroutine(TypeText(textComponent, text));
        }

        // Nastav bublinu jako dítì NPC
        activeChatBubble.transform.SetParent(transform);
    }

    private IEnumerator TypeText(TMPro.TextMeshProUGUI textComponent, string text)
    {
        if (textComponent == null)
            yield break;

        textComponent.text = "";
        foreach (char letter in text)
        {
            if (textComponent == null)
                yield break; // Pokud je textComponent znièen bìhem psaní, ukonèíme korutinu

            textComponent.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        if (textComponent != null)
        {
            textComponent.ForceMeshUpdate();
        }
    }

    private void FacePlayer()
    {
        if (playerTransform == null)
            return;

        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0; // Zabráníme sklápìní nebo naklánìní
        transform.rotation = Quaternion.LookRotation(directionToPlayer);
    }

    private void EndConversation()
    {
        if (activeChatBubble != null)
        {
            Destroy(activeChatBubble);
        }

        isTalking = false;
        if (npcAnimator != null)
        {
            npcAnimator.SetBool("isTalking", false);
        }

        if (!isUsingSecondaryTexts)
        {
            // Reset the first list index, ready for a new conversation cycle if needed
            currentTextIndex = 0;
        }
        else
        {
            // Reset the second list index
            currentSecondaryTextIndex = 0;
        }
    }
}

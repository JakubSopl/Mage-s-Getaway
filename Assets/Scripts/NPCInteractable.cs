using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class NPCInteractable : MonoBehaviour
{
    [SerializeField] private GameObject chatBubblePrefab; // Prefab bubliny
    [SerializeField] private List<string> npcTexts; // PrvnÌ list text˘
    [SerializeField] private List<string> secondaryNpcTexts; // Druh˝ list text˘
    [SerializeField] private Animator npcAnimator; // Animator pro NPC
    private GameObject activeChatBubble; // AktivnÌ instance bubliny
    [SerializeField] private Transform cameraTransform; // Kamera hr·Ëe
    [SerializeField] private Transform playerTransform; // Transform hr·Ëe
    [SerializeField] private float typingSpeed; // Rychlost psanÌ textu
    [SerializeField] private float minFontSize; // Minim·lnÌ velikost textu
    [SerializeField] private float maxFontSize; // Maxim·lnÌ velikost textu
    [SerializeField] private bool doNotRotateToPlayer; // NastavenÌ, zda se NPC otoËÌ k hr·Ëi
    private int currentTextIndex = 0; // Aktu·lnÌ index textu v prvnÌm listu
    private int currentSecondaryTextIndex = 0; // Aktu·lnÌ index textu ve druhÈm listu
    private bool isUsingSecondaryTexts = false; // Flag, kter˝ urËuje, zda se pouûÌv· druh˝ list text˘
    private bool isTalking = false; // Lok·lnÌ stav mluvenÌ

    private void Update()
    {
        if (activeChatBubble != null)
        {
            // Udrûuj bublinu orientovanou na kameru hr·Ëe
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

        // OtoË NPC smÏrem k hr·Ëi, pokud je to povoleno
        if (!doNotRotateToPlayer)
        {
            FacePlayer();
        }

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

        // Nastav bublinu jako dÌtÏ NPC
        activeChatBubble.transform.SetParent(transform);

        // Spusù korutinu na odstranÏnÌ bubliny po 5 sekund·ch
        StartCoroutine(HideBubbleAfterDelay(10f));
    }

    private IEnumerator TypeText(TMPro.TextMeshProUGUI textComponent, string text)
    {
        if (textComponent == null)
            yield break;

        textComponent.text = "";
        foreach (char letter in text)
        {
            if (textComponent == null)
                yield break; // Pokud je textComponent zniËen bÏhem psanÌ, ukonËÌme korutinu

            textComponent.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        if (textComponent != null)
        {
            textComponent.ForceMeshUpdate();
        }
    }

    private IEnumerator HideBubbleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (activeChatBubble != null)
        {
            Destroy(activeChatBubble);
            activeChatBubble = null;
        }

        EndConversation(); // P¯epni NPC zpÏt do stavu idle
    }

    private void FacePlayer()
    {
        if (playerTransform == null)
            return;

        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0; // Zabr·nÌme skl·pÏnÌ nebo nakl·nÏnÌ
        transform.rotation = Quaternion.LookRotation(directionToPlayer);
    }

    private void EndConversation()
    {
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

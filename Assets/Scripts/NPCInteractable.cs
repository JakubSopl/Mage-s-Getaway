using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class NPCInteractable : MonoBehaviour
{
    [SerializeField] private GameObject chatBubblePrefab; // Prefab bubliny
    [SerializeField] private List<string> npcTexts; // První list textù
    [SerializeField] private List<string> secondaryNpcTexts; // Druhı list textù
    [SerializeField] private Animator npcAnimator; // Animator pro NPC
    [SerializeField] private CloakReward cloakRewardScript; // Reference na skript pro odmìnu hráèe
    [SerializeField] private JumpBoostReward jumpBoostRewardScript; // Reference na skript pro zvıšení skoku
    [SerializeField] private bool grantsCloak; // Zda toto NPC dává pláš
    [SerializeField] private bool grantsJumpBoost; // Zda toto NPC dává zvıšenı skok
    private GameObject activeChatBubble; // Aktivní instance bubliny
    [SerializeField] private Transform cameraTransform; // Kamera hráèe
    [SerializeField] private Transform playerTransform; // Transform hráèe
    [SerializeField] private float typingSpeed; // Rychlost psaní textu
    [SerializeField] private float minFontSize; // Minimální velikost textu
    [SerializeField] private float maxFontSize; // Maximální velikost textu
    [SerializeField] private bool doNotRotateToPlayer; // Nastavení, zda se NPC otoèí k hráèi
    private int currentTextIndex = 0; // Aktuální index textu v prvním listu
    private int currentSecondaryTextIndex = 0; // Aktuální index textu ve druhém listu
    private bool isUsingSecondaryTexts = false; // Flag, kterı urèuje, zda se pouívá druhı list textù
    private bool isTalking = false; // Lokální stav mluvení

    private Coroutine hideBubbleCoroutine; // Reference na aktivní korutinu pro skrytí bubliny

    private void Update()
    {
        if (activeChatBubble != null)
        {
            // Udruj bublinu orientovanou na kameru hráèe
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

        // Otoè NPC smìrem k hráèi, pokud je to povoleno
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
                isUsingSecondaryTexts = true; // Switch to the second list for the next interaction
                Interact(); // Immediately continue with secondary texts
            }
        }

        // Restartuj èasovaè pro skrytí bubliny
        if (hideBubbleCoroutine != null)
        {
            StopCoroutine(hideBubbleCoroutine);
        }
        hideBubbleCoroutine = StartCoroutine(HideBubbleAfterDelay(10f));
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

    private IEnumerator HideBubbleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (activeChatBubble != null)
        {
            Destroy(activeChatBubble);
            activeChatBubble = null;
        }

        EndConversation(); // Pøepni NPC zpìt do stavu idle
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
        isTalking = false;
        if (npcAnimator != null)
        {
            npcAnimator.SetBool("isTalking", false);
        }

        bool allTextsCompleted = isUsingSecondaryTexts && currentSecondaryTextIndex >= secondaryNpcTexts.Count;
        bool primaryTextsCompleted = !isUsingSecondaryTexts && currentTextIndex >= npcTexts.Count;

        if (primaryTextsCompleted && !isUsingSecondaryTexts)
        {
            isUsingSecondaryTexts = true;
        }
        else if (allTextsCompleted)
        {
            currentTextIndex = 0;
            currentSecondaryTextIndex = 0;

            if (grantsCloak && cloakRewardScript != null)
            {
                cloakRewardScript.GrantCloak();
            }

            if (grantsJumpBoost && jumpBoostRewardScript != null)
            {
                jumpBoostRewardScript.GrantJumpBoost();
            }
        }
    }
}
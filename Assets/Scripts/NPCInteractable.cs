using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class NPCInteractable : MonoBehaviour
{
    [SerializeField] private GameObject chatBubblePrefab; // Prefab bubliny
    [SerializeField] private List<string> npcTexts; // Prvn� list text�
    [SerializeField] private List<string> secondaryNpcTexts; // Druh� list text�
    [SerializeField] private Animator npcAnimator; // Animator pro NPC
    [SerializeField] private CloakReward cloakRewardScript; // Reference na skript pro odm�nu hr��e
    [SerializeField] private JumpBoostReward jumpBoostRewardScript; // Reference na skript pro zv��en� skoku
    [SerializeField] private bool grantsCloak; // Zda toto NPC d�v� pl᚝
    [SerializeField] private bool grantsJumpBoost; // Zda toto NPC d�v� zv��en� skok
    private GameObject activeChatBubble; // Aktivn� instance bubliny
    [SerializeField] private Transform cameraTransform; // Kamera hr��e
    [SerializeField] private Transform playerTransform; // Transform hr��e
    [SerializeField] private float typingSpeed; // Rychlost psan� textu
    [SerializeField] private float minFontSize; // Minim�ln� velikost textu
    [SerializeField] private float maxFontSize; // Maxim�ln� velikost textu
    [SerializeField] private bool doNotRotateToPlayer; // Nastaven�, zda se NPC oto�� k hr��i
    private int currentTextIndex = 0; // Aktu�ln� index textu v prvn�m listu
    private int currentSecondaryTextIndex = 0; // Aktu�ln� index textu ve druh�m listu
    private bool isUsingSecondaryTexts = false; // Flag, kter� ur�uje, zda se pou��v� druh� list text�
    private bool isTalking = false; // Lok�ln� stav mluven�

    private Coroutine hideBubbleCoroutine; // Reference na aktivn� korutinu pro skryt� bubliny

    private void Update()
    {
        if (activeChatBubble != null)
        {
            // Udr�uj bublinu orientovanou na kameru hr��e
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

        // Oto� NPC sm�rem k hr��i, pokud je to povoleno
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

        // Restartuj �asova� pro skryt� bubliny
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

        // Nastav bublinu jako d�t� NPC
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
                yield break; // Pokud je textComponent zni�en b�hem psan�, ukon��me korutinu

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

        EndConversation(); // P�epni NPC zp�t do stavu idle
    }

    private void FacePlayer()
    {
        if (playerTransform == null)
            return;

        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0; // Zabr�n�me skl�p�n� nebo nakl�n�n�
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
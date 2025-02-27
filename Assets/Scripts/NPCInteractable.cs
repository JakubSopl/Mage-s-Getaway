using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class NPCInteractable : MonoBehaviour
{
    [SerializeField] private GameObject chatBubblePrefab;
    [SerializeField] private List<string> npcTexts;
    [SerializeField] private List<string> secondaryNpcTexts;
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private CloakReward cloakRewardScript;
    [SerializeField] private JumpBoostReward jumpBoostRewardScript;
    [SerializeField] private bool grantsCloak;
    [SerializeField] private bool grantsJumpBoost;
    [SerializeField] private bool grantsPortal;
    [SerializeField] private GameObject portalObject;
    [SerializeField] private AudioSource npcAudioSource;
    [SerializeField] private AudioClip talkingClip;

    private GameObject activeChatBubble;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float typingSpeed;
    [SerializeField] private float minFontSize;
    [SerializeField] private float maxFontSize;
    [SerializeField] private bool doNotRotateToPlayer;

    private int currentTextIndex = 0;
    private int currentSecondaryTextIndex = 0;
    private bool isUsingSecondaryTexts = false;
    private bool isTalking = false;
    private Coroutine hideBubbleCoroutine;

    private void Update()
    {
        if (activeChatBubble != null)
        {
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
                isUsingSecondaryTexts = true;
                Interact();
            }
        }

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
        if (npcAudioSource != null && talkingClip != null)
        {
            npcAudioSource.clip = talkingClip;
            npcAudioSource.loop = true;
            npcAudioSource.volume = 0.1f; // Nastavení hlasitosti na polovinu
            npcAudioSource.Play();
        }
    }

    private void ShowBubble(string text)
    {
        if (chatBubblePrefab == null)
            return;

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
                yield break;

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

        EndConversation();
    }

    private void FacePlayer()
    {
        if (playerTransform == null)
            return;

        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0;
        transform.rotation = Quaternion.LookRotation(directionToPlayer);
    }

    private void EndConversation()
    {
        isTalking = false;
        if (npcAnimator != null)
        {
            npcAnimator.SetBool("isTalking", false);
        }
        if (npcAudioSource != null && npcAudioSource.isPlaying)
        {
            npcAudioSource.Stop();
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

            if (grantsPortal && portalObject != null)
            {
                portalObject.SetActive(true);
            }
        }
    }
}

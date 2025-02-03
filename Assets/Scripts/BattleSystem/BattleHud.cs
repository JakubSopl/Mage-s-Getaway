using System.Collections;
using UnityEngine;
using TMPro;

public class BattleHud : MonoBehaviour
{
    public TMP_Text battleText;
    private Coroutine currentCoroutine; // Ulo�� aktu�ln� b��c� zpr�vu
    private string defaultMessage = "Choose action!"; // V�choz� zpr�va
    private bool isPlayerTurn = true; // Sledov�n� tahu hr��e

    public void SetTurn(bool playerTurn)
    {
        isPlayerTurn = playerTurn;
    }

    public void ChooseText()
    {
        if (isPlayerTurn)
        {
            SetPermanentMessage(defaultMessage); // "Choose action!" z�stane viditeln� pouze pokud je hr�� na tahu
        }
    }

    public void UsedText(string unitName, string actionName)
    {
        ShowTemporaryMessage($"{unitName} uses {actionName}!", 2.5f, true);
    }

    public void DamageText(string unitName, int damage)
    {
        ShowTemporaryMessage($"{unitName} took {damage} damage!", 2.5f, true);
    }

    public void HealText(string unitName, int heal)
    {
        ShowTemporaryMessage($"{unitName} healed {heal} HP!", 3.0f, true);
    }

    public void ManaText(int mana)
    {
        ShowTemporaryMessage($"You need {mana} mana for spell!", 2.5f, true);
    }

    public void RestoreManaText(string unitName, int mana)
    {
        ShowTemporaryMessage($"{unitName} restored {mana} mana!", 2.5f, true);
    }

    public void FullManaText(string unitName)
    {
        ShowTemporaryMessage($"{unitName} already has full mana!", 2.5f, true);
    }

    public void EnemyActionText(string unitName, string actionName)
    {
        ShowTemporaryMessage($"{unitName} uses {actionName}!", 2.5f, false);
    }

    public void EndText(bool won)
    {
        SetPermanentMessage(won ? "You WON!\nPress Restart to start again!" : "You LOST!\nPress Restart to start again!");
    }

    // Metoda pro nastaven� trval� zpr�vy (nap�. "Choose action!")
    private void SetPermanentMessage(string message)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        battleText.text = message;
    }

    // Metoda pro zobrazen� zpr�vy na ur�itou dobu, pot� se vr�t� zp�t na "Choose action!" pouze pokud je hr�� na tahu
    private void ShowTemporaryMessage(string message, float duration, bool isPlayerAction)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(DisplayTemporaryMessage(message, duration, isPlayerAction));
    }

    private IEnumerator DisplayTemporaryMessage(string message, float duration, bool isPlayerAction)
    {
        battleText.text = message;
        yield return new WaitForSeconds(duration);

        if (isPlayerAction)
        {
            yield return new WaitForSeconds(2.0f); // Po�k�, ne� se zobraz� nep��telova akce
        }

        if (isPlayerTurn)
        {
            battleText.text = defaultMessage; // Po uplynut� �asu se vr�t� na "Choose action!" jen pokud je hr�� na tahu
        }
        else
        {
            battleText.text = "Enemy's turn..."; // Zajist�, �e se zobraz� spr�vn� zpr�va pokud je nep��tel na tahu
        }
    }
}

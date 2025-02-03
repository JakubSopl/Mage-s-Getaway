using System.Collections;
using UnityEngine;
using TMPro;

public class BattleHud : MonoBehaviour
{
    public TMP_Text battleText;
    private Coroutine currentCoroutine; // Uloží aktuálnì bìžící zprávu
    private string defaultMessage = "Choose action!"; // Výchozí zpráva
    private bool isPlayerTurn = true; // Sledování tahu hráèe

    public void SetTurn(bool playerTurn)
    {
        isPlayerTurn = playerTurn;
    }

    public void ChooseText()
    {
        if (isPlayerTurn)
        {
            SetPermanentMessage(defaultMessage); // "Choose action!" zùstane viditelné pouze pokud je hráè na tahu
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

    // Metoda pro nastavení trvalé zprávy (napø. "Choose action!")
    private void SetPermanentMessage(string message)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        battleText.text = message;
    }

    // Metoda pro zobrazení zprávy na urèitou dobu, poté se vrátí zpìt na "Choose action!" pouze pokud je hráè na tahu
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
            yield return new WaitForSeconds(2.0f); // Poèká, než se zobrazí nepøítelova akce
        }

        if (isPlayerTurn)
        {
            battleText.text = defaultMessage; // Po uplynutí èasu se vrátí na "Choose action!" jen pokud je hráè na tahu
        }
        else
        {
            battleText.text = "Enemy's turn..."; // Zajistí, že se zobrazí správná zpráva pokud je nepøítel na tahu
        }
    }
}

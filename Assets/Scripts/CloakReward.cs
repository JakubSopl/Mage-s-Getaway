using UnityEngine;

public class CloakReward : MonoBehaviour
{
    [SerializeField] private GameObject playerCloak; // Pl᚝, kter� se aktivuje
    [SerializeField] private Movement playerMovementScript; // Reference na skript pohybu hr��e
    [SerializeField] private float speedBoost = 2f; // Zv��en� rychlosti hr��e

    private bool hasReceivedCloak = false; // Zda ji� hr�� obdr�el pl᚝

    // Metoda, kterou zavol�me po skon�en� konverzace
    public void GrantCloak()
    {
        if (hasReceivedCloak)
        {
            return; // Pokud ji� hr�� z�skal pl᚝, nic neprov�d�j
        }

        // Aktivuj pl᚝
        if (playerCloak != null)
        {
            playerCloak.SetActive(true);
        }

        // P�idej rychlost
        if (playerMovementScript != null)
        {
            playerMovementScript.playerSpeed += speedBoost;
            playerMovementScript.runSpeed += speedBoost;
        }

        hasReceivedCloak = true; // Nastav, �e hr�� ji� pl᚝ obdr�el
    }
}

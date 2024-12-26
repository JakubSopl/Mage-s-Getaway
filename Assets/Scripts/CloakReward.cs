using UnityEngine;

public class CloakReward : MonoBehaviour
{
    [SerializeField] private GameObject playerCloak; // Pláš, kterı se aktivuje
    [SerializeField] private Movement playerMovementScript; // Reference na skript pohybu hráèe
    [SerializeField] private float speedBoost = 2f; // Zvıšení rychlosti hráèe

    private bool hasReceivedCloak = false; // Zda ji hráè obdrel pláš

    // Metoda, kterou zavoláme po skonèení konverzace
    public void GrantCloak()
    {
        if (hasReceivedCloak)
        {
            return; // Pokud ji hráè získal pláš, nic neprovádìj
        }

        // Aktivuj pláš
        if (playerCloak != null)
        {
            playerCloak.SetActive(true);
        }

        // Pøidej rychlost
        if (playerMovementScript != null)
        {
            playerMovementScript.playerSpeed += speedBoost;
            playerMovementScript.runSpeed += speedBoost;
        }

        hasReceivedCloak = true; // Nastav, e hráè ji pláš obdrel
    }
}

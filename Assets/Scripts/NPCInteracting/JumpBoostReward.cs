using UnityEngine;

public class JumpBoostReward : MonoBehaviour
{
    [SerializeField] private GameObject playerBoots; // Boty, které se aktivují
    [SerializeField] private Movement playerMovementScript; // Reference na skript pohybu hráèe
    [SerializeField] private float jumpBoost = 1.5f; // Zvýšení skoku hráèe

    private bool hasReceivedJumpBoost = false; // Zda již hráè obdržel skokový bonus

    // Metoda, kterou zavoláme po skonèení konverzace
    public void GrantJumpBoost()
    {
        if (hasReceivedJumpBoost)
        {
            return; // Pokud již hráè získal bonus, nic neprovádìj
        }

        // Aktivuj boty
        if (playerBoots != null)
        {
            playerBoots.SetActive(true);
        }

        // Pøidej bonus ke skoku
        if (playerMovementScript != null)
        {
            playerMovementScript.jumpSpeed += jumpBoost;
        }

        hasReceivedJumpBoost = true; // Nastav, že hráè již skokový bonus obdržel
    }
}

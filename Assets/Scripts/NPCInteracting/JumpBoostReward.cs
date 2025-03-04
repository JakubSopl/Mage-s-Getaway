using UnityEngine;

public class JumpBoostReward : MonoBehaviour
{
    [SerializeField] private GameObject playerBoots; // Boty, kter� se aktivuj�
    [SerializeField] private Movement playerMovementScript; // Reference na skript pohybu hr��e
    [SerializeField] private float jumpBoost = 1.5f; // Zv��en� skoku hr��e

    private bool hasReceivedJumpBoost = false; // Zda ji� hr�� obdr�el skokov� bonus

    // Metoda, kterou zavol�me po skon�en� konverzace
    public void GrantJumpBoost()
    {
        if (hasReceivedJumpBoost)
        {
            return; // Pokud ji� hr�� z�skal bonus, nic neprov�d�j
        }

        // Aktivuj boty
        if (playerBoots != null)
        {
            playerBoots.SetActive(true);
        }

        // P�idej bonus ke skoku
        if (playerMovementScript != null)
        {
            playerMovementScript.jumpSpeed += jumpBoost;
        }

        hasReceivedJumpBoost = true; // Nastav, �e hr�� ji� skokov� bonus obdr�el
    }
}

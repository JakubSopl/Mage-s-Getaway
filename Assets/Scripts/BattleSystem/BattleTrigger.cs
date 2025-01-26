using UnityEngine;

public class BattleTrigger : MonoBehaviour
{
    public GameObject enemyPrefab; // Prefab nep��tele, kter� m� b�t spawnov�n
    public Transform playerBattlePosition; // Pozice hr��e b�hem boje
    public Transform enemyBattlePosition; // Pozice nep��tele b�hem boje
    public Transform battleCameraPosition; // Pozice kamery b�hem boje
    public BattleController battleController; // Odkaz na BattleController

    private bool battleStarted = false; // Zajist�, �e boj se nespust� v�cekr�t

    private void OnTriggerEnter(Collider other)
    {
        if (battleStarted) return; // Zabr�n� spu�t�n� bitvy v�cekr�t

        if (other.CompareTag("Player")) // Zkontroluje, zda vstoupil hr��
        {
            Debug.Log("Player entered battle trigger. Starting battle...");

            battleStarted = true; // Ozna��, �e bitva byla zah�jena

            StartBattle(other.gameObject); // Spust� bitvu
        }
    }

    private void StartBattle(GameObject player)
    {
        // Spawnuje nep��tele na bojov� pozici
        GameObject enemy = Instantiate(enemyPrefab, enemyBattlePosition.position, Quaternion.identity);

        // Spust� setup bitvy v BattleController
        if (battleController != null)
        {
            battleController.SetupBattle(player, enemy, battleCameraPosition, playerBattlePosition.position);
        }
        else
        {
            Debug.LogError("BattleController is not assigned to SpawnTrigger!");
        }
    }
}

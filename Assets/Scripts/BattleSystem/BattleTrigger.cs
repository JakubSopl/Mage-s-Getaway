using UnityEngine;

public class BattleTrigger : MonoBehaviour
{
    public GameObject enemyPrefab; // Prefab nepøítele, který má být spawnován
    public Transform playerBattlePosition; // Pozice hráèe bìhem boje
    public Transform enemyBattlePosition; // Pozice nepøítele bìhem boje
    public Transform battleCameraPosition; // Pozice kamery bìhem boje
    public BattleController battleController; // Odkaz na BattleController

    private bool battleStarted = false; // Zajistí, že boj se nespustí vícekrát

    private void OnTriggerEnter(Collider other)
    {
        if (battleStarted) return; // Zabrání spuštìní bitvy vícekrát

        if (other.CompareTag("Player")) // Zkontroluje, zda vstoupil hráè
        {
            Debug.Log("Player entered battle trigger. Starting battle...");

            battleStarted = true; // Oznaèí, že bitva byla zahájena

            StartBattle(other.gameObject); // Spustí bitvu
        }
    }

    private void StartBattle(GameObject player)
    {
        // Spawnuje nepøítele na bojové pozici
        GameObject enemy = Instantiate(enemyPrefab, enemyBattlePosition.position, Quaternion.identity);

        // Spustí setup bitvy v BattleController
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

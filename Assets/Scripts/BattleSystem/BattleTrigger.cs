using UnityEngine;

public class BattleTrigger : MonoBehaviour
{
    public GameObject enemyPrefab; // Prefab nepøítele, který má být spawnován
    public Transform playerBattlePosition; // Pozice hráèe bìhem boje
    public Transform enemyBattlePosition; // Pozice nepøítele bìhem boje
    public Transform battleCameraPosition; // Pozice kamery bìhem boje
    public Transform exitSpawnPoint; // Kam se hráè teleportuje po úniku
    public BattleController battleController; // Odkaz na BattleController

    private GameObject currentEnemy; // Uloží referenci na nepøítele
    private bool battleStarted = false; // Zajistí, že boj se nespustí vícekrát

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !battleStarted) // Hráè vstoupil a bitva není aktivní
        {
            Debug.Log("Player entered battle trigger. Starting battle...");
            StartBattle(other.gameObject);
        }
    }

    private void StartBattle(GameObject player)
    {
        battleStarted = true; // Oznaèí, že bitva byla zahájena

        // Pokud už existuje nepøítel (hráè utekl), použijeme ho
        if (currentEnemy == null)
        {
            currentEnemy = Instantiate(enemyPrefab, enemyBattlePosition.position, Quaternion.identity);
        }
        else
        {
            // Resetujeme jeho pozici a zdraví
            currentEnemy.transform.position = enemyBattlePosition.position;
            UnitController enemyController = currentEnemy.GetComponent<UnitController>();
            if (enemyController != null)
            {
                enemyController.currentHealth = enemyController.unitScriptableObject.health;
                enemyController.UpdateHud();
            }
        }

        // Spustí setup bitvy v BattleController a pøedá exitSpawnPoint
        if (battleController != null)
        {
            battleController.SetupBattle(player, currentEnemy, battleCameraPosition, playerBattlePosition.position, this);
        }
        else
        {
            Debug.LogError("BattleController is not assigned to BattleTrigger!");
        }
    }

    // Resetuje bitvu po útìku hráèe
    public void ResetBattle()
    {
        Debug.Log("Battle ended, allowing re-entry.");
        battleStarted = false;
    }

    // Vrátí bod, kam se hráè teleportuje po útìku
    public Transform GetExitSpawnPoint()
    {
        return exitSpawnPoint;
    }
}

using UnityEngine;

public class BattleTrigger : MonoBehaviour
{
    public GameObject enemyPrefab; // Prefab nep��tele, kter� m� b�t spawnov�n
    public Transform playerBattlePosition; // Pozice hr��e b�hem boje
    public Transform enemyBattlePosition; // Pozice nep��tele b�hem boje
    public Transform battleCameraPosition; // Pozice kamery b�hem boje
    public Transform exitSpawnPoint; // Kam se hr�� teleportuje po �niku
    public BattleController battleController; // Odkaz na BattleController

    private GameObject currentEnemy; // Ulo�� referenci na nep��tele
    private bool battleStarted = false; // Zajist�, �e boj se nespust� v�cekr�t

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !battleStarted) // Hr�� vstoupil a bitva nen� aktivn�
        {
            Debug.Log("Player entered battle trigger. Starting battle...");
            StartBattle(other.gameObject);
        }
    }

    private void StartBattle(GameObject player)
    {
        battleStarted = true; // Ozna��, �e bitva byla zah�jena

        // Pokud u� existuje nep��tel (hr�� utekl), pou�ijeme ho
        if (currentEnemy == null)
        {
            currentEnemy = Instantiate(enemyPrefab, enemyBattlePosition.position, Quaternion.identity);
        }
        else
        {
            // Resetujeme jeho pozici a zdrav�
            currentEnemy.transform.position = enemyBattlePosition.position;
            UnitController enemyController = currentEnemy.GetComponent<UnitController>();
            if (enemyController != null)
            {
                enemyController.currentHealth = enemyController.unitScriptableObject.health;
                enemyController.UpdateHud();
            }
        }

        // Spust� setup bitvy v BattleController a p�ed� exitSpawnPoint
        if (battleController != null)
        {
            battleController.SetupBattle(player, currentEnemy, battleCameraPosition, playerBattlePosition.position, this);
        }
        else
        {
            Debug.LogError("BattleController is not assigned to BattleTrigger!");
        }
    }

    // Resetuje bitvu po �t�ku hr��e
    public void ResetBattle()
    {
        Debug.Log("Battle ended, allowing re-entry.");
        battleStarted = false;
    }

    // Vr�t� bod, kam se hr�� teleportuje po �t�ku
    public Transform GetExitSpawnPoint()
    {
        return exitSpawnPoint;
    }
}

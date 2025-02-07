using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Pøidáno pro podporu NavMeshAgent

public class BattleTrigger : MonoBehaviour
{
    public GameObject existingEnemy; // Pøiøaï existujícího nepøítele ruènì v Unity Inspectoru
    public Transform playerBattlePosition; // Pozice hráèe bìhem boje
    public Transform enemyBattlePosition; // Pozice nepøítele bìhem boje
    public Transform battleCameraPosition; // Pozice kamery bìhem boje
    public Transform exitSpawnPoint; // Kam se hráè teleportuje po úniku
    public BattleController battleController; // Odkaz na BattleController

    private bool battleStarted = false; // Zajistí, že boj se nespustí vícekrát
    private Vector3 enemyOriginalPosition; // Uloží pùvodní pozici nepøítele
    private Quaternion enemyOriginalRotation; // Uloží pùvodní rotaci nepøítele

    private void Start()
    {
        if (existingEnemy != null)
        {
            enemyOriginalPosition = existingEnemy.transform.position;
            enemyOriginalRotation = existingEnemy.transform.rotation;
        }
    }

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
        if (existingEnemy == null)
        {
            Debug.LogError("No enemy assigned to BattleTrigger! Please assign an existing enemy in the scene.");
            return;
        }

        battleStarted = true; // Oznaèí, že bitva byla zahájena

        // Resetujeme pozici nepøítele na bojovou pozici
        existingEnemy.transform.position = enemyBattlePosition.position;

        // Resetujeme zdraví nepøítele
        UnitController enemyController = existingEnemy.GetComponent<UnitController>();
        if (enemyController != null)
        {
            enemyController.currentHealth = enemyController.unitScriptableObject.health;
            enemyController.UpdateHud();
        }

        // Spustí setup bitvy v BattleController
        if (battleController != null)
        {
            battleController.SetupBattle(player, existingEnemy, battleCameraPosition, playerBattlePosition.position, this);
        }
        else
        {
            Debug.LogError("BattleController is not assigned to BattleTrigger!");
        }
    }

    public void ResetBattle(GameObject player)
    {
        Debug.Log("Battle ended, teleporting player to exit spawn point.");
        battleStarted = false;

        UnitController playerController = player.GetComponent<UnitController>();
        UnitController enemyController = existingEnemy.GetComponent<UnitController>();

        // Reset nepøítele s penalizací
        if (existingEnemy != null)
        {
            existingEnemy.transform.position = enemyOriginalPosition;
            StartCoroutine(SmoothResetRotation(existingEnemy, enemyOriginalRotation, 2f));

            if (enemyController != null && playerController != null)
            {
                enemyController.ResetStats(playerController); // Nepøítel dostane penalizaci, pokud mìl ménì celkových statistik
                playerController.ResetStats(enemyController); // Hráè dostane penalizaci, pokud mìl ménì celkových statistik
            }
        }

        // Teleportace hráèe zpìt na výchozí pozici
        if (exitSpawnPoint != null && player != null)
        {
            player.transform.position = exitSpawnPoint.position;
            player.transform.rotation = exitSpawnPoint.rotation;
        }
        else
        {
            Debug.LogError("Exit spawn point or player reference is missing!");
        }
    }



    // Coroutine pro plynulý návrat rotace nepøítele
    private IEnumerator SmoothResetRotation(GameObject enemy, Quaternion targetRotation, float duration)
    {
        float timeElapsed = 0f;
        Quaternion startRotation = enemy.transform.rotation;

        while (timeElapsed < duration)
        {
            enemy.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null; // Èeká na další snímek
        }

        enemy.transform.rotation = targetRotation; // Ujistíme se, že finální rotace je pøesnì ta pùvodní
    }

    // Vrátí bod, kam se hráè teleportuje po útìku
    public Transform GetExitSpawnPoint()
    {
        return exitSpawnPoint;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.AI; // P�id�no pro podporu NavMeshAgent

public class BattleTrigger : MonoBehaviour
{
    public GameObject existingEnemy; // P�i�a� existuj�c�ho nep��tele ru�n� v Unity Inspectoru
    public Transform playerBattlePosition; // Pozice hr��e b�hem boje
    public Transform enemyBattlePosition; // Pozice nep��tele b�hem boje
    public Transform battleCameraPosition; // Pozice kamery b�hem boje
    public Transform exitSpawnPoint; // Kam se hr�� teleportuje po �niku
    public BattleController battleController; // Odkaz na BattleController

    private bool battleStarted = false; // Zajist�, �e boj se nespust� v�cekr�t
    private Vector3 enemyOriginalPosition; // Ulo�� p�vodn� pozici nep��tele
    private Quaternion enemyOriginalRotation; // Ulo�� p�vodn� rotaci nep��tele

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
        if (other.CompareTag("Player") && !battleStarted) // Hr�� vstoupil a bitva nen� aktivn�
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

        battleStarted = true; // Ozna��, �e bitva byla zah�jena

        // Resetujeme pozici nep��tele na bojovou pozici
        existingEnemy.transform.position = enemyBattlePosition.position;

        // Resetujeme zdrav� nep��tele
        UnitController enemyController = existingEnemy.GetComponent<UnitController>();
        if (enemyController != null)
        {
            enemyController.currentHealth = enemyController.unitScriptableObject.health;
            enemyController.UpdateHud();
        }

        // Spust� setup bitvy v BattleController
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

        // Reset nep��tele s penalizac�
        if (existingEnemy != null)
        {
            existingEnemy.transform.position = enemyOriginalPosition;
            StartCoroutine(SmoothResetRotation(existingEnemy, enemyOriginalRotation, 2f));

            if (enemyController != null && playerController != null)
            {
                enemyController.ResetStats(playerController); // Nep��tel dostane penalizaci, pokud m�l m�n� celkov�ch statistik
                playerController.ResetStats(enemyController); // Hr�� dostane penalizaci, pokud m�l m�n� celkov�ch statistik
            }
        }

        // Teleportace hr��e zp�t na v�choz� pozici
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



    // Coroutine pro plynul� n�vrat rotace nep��tele
    private IEnumerator SmoothResetRotation(GameObject enemy, Quaternion targetRotation, float duration)
    {
        float timeElapsed = 0f;
        Quaternion startRotation = enemy.transform.rotation;

        while (timeElapsed < duration)
        {
            enemy.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null; // �ek� na dal�� sn�mek
        }

        enemy.transform.rotation = targetRotation; // Ujist�me se, �e fin�ln� rotace je p�esn� ta p�vodn�
    }

    // Vr�t� bod, kam se hr�� teleportuje po �t�ku
    public Transform GetExitSpawnPoint()
    {
        return exitSpawnPoint;
    }
}

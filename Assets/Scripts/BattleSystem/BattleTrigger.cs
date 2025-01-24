using UnityEngine;

public class BattleTrigger : MonoBehaviour
{
    public Transform playerSummon;
    public Transform enemySummon;
    public Transform battleCameraPosition;
    public GameObject enemyPrefab;

    private bool battleStarted = false;

    void OnTriggerEnter(Collider other)
    {
        if (!battleStarted && other.CompareTag("Player"))
        {
            StartBattle(other.gameObject);
        }
    }

    private void StartBattle(GameObject player)
    {
        battleStarted = true;

        // Pøesun hráèe na bojovou pozici
        player.transform.position = playerSummon.position;
        player.transform.rotation = playerSummon.rotation;

        // Vytvoøení nepøítele
        GameObject enemy = Instantiate(enemyPrefab, enemySummon.position, Quaternion.identity);

        // Aktivace kamery pro boj
        Camera.main.transform.position = battleCameraPosition.position;
        Camera.main.transform.rotation = battleCameraPosition.rotation;

        // Nastavení BattleControlleru
        BattleController battleController = FindObjectOfType<BattleController>();
        battleController.SetupBattle(player, enemy);
    }
}

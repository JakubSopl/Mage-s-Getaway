using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    enum GameState
    {
        TURN_PLAYER,
        TURN_ENEMY,
        WIN,
        LOSS
    }

    public GameObject battleUI; // Odkaz na rodi�ovsk� objekt pro bojov� UI
    public UnitHud PlayerHud;
    public UnitHud EnemyHud;
    public BattleHud BattleHud;
    public BattleCameraController cameraController;

    private GameObject player;
    private GameObject enemy;
    private UnitController playerController;
    private UnitController enemyController;
    private GameState state;

    public void SetupBattle(GameObject player, GameObject enemy, Transform battleCameraPosition, Vector3 playerBattlePosition)
    {
        Debug.Log("SetupBattle started.");

        this.player = player;
        this.enemy = enemy;

        // P�epni kameru na boj
        cameraController.EnterBattleMode(battleCameraPosition);

        // Aktivuj bojov� UI
        battleUI.SetActive(true);

        // Inicializace UnitController pro hr��e a nep��tele
        var playerController = player.GetComponent<UnitController>();
        var enemyController = enemy.GetComponent<UnitController>();

        if (playerController == null || enemyController == null)
        {
            Debug.LogError("PlayerController or EnemyController is null!");
            return;
        }

        // Deaktivace pohybu hr��e
        var movement = player.GetComponent<Movement>();
        if (movement != null)
        {
            movement.isInBattle = true;
            movement.EnterBattleMode(playerBattlePosition);
        }

        // Oto�en� hr��e sm�rem k nep��teli
        player.transform.LookAt(new Vector3(enemy.transform.position.x, player.transform.position.y, enemy.transform.position.z));

        // Oto�en� nep��tele sm�rem k hr��i
        enemy.transform.LookAt(new Vector3(player.transform.position.x, enemy.transform.position.y, player.transform.position.z));

        // Nastav HUDy
        PlayerHud.StartHud(playerController);
        EnemyHud.StartHud(enemyController);

        Debug.Log("Battle HUD initialized.");
        BattleHud.ChooseText();
    }




    private void TurnPlayer()
    {
        if (enemyController.currentHealth <= 0)
        {
            state = GameState.WIN;
            EndBattle();
        }
        else
        {
            state = GameState.TURN_ENEMY;
            StartCoroutine(TurnEnemy());
        }
    }

    private IEnumerator TurnEnemy()
    {
        yield return new WaitForSeconds(0.1f);
        enemyController.MakeTurn(playerController, () =>
        {
            if (playerController.currentHealth <= 0)
            {
                state = GameState.LOSS;
                EndBattle();
            }
            else
            {
                state = GameState.TURN_PLAYER;
                BattleHud.ChooseText();
            }
        });
    }

    private void EndBattle()
    {
        if (state == GameState.WIN)
        {
            Destroy(enemy); // Zni� nep��tele
            BattleHud.EndText(true);
        }
        else if (state == GameState.LOSS)
        {
            Destroy(player); // Zni� hr��e
            BattleHud.EndText(false);
        }

        // P�epni zp�t na kameru t�et� osoby
        cameraController.ExitBattleMode();
    }

    public void ButtonAttack()
    {
        if (state != GameState.TURN_PLAYER) return;

        state = GameState.TURN_ENEMY;
        playerController.AttackTurn(enemyController, TurnPlayer);
    }

    public void ButtonHeal()
    {
        if (state != GameState.TURN_PLAYER) return;

        if (!playerController.EnoughManaForSpell("heal"))
        {
            BattleHud.ManaText(playerController.unitScriptableObject.healMana);
            return;
        }

        state = GameState.TURN_ENEMY;
        playerController.HealTurn(TurnPlayer);
    }

    public void ButtonRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ButtonExit()
    {
        Application.Quit();
    }
}

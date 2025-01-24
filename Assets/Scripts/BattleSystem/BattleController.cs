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

    public UnitHud PlayerHud;
    public UnitHud EnemyHud;
    public BattleHud BattleHud;

    private GameObject player;
    private GameObject enemy;
    private UnitController playerController;
    private UnitController enemyController;
    private GameState state;

    public void SetupBattle(GameObject player, GameObject enemy)
    {
        this.player = player;
        this.enemy = enemy;

        playerController = player.GetComponent<UnitController>();
        enemyController = enemy.GetComponent<UnitController>();

        state = GameState.TURN_PLAYER;

        // Nastav HUD
        StartCoroutine(PlayerHud.StartHud(PlayerHud, playerController));
        StartCoroutine(EnemyHud.StartHud(EnemyHud, enemyController));

        playerController.SetBattleHud(BattleHud);
        enemyController.SetBattleHud(BattleHud);

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
            Destroy(enemy); // Zniè nepøítele
            BattleHud.EndText(true);
        }
        else if (state == GameState.LOSS)
        {
            Destroy(player); // Zniè hráèe
            BattleHud.EndText(false);
        }
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

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

    public GameObject battleUI;
    public UnitHud PlayerHud;
    public UnitHud EnemyHud;
    public BattleHud BattleHud;
    public BattleCameraController cameraController;
    private BattleTrigger battleTrigger;

    private GameObject player;
    private GameObject enemy;
    private UnitController playerController;
    private UnitController enemyController;
    private GameState state;

    void Start()
    {
        ShowCursor(true);
    }

    public void ShowCursor(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    void OnDestroy()
    {
        ShowCursor(false);
    }


    public void SetupBattle(GameObject player, GameObject enemy, Transform battleCameraPosition, Vector3 playerBattlePosition, BattleTrigger trigger)
    {
        Debug.Log("SetupBattle started.");

        this.player = player;
        this.enemy = enemy;
        this.battleTrigger = trigger; // Uložíme aktuální BattleTrigger

        cameraController.EnterBattleMode(battleCameraPosition);

        if (battleUI != null)
        {
            battleUI.SetActive(true);
        }
        else
        {
            Debug.LogError("Battle UI is not assigned!");
        }

        playerController = player.GetComponent<UnitController>();
        enemyController = enemy.GetComponent<UnitController>();

        if (playerController == null || enemyController == null)
        {
            Debug.LogError("PlayerController or EnemyController is null!");
            return;
        }

        if (enemyController.currentHealth == 0)
        {
            enemyController.currentHealth = enemyController.unitScriptableObject.health;
        }

        if (playerController.currentHealth == 0)
        {
            playerController.currentHealth = playerController.unitScriptableObject.health;
        }

        Debug.Log($"[SetupBattle] Player HP: {playerController.currentHealth}, Enemy HP: {enemyController.currentHealth}");

        var movement = player.GetComponent<Movement>();
        if (movement != null)
        {
            movement.isInBattle = true;
            movement.EnterBattleMode(playerBattlePosition);
        }

        player.transform.LookAt(new Vector3(enemy.transform.position.x, player.transform.position.y, enemy.transform.position.z));
        enemy.transform.LookAt(new Vector3(player.transform.position.x, enemy.transform.position.y, player.transform.position.z));

        if (PlayerHud != null)
        {
            PlayerHud.StartHud(playerController);
            PlayerHud.UpdateHud(playerController);
            Debug.Log("Player HUD initialized.");
        }
        else
        {
            Debug.LogError("Player HUD is not assigned!");
        }

        if (EnemyHud != null)
        {
            enemyController.hud = EnemyHud;
            enemyController.hud.StartHud(enemyController);
            enemyController.hud.UpdateHud(enemyController);
            Debug.Log("Enemy HUD initialized.");
        }
        else
        {
            Debug.LogError("Enemy HUD is not assigned!");
        }

        if (BattleHud != null)
        {
            BattleHud.ChooseText();
        }
        else
        {
            Debug.LogError("BattleHud is not assigned!");
        }

        state = GameState.TURN_PLAYER;
    }


    private IEnumerator DebugCheckHealth()
    {
        yield return new WaitForSeconds(0.2f);
        Debug.Log($"[Delayed Check] Player HP: {playerController.currentHealth}, Enemy HP: {enemyController.currentHealth}");
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
        yield return new WaitForSeconds(0.5f);

        Debug.Log("Enemy attacks player.");
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
            Destroy(enemy);
            BattleHud.EndText(true);
            Debug.Log("Player won the battle!");
        }
        else if (state == GameState.LOSS)
        {
            Destroy(player);
            BattleHud.EndText(false);
            Debug.Log("Player lost the battle!");
        }

        cameraController.ExitBattleMode();

        battleUI.SetActive(false);

        var movement = player.GetComponent<Movement>();
        if (movement != null)
        {
            movement.isInBattle = false;
        }
    }

    public void ButtonAttack()
    {
        if (state != GameState.TURN_PLAYER)
        {
            Debug.LogWarning("Cannot attack: It's not the player's turn.");
            return;
        }

        if (playerController == null || enemyController == null)
        {
            Debug.LogError("PlayerController or EnemyController is null!");
            return;
        }

        Debug.Log("Player attacks enemy.");
        state = GameState.TURN_ENEMY;
        playerController.AttackTurn(enemyController, TurnPlayer);
    }

    public void ButtonHeal()
    {
        if (state != GameState.TURN_PLAYER)
        {
            Debug.LogWarning("Cannot heal: It's not the player's turn.");
            return;
        }

        if (playerController == null)
        {
            Debug.LogError("PlayerController is null!");
            return;
        }

        if (!playerController.EnoughManaForSpell("heal"))
        {
            Debug.LogWarning("Not enough mana to heal.");
            BattleHud.ManaText(playerController.unitScriptableObject.healMana);
            return;
        }

        Debug.Log("Player heals.");
        state = GameState.TURN_ENEMY;
        playerController.HealTurn(TurnPlayer);
    }

    public void ButtonStrongAttack()
    {
        if (state != GameState.TURN_PLAYER) return;

        state = GameState.TURN_ENEMY;
        playerController.StrongAttackTurn(enemyController, TurnPlayer);
    }

    public void ButtonRestoreMana()
    {
        if (state != GameState.TURN_PLAYER)
        {
            Debug.LogWarning("Cannot restore mana: It's not the player's turn.");
            return;
        }

        if (playerController == null)
        {
            Debug.LogError("PlayerController is null!");
            return;
        }

        // Kontrola, zda je mana už na maximu
        if (playerController.currentMana >= playerController.unitScriptableObject.mana)
        {
            Debug.Log("Mana is already full!");
            BattleHud.FullManaText(playerController.unitScriptableObject.name); // Zobrazí zprávu v HUDu
            return;
        }

        int restoredMana = 10; // Množství obnovené many
        Debug.Log($"Player restored {restoredMana} mana.");

        playerController.RestoreManaTurn(restoredMana, TurnPlayer); // Použije novou metodu

        state = GameState.TURN_ENEMY;
    }


    public void ButtonRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ButtonExit()
    {
        if (state == GameState.WIN || state == GameState.LOSS)
        {
            Debug.Log("Battle is already over.");
            return;
        }

        if (state != GameState.TURN_PLAYER)
        {
            Debug.LogWarning("You can only escape during your turn!");
            BattleHud.CannotEscapeText(); // Zobrazí zprávu pøes BattleHud
            return;
        }

        Debug.Log("Player is escaping the battle...");

        // Získáme exitSpawnPoint z aktuálního BattleTriggeru
        Transform exitPoint = battleTrigger?.GetExitSpawnPoint();
        if (exitPoint != null)
        {
            Debug.Log("Teleporting player to exit point at: " + exitPoint.position);

            // Ovìøíme, zda má hráè CharacterController a pokud ano, vypneme ho pøed pøesunem
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
                player.transform.position = exitPoint.position;
                controller.enabled = true;
            }
            else
            {
                player.transform.position = exitPoint.position;
            }
        }
        else
        {
            Debug.LogWarning("Exit spawn point is not set! Using default position.");
            player.transform.position += new Vector3(5f, 0f, 5f); // Posune hráèe opodál
        }

        battleUI.SetActive(false);
        cameraController.ExitBattleMode();
        state = GameState.TURN_PLAYER;

        var movement = player.GetComponent<Movement>();
        if (movement != null)
        {
            movement.isInBattle = false;
        }

        battleTrigger.ResetBattle(player); // Resetuje možnost znovu vstoupit do boje

        Debug.Log("Player has successfully escaped the battle.");
    }

}

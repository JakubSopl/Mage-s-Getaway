using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random; // Pro náhodné chování AI

public class UnitController : MonoBehaviour
{
    public enum UnitState
    {
        IDLE,
        BUSY
    }

    public UnitScriptableObject unitScriptableObject;
    public UnitState state;
    public UnitHud hud;
    public BattleHud battleHud;
    public Animator animator;

    public int currentHealth, currentMana, currentDamage, currentDefense;

    private bool initialized = false;
    public RuntimeAnimatorController defaultController;

    public GameObject attackEffect;
    public GameObject strongAttackEffect;
    public GameObject healEffect;

    private void SpawnEffect(GameObject effectPrefab, Transform target)
    {
        if (effectPrefab == null || target == null) return;

        GameObject effect = Instantiate(effectPrefab, target.position, Quaternion.identity);
        Destroy(effect, 2f);
    }

    void Start()
    {
        if (!initialized)
        {
            InitializeStats();
        }

        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator component is MISSING!");
            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("Animator has NO CONTROLLER assigned! Assigning it now...");
            if (defaultController != null)
            {
                animator.runtimeAnimatorController = defaultController;
                Debug.Log($"Animator Controller set to: {defaultController.name}");
            }
            else
            {
                Debug.LogError("Failed to assign Animator Controller! Please check Inspector.");
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {

            if (animator != null)
            {
                Debug.Log("Manually triggering AttackTrigger.");
                animator.SetTrigger("AttackTrigger");
            }
            else
            {
                Debug.LogError("Animator is NULL!");
            }
        }
    }

    private void InitializeStats()
    {
        if (initialized) return;

        state = UnitState.IDLE;
        currentHealth = unitScriptableObject.health;
        currentMana = unitScriptableObject.mana;
        currentDamage = unitScriptableObject.damage;
        currentDefense = unitScriptableObject.defense;
        initialized = true;

        UpdateHud();
    }

    public void UpdateHud()
    {
        if (hud != null)
        {
            hud.UpdateHud(this);
        }
    }

    public void SetHud(UnitHud hud)
    {
        this.hud = hud;
        hud.unitHP.maxValue = unitScriptableObject.health;
        hud.unitMP.maxValue = unitScriptableObject.mana;
        hud.UpdateHud(this);
    }

    public void SetBattleHud(BattleHud battleHud)
    {
        this.battleHud = battleHud;
    }

    public void EnterBattleMode()
    {
        if (animator != null)
        {
            animator.SetBool("IsInBattle", true);

            // Pokud je hráè ve stavu "Falling Idle", vynutíme pøechod do "Fighting Idle"
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Falling Idle"))
            {
                animator.Play("Fighting Idle"); // Pøímá zmìna animace
            }
        }
    }


    public void ExitBattleMode()
    {
        if (animator != null)
        {
            animator.SetBool("IsInBattle", false);
        }
    }

    private void ResetTriggers()
    {
        if (animator == null) return;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(param.name);
            }
        }
    }

    public void MakeTurn(UnitController other, Action onTurnComplete)
    {
        StartCoroutine(EnemyTurn(other, onTurnComplete));
    }

    private IEnumerator EnemyTurn(UnitController other, Action onTurnComplete)
    {
        yield return new WaitForSeconds(Random.Range(1.0f, 2.5f)); // Náhodné zpoždìní pøed útokem

        int action = Random.Range(0, 3); // 0 = Normální útok, 1 = Silný útok, 2 = Heal

        switch (action)
        {
            case 0:
                AttackTurn(other, onTurnComplete);
                break;
            case 1:
                StrongAttackTurn(other, onTurnComplete);
                break;
            case 2:
                if (currentHealth < unitScriptableObject.health && currentMana >= unitScriptableObject.healMana)
                {
                    HealTurn(onTurnComplete);
                }
                else
                {
                    AttackTurn(other, onTurnComplete);
                }
                break;
        }
    }


    public void AttackTurn(UnitController other, Action onTurnComplete)
    {
        if (currentMana < unitScriptableObject.attackMana)
        {
            battleHud.ManaText(unitScriptableObject.attackMana);
            onTurnComplete?.Invoke();
            return;
        }

        state = UnitState.BUSY;
        ResetTriggers();
        animator.SetTrigger("AttackTrigger");

        currentMana -= unitScriptableObject.attackMana;
        int damage = Math.Max(0, currentDamage - other.currentDefense);

        // Spawn efektu na cíli
        SpawnEffect(attackEffect, other.transform);

        StartCoroutine(ApplyDamageAfterDelay(other, damage, onTurnComplete));
    }

    public void StrongAttackTurn(UnitController other, Action onTurnComplete)
    {
        if (currentMana < unitScriptableObject.strongAttackMana)
        {
            battleHud.ManaText(unitScriptableObject.strongAttackMana);
            onTurnComplete?.Invoke();
            return;
        }

        state = UnitState.BUSY;
        ResetTriggers();
        animator.SetTrigger("StrongAttackTrigger");

        currentMana -= unitScriptableObject.strongAttackMana;
        int damage = Math.Max(0, unitScriptableObject.strongAttackDamage - other.currentDefense);

        // Spawn efektu na cíli
        SpawnEffect(strongAttackEffect, other.transform);

        StartCoroutine(ApplyDamageAfterDelay(other, damage, onTurnComplete));
    }

    public void HealTurn(Action onTurnComplete)
    {
        state = UnitState.BUSY;
        ResetTriggers();
        animator.SetTrigger("HealTrigger");

        currentMana -= unitScriptableObject.healMana;
        int healAmount = unitScriptableObject.healPower;
        currentHealth = Math.Min(unitScriptableObject.health, currentHealth + healAmount);
        battleHud.HealText(unitScriptableObject.name, healAmount);

        // Spawn efektu na sobì
        SpawnEffect(healEffect, this.transform);

        UpdateHud();

        StartCoroutine(EndTurnWithDelay(onTurnComplete));
    }

    private IEnumerator ApplyDamageAfterDelay(UnitController target, int damage, Action onTurnComplete)
    {
        yield return new WaitForSeconds(1.0f);
        target.TakeDamage(damage);

        if (hud != null)
        {
            hud.UpdateHud(this);
        }

        state = UnitState.IDLE;
        onTurnComplete?.Invoke();
    }

    private IEnumerator EndTurnWithDelay(Action onTurnComplete)
    {
        yield return new WaitForSeconds(1.5f);
        state = UnitState.IDLE;
        onTurnComplete?.Invoke();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, unitScriptableObject.health);
        battleHud.DamageText(unitScriptableObject.name, damage);
        ResetTriggers();
        animator.SetTrigger("TakeDamageTrigger");

        UpdateHud();

        if (currentHealth <= 0)
        {
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        ResetTriggers();
        animator.SetTrigger("DeathTrigger");
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    public void Delete()
    {
        StartCoroutine(DeathSequence());
    }

    public bool EnoughManaForSpell(string name)
    {
        if (name == "heal" && currentMana < unitScriptableObject.healMana)
            return false;
        return true;
    }
}

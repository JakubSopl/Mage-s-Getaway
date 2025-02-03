using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random; // Pro n�hodn� chov�n� AI

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
    public GameObject restoreManaEffect; 

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

            // Pokud je hr�� ve stavu "Falling Idle", vynut�me p�echod do "Fighting Idle"
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Falling Idle"))
            {
                animator.Play("Fighting Idle"); // P��m� zm�na animace
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
        yield return new WaitForSeconds(Random.Range(1.0f, 2.5f)); // N�hodn� zpo�d�n� p�ed �tokem

        int actionRoll = Random.Range(0, 10); // Rozd�len� 0-9 pro p�esn� �ance

        if (actionRoll < 4) // 0-3 (40%) Norm�ln� �tok
        {
            AttackTurn(other, onTurnComplete);
        }
        else if (actionRoll < 6) // 4-5 (20%) Siln� �tok
        {
            StrongAttackTurn(other, onTurnComplete);
        }
        else if (actionRoll < 8) // 6-7 (20%) Heal, pokud m� m�n� ne� max HP
        {
            if (currentHealth < unitScriptableObject.health && currentMana >= unitScriptableObject.healMana)
            {
                HealTurn(onTurnComplete);
            }
            else
            {
                AttackTurn(other, onTurnComplete); // Pokud nem��e healovat, ud�l� norm�ln� �tok
            }
        }
        else // 8-9 (20%) Obnova many, pokud je pod 50%
        {
            if (currentMana < unitScriptableObject.mana / 2)
            {
                RestoreManaTurn(10, onTurnComplete); // P�id� 10 many
            }
            else
            {
                AttackTurn(other, onTurnComplete); // Pokud nem� smysl obnovit manu, ud�l� norm�ln� �tok
            }
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

        // Spawn efektu na c�li
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

        // Spawn efektu na c�li
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

        // Spawn efektu na sob�
        SpawnEffect(healEffect, this.transform);

        UpdateHud();

        StartCoroutine(EndTurnWithDelay(onTurnComplete));
    }

    public void RestoreManaTurn(int manaAmount, Action onTurnComplete)
    {
        state = UnitState.BUSY;
        ResetTriggers();
        animator.SetTrigger("HealTrigger"); // P�ehraje stejnou animaci jako p�i healu

        currentMana = Mathf.Min(unitScriptableObject.mana, currentMana + manaAmount);
        battleHud.RestoreManaText(unitScriptableObject.name, manaAmount); // Zobraz� zpr�vu v HUDu

        // Spawn efektu na sob�
        SpawnEffect(restoreManaEffect, this.transform);

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

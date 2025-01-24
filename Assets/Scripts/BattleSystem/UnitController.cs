using System;
using System.Collections;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public enum UnitState
    {
        IDLE,
        BUSY,
        MOVING
    }

    public UnitScriptableObject unitScriptableObject;
    public UnitState state;
    public UnitHud hud;
    public BattleHud battleHud;
    public UnitParticleController particleController;
    public UnitSoundController soundController;

    private Vector3 otherPos;
    private Action onMoveComplete;
    public UnitBase unit;

    public int currentHealth, currentMana, currentDamage, currentDefense;

    void Start()
    {
        unit = GetComponent<UnitBase>();
        soundController = GetComponent<UnitSoundController>();
        state = UnitState.IDLE;
        currentHealth = unitScriptableObject.health;
        currentMana = unitScriptableObject.mana;
        currentDamage = unitScriptableObject.damage;
        currentDefense = unitScriptableObject.defense;
    }

    public void SetHud(UnitHud hud)
    {
        this.hud = hud;
    }

    public void SetBattleHud(BattleHud battleHud)
    {
        this.battleHud = battleHud;
    }

    public void MakeTurn(UnitController other, Action onTurnComplete)
    {
        if (currentHealth + unitScriptableObject.healPower <= unitScriptableObject.health && EnoughManaForSpell("heal"))
            HealTurn(onTurnComplete);
        else
            AttackTurn(other, onTurnComplete);
    }

    public void AttackTurn(UnitController other, Action onTurnComplete)
    {
        battleHud.UsedText(unitScriptableObject.name, "Attack");

        Vector3 startPosition = transform.position;
        Vector3 moveTargetPosition = other.transform.position + (transform.position - other.transform.position).normalized * 2f;

        MoveToPosition(moveTargetPosition, () =>
        {
            state = UnitState.BUSY;
            unit.LookAt(other.transform.position); // Otoè smìrem k nepøíteli
            other.unit.PlayAnimation("HitTrigger");

            unit.PlayForcedAnimation("AttackTrigger", () =>
            {
                int damage = Math.Max(0, currentDamage - other.currentDefense);
                battleHud.DamageText(other.unitScriptableObject.name, damage);

                other.currentHealth -= damage;
                other.hud.UpdateHud(other);

                MoveToPosition(startPosition, () =>
                {
                    state = UnitState.IDLE;
                    unit.PlayAnimation("MoveTrigger");
                    onTurnComplete();
                });
            });
        });
    }

    public void HealTurn(Action onTurnComplete)
    {
        battleHud.UsedText(unitScriptableObject.name, "Heal");

        StartCoroutine(battleHud.HealText(unitScriptableObject.name, unitScriptableObject.healPower));
        soundController.PlaySound("Heal");

        StartCoroutine(particleController.PlayParticle("Heal", () =>
        {
            currentMana -= unitScriptableObject.healMana;
            currentHealth = Math.Min(unitScriptableObject.health, currentHealth + unitScriptableObject.healPower);
            hud.UpdateHud(this);
            onTurnComplete();
        }));
    }

    public bool EnoughManaForSpell(string name)
    {
        if (name == "heal" && currentMana < unitScriptableObject.healMana)
            return false;
        return true;
    }

    private void Update()
    {
        if (state == UnitState.MOVING)
        {
            float moveSpeed = 5f;
            transform.position += (otherPos - transform.position).normalized * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, otherPos) < 0.1f)
            {
                transform.position = otherPos;
                state = UnitState.IDLE;
                onMoveComplete?.Invoke();
            }
        }
    }

    private void MoveToPosition(Vector3 otherPos, Action onMoveComplete)
    {
        unit.PlayAnimation("MoveTrigger");
        unit.LookAt(otherPos); // Pøidání otoèení
        this.otherPos = otherPos;
        this.onMoveComplete = onMoveComplete;
        state = UnitState.MOVING;
    }

    public void Delete()
    {
        unit.PlayAnimation("DeathTrigger");
        StartCoroutine(particleController.PlayParticle("Death", () => { Destroy(this.gameObject, 1); }));
    }
}

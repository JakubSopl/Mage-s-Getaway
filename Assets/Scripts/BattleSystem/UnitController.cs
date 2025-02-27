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
    public GameObject restoreManaEffect;

    public AudioSource audioSource;
    public AudioClip healSound;
    public AudioClip manaRestoreSound;
    public AudioClip attackSound;
    public AudioClip strongAttackSound;
    public AudioClip exitBattleSound;
    public AudioClip damageSound;


    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

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

        // Zajistíme, že se currentDefense opravdu vygeneruje
        currentDefense = GetRandomDefense();
        Debug.Log($"{unitScriptableObject.name} má random defense: {currentDefense}");

        initialized = true;
        UpdateHud();
    }
    private int GetRandomDefense()
    {
        int roll = Random.Range(0, 100); // Rozsah 0-99

        if (roll < 30) return 1;  // 30% šance
        else if (roll < 55) return 2;  // 25% šance
        else if (roll < 75) return 3;  // 20% šance
        else if (roll < 90) return 4;  // 15% šance
        else return 5; // 10% šance
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
        // Pøehrání exitBattleSound na 25% hlasitosti
        if (exitBattleSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(exitBattleSound, 0.25f);
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
        if (state == UnitState.BUSY) return; // Zabráníme dvojímu volání tahu

        // Pøi každém tahu nastavíme novou hodnotu obrany
        currentDefense = GetRandomDefense();
        Debug.Log($"{unitScriptableObject.name} získal novou obranu: {currentDefense}");

        state = UnitState.BUSY; // Nastavíme jednotku do stavu "zaneprázdnìná"

        StartCoroutine(EnemyTurn(other, onTurnComplete));
    }


    private IEnumerator EnemyTurn(UnitController other, Action onTurnComplete)
    {
        // Náhodné zpoždìní pøed útokem pro simulaci reakèní doby nepøítele
        yield return new WaitForSeconds(Random.Range(1.0f, 2.5f));

        // Pokud má nepøítel 0 many, obnoví manu místo útoku
        if (currentMana <= 0)
        {
            RestoreManaTurn(15, onTurnComplete); // Pøidá 15 many
            yield break; // Ukonèí funkci, aby nepokraèovala v dalších akcích
        }

        // Náhodný výbìr akce (hodnota od 0 do 9)
        int actionRoll = Random.Range(0, 10);

        if (actionRoll < 4) // 0-3 (40%) Normální útok
        {
            AttackTurn(other, onTurnComplete);
        }
        else if (actionRoll < 6) // 4-5 (20%) Silný útok
        {
            StrongAttackTurn(other, onTurnComplete);
        }
        else if (actionRoll < 8) // 6-7 (20%) Heal, pokud má ménì než max HP
        {
            if (currentHealth < unitScriptableObject.health && currentMana >= unitScriptableObject.healMana)
            {
                HealTurn(onTurnComplete);
            }
            else
            {
                AttackTurn(other, onTurnComplete); // Pokud nemùže healovat, provede normální útok
            }
        }
        else // 8-9 (20%) Obnova many, pokud je pod 50%
        {
            if (currentMana < unitScriptableObject.mana / 2)
            {
                RestoreManaTurn(10, onTurnComplete); // Pøidá 10 many
            }
            else
            {
                AttackTurn(other, onTurnComplete); // Pokud není potøeba obnova many, provede normální útok
            }
        }

        // Zajištìní, že tah nepøítele vždy skonèí
        yield return new WaitForSeconds(1.5f);
        state = UnitState.IDLE;
        onTurnComplete?.Invoke();
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
        PlaySound(attackSound);

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
        PlaySound(strongAttackSound);

        StartCoroutine(ApplyDamageAfterDelay(other, damage, onTurnComplete));
    }

    public void HealTurn(Action onTurnComplete)
    {
        if (currentHealth >= unitScriptableObject.health)
        {
            battleHud.FullHealthText(unitScriptableObject.name); // Zpráva, že HP je již plné
            onTurnComplete?.Invoke();
            return;
        }

        state = UnitState.BUSY;
        ResetTriggers();
        animator.SetTrigger("HealTrigger");

        currentMana -= unitScriptableObject.healMana;
        int healAmount = unitScriptableObject.healPower;
        currentHealth = Math.Min(unitScriptableObject.health, currentHealth + healAmount);
        battleHud.HealText(unitScriptableObject.name, healAmount);

        // Spawn efektu na sobì
        SpawnEffect(healEffect, this.transform);
        PlaySound(healSound);

        UpdateHud();

        StartCoroutine(EndTurnWithDelay(onTurnComplete));
    }

    public void RestoreManaTurn(int manaAmount, Action onTurnComplete)
    {
        state = UnitState.BUSY;
        ResetTriggers();
        animator.SetTrigger("HealTrigger"); // Pøehraje stejnou animaci jako pøi healu

        currentMana = Mathf.Min(unitScriptableObject.mana, currentMana + manaAmount);
        battleHud.RestoreManaText(unitScriptableObject.name, manaAmount); // Zobrazí zprávu v HUDu

        // Spawn efektu na sobì
        SpawnEffect(restoreManaEffect, this.transform);
        PlaySound(manaRestoreSound);

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

        yield return new WaitForSeconds(1.0f); // Dáme vizuální efekt pøed dalším tahem

        state = UnitState.IDLE; // Ujistíme se, že jednotka dokonèila tah
        onTurnComplete?.Invoke(); // Pøedáme tah dál
    }

    private IEnumerator EndTurnWithDelay(Action onTurnComplete)
    {
        yield return new WaitForSeconds(1.5f);

        state = UnitState.IDLE; // Oprava: vždy zajistí, že jednotka dokonèí svùj tah
        onTurnComplete?.Invoke();
    }


    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, unitScriptableObject.health);

        // Pøehraje zvuk poškození
        PlaySound(damageSound);

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
        //Destroy(gameObject);
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

    public bool wasWeakerOnExit = false; // Penalizace pro hráèe
    public bool opponentWasWeakerOnExit = false; // Penalizace pro nepøítele


    public void ResetStats(UnitController opponent)
    {
        Debug.Log($"{unitScriptableObject.name} stats reset!");

        int previousHealth = currentHealth;
        int previousMana = currentMana;

        // Reset základních hodnot na maximum
        currentHealth = unitScriptableObject.health;
        currentMana = unitScriptableObject.mana;
        currentDamage = unitScriptableObject.damage;
        // Vygenerování nové hodnoty pro defense
        currentDefense = GetRandomDefense();

        if (opponent != null)
        {
            // Spoèítáme celkové HP + Mana pro obì jednotky
            int totalPreviousPlayerStats = previousHealth + previousMana;
            int totalPreviousEnemyStats = opponent.currentHealth + opponent.currentMana;

        }

        UpdateHud(); // Aktualizace UI
    }
}

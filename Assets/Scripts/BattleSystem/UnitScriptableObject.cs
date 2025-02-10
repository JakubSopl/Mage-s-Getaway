using UnityEngine;

[CreateAssetMenu(fileName = "UnitScriptableObject", menuName = "JAGAZEM/Unit")]
public class UnitScriptableObject : ScriptableObject
{
    public new string name;

    public int health;
    public int mana;

    public int damage;
    public int attackMana;

    [SerializeField] private int defense;

    public int healPower;
    public int healMana;

    public int strongAttackDamage;
    public int strongAttackMana;

    public int Defense => defense; // Getter

    public void Initialize()
    {
        defense = GetRandomDefense();
    }

    private int GetRandomDefense()
    {
        int roll = Random.Range(1, 101);

        if (roll <= 30) return 1;
        else if (roll <= 55) return 2;
        else if (roll <= 75) return 3;
        else if (roll <= 90) return 4;
        else return 5;
    }
}

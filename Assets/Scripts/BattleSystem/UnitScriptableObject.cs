using UnityEngine;

[CreateAssetMenu(fileName = "UnitScriptableObject", menuName = "JAGAZEM/Unit")]
public class UnitScriptableObject : ScriptableObject
{
    public new string name;

    public int health;
    public int mana;

    public int damage;
    public int defense;

    public int healPower = 25;
    public int healMana = 10;

    // Nové atributy pro útoky
    public int attackMana = 5;  // Náklady na normální útok
    public int strongAttackDamage = 35;
    public int strongAttackMana = 20; // Náklady na silný útok
}

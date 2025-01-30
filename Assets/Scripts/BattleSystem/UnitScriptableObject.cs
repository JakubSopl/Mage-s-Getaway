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

    // Nov� atributy pro �toky
    public int attackMana = 5;  // N�klady na norm�ln� �tok
    public int strongAttackDamage = 35;
    public int strongAttackMana = 20; // N�klady na siln� �tok
}

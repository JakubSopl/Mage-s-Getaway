using UnityEngine;

[CreateAssetMenu(fileName = "UnitScriptableObject", menuName = "JAGAZEM/Unit")]
public class UnitScriptableObject : ScriptableObject
{
    public new string name;

    public int health;
    public int mana;

    public int damage;
    public int attackMana;

    public int healPower;
    public int healMana;

    public int strongAttackDamage;
    public int strongAttackMana;
}

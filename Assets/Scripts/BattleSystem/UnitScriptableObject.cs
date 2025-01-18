using UnityEngine;

[CreateAssetMenu(fileName = "UnitScriptableObject", menuName = "JAGAZEM/Unit")]
public class UnitScriptableObject : ScriptableObject
{
    public new string name;

    public int health;
    public int mana;

    public int damage;
    public int defense;

    public int healPower = 30;
    public int healMana = 10;
}

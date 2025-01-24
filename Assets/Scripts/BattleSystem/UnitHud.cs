using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitHud : MonoBehaviour
{
    public TMP_Text unitName;
    public Slider unitHP, unitMP;

    public IEnumerator StartHud(UnitHud hud, UnitController unit)
    {
        yield return new WaitUntil(() => unit.currentHealth != 0);

        unitName.text = unit.unitScriptableObject.name;
        unitHP.maxValue = unit.unitScriptableObject.health;
        unitMP.maxValue = unit.unitScriptableObject.mana;

        UpdateHud(unit);
        AssignHud(hud, unit);
    }

    public void UpdateHud(UnitController unit)
    {
        unitHP.value = unit.currentHealth;
        unitMP.value = unit.currentMana;
    }

    public void AssignHud(UnitHud hud, UnitController unit)
    {
        unit.SetHud(hud);
    }
}

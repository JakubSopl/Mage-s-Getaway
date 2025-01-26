using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitHud : MonoBehaviour
{
    public TMP_Text unitName; // Textov� pole pro jm�no jednotky
    public Slider unitHP;     // Slider pro zdrav�
    public Slider unitMP;     // Slider pro manu

    public void StartHud(UnitController unit)
    {
        // Inicializace jm�na a maxim�ln�ch hodnot slider�
        unitName.text = unit.unitScriptableObject.name;
        unitHP.maxValue = unit.unitScriptableObject.health;
        unitMP.maxValue = unit.unitScriptableObject.mana;

        // Nastaven� aktu�ln�ch hodnot
        UpdateHud(unit);
    }

    public void UpdateHud(UnitController unit)
    {
        // Aktualizace aktu�ln�ch hodnot na sliderech
        unitHP.value = unit.currentHealth;
        unitMP.value = unit.currentMana;
    }
}

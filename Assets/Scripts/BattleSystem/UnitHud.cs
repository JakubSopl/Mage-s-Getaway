using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitHud : MonoBehaviour
{
    public TMP_Text unitName; // Textové pole pro jméno jednotky
    public Slider unitHP;     // Slider pro zdraví
    public Slider unitMP;     // Slider pro manu

    public void StartHud(UnitController unit)
    {
        // Inicializace jména a maximálních hodnot sliderù
        unitName.text = unit.unitScriptableObject.name;
        unitHP.maxValue = unit.unitScriptableObject.health;
        unitMP.maxValue = unit.unitScriptableObject.mana;

        // Nastavení aktuálních hodnot
        UpdateHud(unit);
    }

    public void UpdateHud(UnitController unit)
    {
        // Aktualizace aktuálních hodnot na sliderech
        unitHP.value = unit.currentHealth;
        unitMP.value = unit.currentMana;
    }
}

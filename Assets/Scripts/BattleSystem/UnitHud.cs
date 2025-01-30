using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitHud : MonoBehaviour
{
    public TMP_Text unitName; // Textové pole pro jméno jednotky
    public Slider unitHP;     // Slider pro zdraví
    public Slider unitMP;     // Slider pro manu
    public float animationSpeed = 2.0f; // Rychlost animace pøechodu

    public void StartHud(UnitController unit)
    {
        // Inicializace maximálních hodnot
        unitName.text = unit.unitScriptableObject.name;
        unitHP.maxValue = unit.unitScriptableObject.health;
        unitMP.maxValue = unit.unitScriptableObject.mana;

        // Nastavení aktuálních hodnot
        unitHP.value = unit.currentHealth;
        unitMP.value = unit.currentMana;

        // Debugging pro ovìøení
        Debug.Log($"Initializing HUD for {unit.unitScriptableObject.name}: Max HP = {unitHP.maxValue}, Max Mana = {unitMP.maxValue}");
    }

    public void UpdateHud(UnitController unit)
    {
        if (!gameObject.activeInHierarchy) return; // Zabrání volání coroutines, když není aktivní

        Debug.Log($"Updating HUD for {unit.unitScriptableObject.name}: HP = {unit.currentHealth}, Mana = {unit.currentMana}");

        StartCoroutine(AnimateSliderChange(unitHP, unit.currentHealth));
        StartCoroutine(AnimateSliderChange(unitMP, unit.currentMana));
    }

    private IEnumerator AnimateSliderChange(Slider slider, float targetValue)
    {
        if (slider == null) yield break; // Zajistí, že slider není null
        if (!gameObject.activeInHierarchy) yield break; // Zabrání spuštìní animace, pokud je objekt deaktivován

        float startValue = slider.value;
        float elapsedTime = 0f;

        while (elapsedTime < 1f / animationSpeed)
        {
            elapsedTime += Time.deltaTime;
            slider.value = Mathf.Lerp(startValue, targetValue, elapsedTime * animationSpeed);
            yield return null;
        }

        slider.value = targetValue; // Zajistí pøesné nastavení finální hodnoty
    }
}

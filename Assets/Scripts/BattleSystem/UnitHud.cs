using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitHud : MonoBehaviour
{
    public TMP_Text unitName; // Textov� pole pro jm�no jednotky
    public Slider unitHP;     // Slider pro zdrav�
    public Slider unitMP;     // Slider pro manu
    public float animationSpeed = 2.0f; // Rychlost animace p�echodu

    public void StartHud(UnitController unit)
    {
        // Inicializace maxim�ln�ch hodnot
        unitName.text = unit.unitScriptableObject.name;
        unitHP.maxValue = unit.unitScriptableObject.health;
        unitMP.maxValue = unit.unitScriptableObject.mana;

        // Nastaven� aktu�ln�ch hodnot
        unitHP.value = unit.currentHealth;
        unitMP.value = unit.currentMana;

        // Debugging pro ov��en�
        Debug.Log($"Initializing HUD for {unit.unitScriptableObject.name}: Max HP = {unitHP.maxValue}, Max Mana = {unitMP.maxValue}");
    }

    public void UpdateHud(UnitController unit)
    {
        if (!gameObject.activeInHierarchy) return; // Zabr�n� vol�n� coroutines, kdy� nen� aktivn�

        Debug.Log($"Updating HUD for {unit.unitScriptableObject.name}: HP = {unit.currentHealth}, Mana = {unit.currentMana}");

        StartCoroutine(AnimateSliderChange(unitHP, unit.currentHealth));
        StartCoroutine(AnimateSliderChange(unitMP, unit.currentMana));
    }

    private IEnumerator AnimateSliderChange(Slider slider, float targetValue)
    {
        if (slider == null) yield break; // Zajist�, �e slider nen� null
        if (!gameObject.activeInHierarchy) yield break; // Zabr�n� spu�t�n� animace, pokud je objekt deaktivov�n

        float startValue = slider.value;
        float elapsedTime = 0f;

        while (elapsedTime < 1f / animationSpeed)
        {
            elapsedTime += Time.deltaTime;
            slider.value = Mathf.Lerp(startValue, targetValue, elapsedTime * animationSpeed);
            yield return null;
        }

        slider.value = targetValue; // Zajist� p�esn� nastaven� fin�ln� hodnoty
    }
}

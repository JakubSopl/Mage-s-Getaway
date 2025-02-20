using UnityEngine;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset1 Preset;

    [SerializeField, Range(0, 24)] private float TimeOfDay = 10f;
    [SerializeField] private float daytimeSpeedMultiplier;
    [SerializeField] private float nighttimeSpeedMultiplier;
    [SerializeField] private float sunriseSunsetMultiplier;

    [SerializeField] private float timeScaleFactor = 0.33f; // 0.33 znamená 3x pomalejší den

    [SerializeField] private float baselineDirectionalIntensity = 1f;
    [SerializeField] private float baselineAmbientIntensity = 1f;

    private void Update()
    {
        if (Preset == null)
            return;

        if (Application.isPlaying)
        {
            float timeMultiplier;

            if (TimeOfDay >= 4.5f && TimeOfDay < 6.2f)
            {
                timeMultiplier = sunriseSunsetMultiplier;
            }
            else if (TimeOfDay >= 17.8f && TimeOfDay < 19.2f)
            {
                timeMultiplier = sunriseSunsetMultiplier;
            }
            else if (TimeOfDay >= 5f && TimeOfDay < 17f)
            {
                timeMultiplier = daytimeSpeedMultiplier;
            }
            else
            {
                timeMultiplier = nighttimeSpeedMultiplier;
            }

            // Použití globálního zpomalovacího faktoru
            TimeOfDay += Time.deltaTime * timeMultiplier * timeScaleFactor;
            TimeOfDay %= 24;

            UpdateLighting(TimeOfDay / 24f);
        }
        else
        {
            UpdateLighting(TimeOfDay / 24f);
        }
    }

    private void UpdateLighting(float timePercent)
    {
        Color ambientColor = Preset.AmbientColor.Evaluate(timePercent);
        Color fogColor = Preset.FogColor.Evaluate(timePercent);

        if (TimeOfDay >= 5 && TimeOfDay < 17)
        {
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.fogColor = fogColor;
        }
        else if (TimeOfDay >= 4.5f && TimeOfDay < 6.2f)
        {
            RenderSettings.ambientLight = Color.Lerp(ambientColor, Color.white, 0.2f);
            RenderSettings.fogColor = Color.Lerp(fogColor, Color.yellow, 0.1f);
        }
        else if (TimeOfDay >= 17.8f && TimeOfDay < 19.2f)
        {
            RenderSettings.ambientLight = Color.Lerp(ambientColor, Color.red, 0.2f);
            RenderSettings.fogColor = Color.Lerp(fogColor, Color.red, 0.1f);
        }
        else
        {
            RenderSettings.ambientLight = Color.Lerp(ambientColor, Color.blue, 0.2f);
            RenderSettings.fogColor = Color.Lerp(fogColor, Color.black, 0.2f);
        }

        RenderSettings.ambientIntensity = baselineAmbientIntensity;

        if (DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);
            DirectionalLight.intensity = baselineDirectionalIntensity;
            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }
    }

    private void OnValidate()
    {
        if (DirectionalLight != null)
            return;

        if (RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                    return;
                }
            }
        }
    }
}

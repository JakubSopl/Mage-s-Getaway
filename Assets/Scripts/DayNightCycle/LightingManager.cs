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

        float targetAmbientIntensity;

        if (TimeOfDay >= 5 && TimeOfDay < 17)
        {
            targetAmbientIntensity = 1.0f;
        }
        else if (TimeOfDay >= 4.5f && TimeOfDay < 6.2f)
        {
            float t = Mathf.InverseLerp(4.5f, 6.2f, TimeOfDay);
            targetAmbientIntensity = Mathf.Lerp(1.3f, 1.0f, t);
        }
        else if (TimeOfDay >= 17.8f && TimeOfDay < 19.2f)
        {
            float t = Mathf.InverseLerp(17.8f, 19.2f, TimeOfDay);
            targetAmbientIntensity = Mathf.Lerp(1.0f, 1.3f, t);
        }
        else // NOC
        {
            targetAmbientIntensity = 1.2f;
        }

        baselineAmbientIntensity = Mathf.Lerp(baselineAmbientIntensity, targetAmbientIntensity, Time.deltaTime * 0.5f);

        RenderSettings.ambientLight = ambientColor;
        RenderSettings.fogColor = fogColor;
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

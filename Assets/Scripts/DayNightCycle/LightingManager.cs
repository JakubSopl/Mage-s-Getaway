using UnityEngine;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    // Scene References
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset1 Preset;

    // Variables
    [SerializeField, Range(0, 24)] private float TimeOfDay = 10f; // Default to 10:00 for baseline lighting
    [SerializeField] private float daytimeSpeedMultiplier = 0.05f;
    [SerializeField] private float nighttimeSpeedMultiplier = 0.1f;
    [SerializeField] private float sunriseSunsetMultiplier = 0.015f; 

    // Light intensity settings - baseline for consistent visibility
    [SerializeField] private float baselineDirectionalIntensity = 1f; // Baseline intensity for all times
    [SerializeField] private float baselineAmbientIntensity = 1f; // Baseline ambient intensity

    private void Update()
    {
        if (Preset == null)
            return;

        if (Application.isPlaying)
        {
            // Determine the speed multiplier based on the current time of day
            float timeMultiplier;

            if (TimeOfDay >= 4.5f && TimeOfDay < 6.2f) // Sunrise period (4:30 - 6:12)
            {
                timeMultiplier = sunriseSunsetMultiplier;
            }
            else if (TimeOfDay >= 17.8f && TimeOfDay < 19.2f) // Sunset period (17:48 - 19:12)
            {
                timeMultiplier = sunriseSunsetMultiplier;
            }
            else if (TimeOfDay >= 5f && TimeOfDay < 17f) // Daytime period (5:00 - 17:00)
            {
                timeMultiplier = daytimeSpeedMultiplier;
            }
            else // Nighttime period
            {
                timeMultiplier = nighttimeSpeedMultiplier;
            }

            // Update time of day with the appropriate speed multiplier
            TimeOfDay += Time.deltaTime * timeMultiplier;
            TimeOfDay %= 24; // Modulus to keep TimeOfDay within 0-24

            UpdateLighting(TimeOfDay / 24f);
        }
        else
        {
            UpdateLighting(TimeOfDay / 24f);
        }
    }

    private void UpdateLighting(float timePercent)
    {
        // Set ambient and fog color based on the gradient with slight variations based on timePercent
        Color ambientColor = Preset.AmbientColor.Evaluate(timePercent);
        Color fogColor = Preset.FogColor.Evaluate(timePercent);

        // Apply slight variations to simulate sunrise, sunset, and nighttime without reducing visibility
        if (TimeOfDay >= 5 && TimeOfDay < 17) // Daytime (5:00 to 17:00)
        {
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.fogColor = fogColor;
        }
        else if (TimeOfDay >= 4.5f && TimeOfDay < 6.2f) // Sunrise
        {
            RenderSettings.ambientLight = Color.Lerp(ambientColor, Color.white, 0.2f); // Warmer tint
            RenderSettings.fogColor = Color.Lerp(fogColor, Color.yellow, 0.1f);
        }
        else if (TimeOfDay >= 17.8f && TimeOfDay < 19.2f) // Sunset
        {
            RenderSettings.ambientLight = Color.Lerp(ambientColor, Color.red, 0.2f); // Warm sunset tint
            RenderSettings.fogColor = Color.Lerp(fogColor, Color.red, 0.1f);
        }
        else // Nighttime
        {
            RenderSettings.ambientLight = Color.Lerp(ambientColor, Color.blue, 0.2f); // Slightly cooler tint
            RenderSettings.fogColor = Color.Lerp(fogColor, Color.black, 0.2f);
        }

        // Set consistent ambient and directional light intensities based on the baseline values
        RenderSettings.ambientIntensity = baselineAmbientIntensity;

        // If the directional light is set, update its color and intensity
        if (DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);
            DirectionalLight.intensity = baselineDirectionalIntensity;
            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }
    }

    // Try to find a directional light to use if we haven't set one
    private void OnValidate()
    {
        if (DirectionalLight != null)
            return;

        // Search for lighting tab sun
        if (RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        // Search scene for light that fits criteria (directional)
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
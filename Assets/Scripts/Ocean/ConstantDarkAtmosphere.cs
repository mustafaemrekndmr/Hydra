using UnityEngine;

/// <summary>
/// Legacy atmosphere controller - now handled by UnderwaterEffectController's
/// depth-based gradient system. Kept for backward compatibility but does nothing
/// in LateUpdate to avoid interfering with the depth gradient.
/// Call ForceReapply() if you need a one-time override.
/// </summary>
public class ConstantDarkAtmosphere : MonoBehaviour
{
    [Header("Deep Ocean Settings (reference only)")]
    public Color fogColor = new Color(0.02f, 0.1f, 0.2f, 1f);
    public float fogDensity = 0.025f;
    public Color ambientColor = new Color(0.05f, 0.15f, 0.25f);
    public float sunIntensity = 0.5f;
    public Color sunColor = new Color(0.3f, 0.5f, 0.7f);
    
    [Header("References")]
    public Light directionalLight;
    
    void Start()
    {
        // Do nothing — UnderwaterEffectController handles atmosphere now
        // This prevents double-application of settings that override the depth gradient
    }
    
    /// <summary>
    /// One-time override if needed
    /// </summary>
    public void ForceReapply()
    {
        if (directionalLight == null)
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    break;
                }
            }
        }
        
        RenderSettings.fog = true;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;
        
        if (directionalLight != null)
        {
            directionalLight.intensity = sunIntensity;
            directionalLight.color = sunColor;
        }
    }
}

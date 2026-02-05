using UnityEngine;

/// <summary>
/// Forces constant dark atmosphere regardless of depth
/// Overrides any depth-based lighting changes
/// </summary>
public class ConstantDarkAtmosphere : MonoBehaviour
{
    [Header("Constant Dark Settings")]
    public Color constantFogColor = new Color(0.015f, 0.18f, 0.3f, 1f); // Balanced dark blue
    public float constantFogDensity = 0.05f; // Moderate fog
    public Color constantAmbient = new Color(0.03f, 0.12f, 0.22f); // Visible but dark
    public float constantSunIntensity = 0.35f; // Dim but present
    
    [Header("References")]
    public Light directionalLight;
    
    void Start()
    {
        // Find directional light if not assigned
        if (directionalLight == null)
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    break;
                }
            }
        }
        
        ApplyConstantDarkness();
    }
    
    void LateUpdate()
    {
        // Force constant darkness every frame
        // This overrides any other script trying to change lighting
        ApplyConstantDarkness();
    }
    
    void ApplyConstantDarkness()
    {
        // Force fog settings
        RenderSettings.fog = true;
        RenderSettings.fogColor = constantFogColor;
        RenderSettings.fogDensity = constantFogDensity;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        
        // Force ambient
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = constantAmbient;
        
        // Force directional light
        if (directionalLight != null)
        {
            directionalLight.intensity = constantSunIntensity;
            directionalLight.color = new Color(0.2f, 0.3f, 0.5f); // Dim blue
        }
    }
}

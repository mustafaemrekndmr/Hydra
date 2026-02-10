using UnityEngine;

/// <summary>
/// Controls underwater visual effects with depth-based smooth gradients.
/// The scene is ALWAYS underwater — we never switch to skybox/void.
/// Fog, ambient light, and camera color smoothly shift based on depth.
/// Auto-detects WaterSurface GameObject for proper water level.
/// </summary>
[RequireComponent(typeof(Camera))]
public class UnderwaterEffectController : MonoBehaviour
{
    [Header("Water Settings (auto-detected)")]
    public float waterLevel = 10f;
    
    [Header("Shallow Water (near surface)")]
    public Color shallowFogColor = new Color(0.04f, 0.15f, 0.28f);
    public Color shallowAmbient = new Color(0.06f, 0.18f, 0.3f);
    public float shallowFogDensity = 0.018f;
    
    [Header("Deep Water (max depth)")]
    public Color deepFogColor = new Color(0.01f, 0.04f, 0.1f);
    public Color deepAmbient = new Color(0.02f, 0.06f, 0.12f);
    public float deepFogDensity = 0.04f;
    
    [Header("Depth Gradient")]
    [Tooltip("Depth at which deep-water look is fully applied")]
    public float maxGradientDepth = 30f;
    
    [Header("Directional Light")]
    public float shallowSunIntensity = 0.6f;
    public float deepSunIntensity = 0.15f;
    public Color sunColor = new Color(0.3f, 0.5f, 0.7f);
    
    // State
    private Camera cam;
    private Light directionalLight;
    private float currentDepth;
    private float smoothDepth;
    
    /// <summary>Always true in ocean scene</summary>
    public bool IsUnderwater => true;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        
        // Auto-detect water surface
        GameObject waterSurface = GameObject.Find("WaterSurface");
        if (waterSurface != null)
            waterLevel = waterSurface.transform.position.y;
        
        // Find directional light
        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                directionalLight = light;
                break;
            }
        }
        
        // Force camera to solid color always (no skybox void)
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 150f;
        
        // Initial setup
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
        
        smoothDepth = Mathf.Max(0f, waterLevel - transform.position.y);
        ApplyDepthEffects(smoothDepth);
    }
    
    void Update()
    {
        // Calculate current depth from water surface
        currentDepth = Mathf.Max(0f, waterLevel - transform.position.y);
        
        // Smooth depth changes for visual comfort
        smoothDepth = Mathf.Lerp(smoothDepth, currentDepth, Time.deltaTime * 3f);
        
        ApplyDepthEffects(smoothDepth);
    }
    
    void ApplyDepthEffects(float depth)
    {
        // Normalized depth ratio (0 = surface, 1 = maxGradientDepth or deeper)
        float t = Mathf.Clamp01(depth / maxGradientDepth);
        
        // Smooth curve for more natural transition
        float curve = t * t * (3f - 2f * t); // smoothstep
        
        // ── Fog ──
        Color fogColor = Color.Lerp(shallowFogColor, deepFogColor, curve);
        float fogDensity = Mathf.Lerp(shallowFogDensity, deepFogDensity, curve);
        
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
        
        // ── Camera background (matches fog so no void visible) ──
        cam.backgroundColor = fogColor;
        
        // ── Ambient light ──
        Color ambient = Color.Lerp(shallowAmbient, deepAmbient, curve);
        RenderSettings.ambientLight = ambient;
        
        // ── Directional light ──
        if (directionalLight != null)
        {
            directionalLight.intensity = Mathf.Lerp(shallowSunIntensity, deepSunIntensity, curve);
            directionalLight.color = sunColor;
        }
    }
}

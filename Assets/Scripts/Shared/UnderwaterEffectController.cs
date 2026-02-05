using UnityEngine;

/// <summary>
/// Controls underwater visual effects including fog, color grading, and improved lighting
/// </summary>
[RequireComponent(typeof(Camera))]
public class UnderwaterEffectController : MonoBehaviour
{
    [Header("Water Settings")]
    public float waterLevel = 0f;
    
    [Header("Underwater Visuals")]
    public Color underwaterColor = new Color(0.02f, 0.2f, 0.35f, 1f); // Balanced ocean blue
    public float fogDensity = 0.05f; // Moderate fog for visibility
    public float nearClipPlane = 0.1f;
    
    [Header("Lighting Adjustments")]
    public float underwaterExposure = 0.6f; // Balanced exposure
    public Color ambientColor = new Color(0.04f, 0.15f, 0.28f); // Visible ambient
    public Color sunlightColor = new Color(0.35f, 0.55f, 0.75f); // Balanced sunlight
    public float sunlightIntensity = 0.4f; // Moderate intensity
    
    [Header("Depth-Based Effects")]
    public bool enableDepthGradient = false; // DISABLED - constant dark atmosphere
    public float maxDepth = 50f;
    public Color deepWaterColor = new Color(0.005f, 0.05f, 0.1f, 1f); // Almost black at depth
    
    [Header("Particle Effects")]
    public bool enableParticles = true;
    public GameObject particlePrefab; // Optional: floating particles
    
    // Private state
    private bool isUnderwater = false;
    private Color defaultFogColor;
    private float defaultFogDensity;
    private Material defaultSkybox;
    private Color defaultAmbientColor;
    private Camera cam;
    private Light directionalLight;
    private Color defaultLightColor;
    private float defaultLightIntensity;
    private GameObject particleSystem;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        
        // Find directional light (sun)
        directionalLight = FindObjectOfType<Light>();
        if (directionalLight != null && directionalLight.type == LightType.Directional)
        {
            defaultLightColor = directionalLight.color;
            defaultLightIntensity = directionalLight.intensity;
        }
        
        // Store default settings
        defaultFogColor = RenderSettings.fogColor;
        defaultFogDensity = RenderSettings.fogDensity;
        defaultSkybox = RenderSettings.skybox;
        defaultAmbientColor = RenderSettings.ambientLight;
        
        // Initial check
        CheckWaterState();
    }
    
    void Update()
    {
        CheckWaterState();
        
        // Depth-based effects DISABLED - keep constant dark atmosphere
        // if (isUnderwater && enableDepthGradient)
        // {
        //     UpdateDepthEffects();
        // }
    }
    
    void CheckWaterState()
    {
        // Simple height check for water level
        if (transform.position.y < waterLevel - 0.1f)
        {
            if (!isUnderwater) EnterWater();
        }
        else
        {
            if (isUnderwater) ExitWater();
        }
    }
    
    void UpdateDepthEffects()
    {
        // Calculate depth below water surface
        float depth = Mathf.Abs(transform.position.y - waterLevel);
        float depthRatio = Mathf.Clamp01(depth / maxDepth);
        
        // Gradually darken as we go deeper
        Color currentFogColor = Color.Lerp(underwaterColor, deepWaterColor, depthRatio);
        RenderSettings.fogColor = currentFogColor;
        cam.backgroundColor = currentFogColor;
        
        // Increase fog density with depth
        float currentFogDensity = Mathf.Lerp(fogDensity, fogDensity * 2f, depthRatio);
        RenderSettings.fogDensity = currentFogDensity;
        
        // Dim light with depth
        if (directionalLight != null)
        {
            float lightIntensity = Mathf.Lerp(sunlightIntensity, sunlightIntensity * 0.3f, depthRatio);
            directionalLight.intensity = lightIntensity;
        }
    }
    
    void EnterWater()
    {
        isUnderwater = true;
        
        // 1. Enable Fog
        RenderSettings.fog = true;
        RenderSettings.fogColor = underwaterColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogMode = FogMode.ExponentialSquared; // Realistic falloff
        
        // 2. Change Camera Background
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = underwaterColor;
        cam.nearClipPlane = nearClipPlane;
        
        // 3. Adjust Lighting Atmosphere
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;
        
        // 4. Adjust Directional Light (Sun)
        if (directionalLight != null)
        {
            directionalLight.color = sunlightColor;
            directionalLight.intensity = sunlightIntensity;
        }
        
        // 5. Disable Skybox reflection
        RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
        
        // 6. Add floating particles (optional)
        if (enableParticles && particlePrefab != null && particleSystem == null)
        {
            particleSystem = Instantiate(particlePrefab, transform.position, Quaternion.identity);
            particleSystem.transform.SetParent(transform);
        }
        
        Debug.Log("Entering Underwater Mode - Deep Ocean Atmosphere");
    }
    
    void ExitWater()
    {
        isUnderwater = false;
        
        // 1. Restore Fog
        RenderSettings.fogColor = defaultFogColor;
        RenderSettings.fogDensity = defaultFogDensity;
        
        // 2. Restore Camera Background
        if (defaultSkybox != null)
        {
            cam.clearFlags = CameraClearFlags.Skybox;
            RenderSettings.skybox = defaultSkybox;
        }
        else
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = defaultFogColor;
        }
        
        // 3. Restore Lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientLight = defaultAmbientColor;
        RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;
        
        // 4. Restore Directional Light
        if (directionalLight != null)
        {
            directionalLight.color = defaultLightColor;
            directionalLight.intensity = defaultLightIntensity;
        }
        
        // 5. Remove particles
        if (particleSystem != null)
        {
            Destroy(particleSystem);
            particleSystem = null;
        }
        
        Debug.Log("Exiting Underwater Mode");
    }
}

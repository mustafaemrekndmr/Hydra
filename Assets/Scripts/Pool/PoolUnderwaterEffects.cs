using UnityEngine;

/// <summary>
/// Pool-specific underwater effect controller
/// Lighter, clearer water for indoor pool environment
/// </summary>
public class PoolUnderwaterEffects : MonoBehaviour
{
    [Header("Pool Water Settings")]
    public float waterLevel = 0f;
    
    [Header("Pool Visuals - Clear Water")]
    public Color poolWaterColor = new Color(0.1f, 0.4f, 0.6f, 1f); // Clear pool blue
    public float fogDensity = 0.02f; // Very light fog for clear pool
    public float nearClipPlane = 0.1f;
    
    [Header("Pool Lighting - Bright")]
    public float poolExposure = 1.0f; // Bright indoor lighting
    public Color ambientColor = new Color(0.2f, 0.3f, 0.4f); // Bright ambient
    public Color lightColor = new Color(0.9f, 0.95f, 1f); // Bright white light
    public float lightIntensity = 1.0f; // Full brightness
    
    [Header("References")]
    public Camera mainCamera;
    public Light directionalLight;
    
    private bool isUnderwater = false;
    private Color originalFogColor;
    private float originalFogDensity;
    
    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
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
        
        // Store original settings
        originalFogColor = RenderSettings.fogColor;
        originalFogDensity = RenderSettings.fogDensity;
    }
    
    void Update()
    {
        CheckWaterState();
    }
    
    void CheckWaterState()
    {
        if (transform.position.y < waterLevel - 0.1f)
        {
            if (!isUnderwater) EnterWater();
        }
        else
        {
            if (isUnderwater) ExitWater();
        }
    }
    
    void EnterWater()
    {
        isUnderwater = true;
        
        // Pool underwater settings - bright and clear
        RenderSettings.fog = true;
        RenderSettings.fogColor = poolWaterColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        
        // Bright ambient for pool
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;
        
        // Camera settings
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = poolWaterColor;
            mainCamera.nearClipPlane = nearClipPlane;
        }
        
        // Bright directional light for pool
        if (directionalLight != null)
        {
            directionalLight.color = lightColor;
            directionalLight.intensity = lightIntensity;
        }
        
        Debug.Log("Entered pool water - clear and bright");
    }
    
    void ExitWater()
    {
        isUnderwater = false;
        
        // Restore settings
        RenderSettings.fog = true;
        RenderSettings.fogColor = originalFogColor;
        RenderSettings.fogDensity = originalFogDensity;
        
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.Skybox;
        }
        
        Debug.Log("Exited pool water");
    }
}

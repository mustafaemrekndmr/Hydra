using UnityEngine;

/// <summary>
/// Master controller for all ocean scene enhancements.
/// Finds existing scene elements (WaterSurface, Terrain) and
/// initializes atmospheric + gameplay effects on top of them.
/// </summary>
public class OceanSceneEnhancer : MonoBehaviour
{
    [Header("Scene References (auto-detected)")]
    public Camera mainCamera;
    public Transform rovTransform;
    public Transform waterSurface;
    
    [Header("Enable/Disable Effects")]
    public bool enableParticles = true;
    public bool enableHUD = true;
    public bool enableCameraShake = true;
    public bool enableBioluminescence = true;
    public bool enableSonar = true;
    
    [Header("Water & Atmosphere")]
    [Tooltip("Fog color tint for deep ocean")]
    public Color deepOceanFog = new Color(0.02f, 0.1f, 0.2f, 1f);
    public float fogDensity = 0.025f;
    public Color ambientLight = new Color(0.05f, 0.15f, 0.25f);
    
    private bool initialized = false;
    
    void Start()
    {
        InitializeOcean();
    }
    
    void InitializeOcean()
    {
        if (initialized) return;
        initialized = true;
        
        // ── Auto-detect references ──
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (rovTransform == null)
        {
            GameObject rov = GameObject.Find("ROV");
            if (rov != null) rovTransform = rov.transform;
        }
        
        if (waterSurface == null)
        {
            GameObject ws = GameObject.Find("WaterSurface");
            if (ws != null) waterSurface = ws.transform;
        }
        
        Rigidbody rovRb = rovTransform != null ? rovTransform.GetComponent<Rigidbody>() : null;
        float waterY = waterSurface != null ? waterSurface.position.y : 10f;
        
        // Atmosphere is now handled by UnderwaterEffectController (depth gradient)
        
        // ── 3. Underwater Particles ──
        if (enableParticles && mainCamera != null)
        {
            // Particles follow camera
            UnderwaterParticles particles = mainCamera.gameObject.AddComponent<UnderwaterParticles>();
        }
        
        // ── 4. ROV HUD with correct water level ──
        if (enableHUD && mainCamera != null)
        {
            ROVHUD hud = mainCamera.gameObject.AddComponent<ROVHUD>();
            hud.rovTransform = rovTransform;
            hud.rovRigidbody = rovRb;
            hud.waterSurfaceY = waterY;
        }
        
        // ── 5. Camera Shake ──
        if (enableCameraShake && mainCamera != null)
        {
            UnderwaterCameraShake shake = mainCamera.gameObject.AddComponent<UnderwaterCameraShake>();
            shake.shakeIntensity = 0.015f;
            shake.rovRigidbody = rovRb;
        }
        
        // ── 6. Bioluminescent Life ──
        if (enableBioluminescence)
        {
            GameObject go = new GameObject("BioLife");
            BioluminescentLife bio = go.AddComponent<BioluminescentLife>();
        }
        
        // ── 7. Sonar System ──
        if (enableSonar && rovTransform != null)
        {
            ROVSonar sonar = rovTransform.gameObject.AddComponent<ROVSonar>();
            sonar.sonarRange = 50f;
            sonar.toggleKey = KeyCode.Tab;
        }
    }
}


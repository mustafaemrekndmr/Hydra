using UnityEngine;

/// <summary>
/// Master controller for ocean scene runtime effects.
/// Finds EXISTING ROV and ChargingStation in the scene (built by Editor tool),
/// then adds runtime-only scripts: HUD, Sonar, Particles, Camera effects.
/// Does NOT create or destroy scene objects.
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
    
    private bool initialized = false;
    
    void Start()
    {
        InitializeOcean();
    }
    
    void InitializeOcean()
    {
        if (initialized) return;
        initialized = true;
        
        // ── Auto-detect scene references ──
        if (waterSurface == null)
        {
            GameObject ws = GameObject.Find("WaterSurface");
            if (ws != null) waterSurface = ws.transform;
        }
        
        float waterY = waterSurface != null ? waterSurface.position.y : 10f;
        
        // ── Find existing ROV (created by Editor tool) ──
        if (rovTransform == null)
        {
            GameObject rov = GameObject.Find("ROV");
            if (rov != null) rovTransform = rov.transform;
        }
        
        if (rovTransform == null)
        {
            Debug.LogError("OceanSceneEnhancer: No ROV found! Run Tools > Build Ocean Scene Objects first.");
            return;
        }
        
        // Find camera on ROV
        mainCamera = rovTransform.GetComponentInChildren<Camera>();
        Rigidbody rovRb = rovTransform.GetComponent<Rigidbody>();
        
        if (mainCamera == null)
        {
            Debug.LogError("OceanSceneEnhancer: No Camera found on ROV!");
            return;
        }
        
        // ══════════════════════════════════════
        // ADD RUNTIME EFFECTS (only if not already present)
        // ══════════════════════════════════════
        
        // UnderwaterEffectController
        if (mainCamera.GetComponent<UnderwaterEffectController>() == null)
            mainCamera.gameObject.AddComponent<UnderwaterEffectController>();
        
        // Particles
        if (enableParticles && mainCamera.GetComponent<UnderwaterParticles>() == null)
            mainCamera.gameObject.AddComponent<UnderwaterParticles>();
        
        // HUD
        if (enableHUD && mainCamera.GetComponent<ROVHUD>() == null)
        {
            ROVHUD hud = mainCamera.gameObject.AddComponent<ROVHUD>();
            hud.rovTransform = rovTransform;
            hud.rovRigidbody = rovRb;
            hud.waterSurfaceY = waterY;
        }
        
        // Camera Shake
        if (enableCameraShake && mainCamera.GetComponent<UnderwaterCameraShake>() == null)
        {
            UnderwaterCameraShake shake = mainCamera.gameObject.AddComponent<UnderwaterCameraShake>();
            shake.shakeIntensity = 0.015f;
            shake.rovRigidbody = rovRb;
        }
        
        // Bioluminescent Life
        if (enableBioluminescence && FindAnyObjectByType<BioluminescentLife>() == null)
        {
            GameObject go = new GameObject("BioLife");
            go.AddComponent<BioluminescentLife>();
        }
        
        // Sonar
        if (enableSonar && rovTransform.GetComponent<ROVSonar>() == null)
        {
            ROVSonar sonar = rovTransform.gameObject.AddComponent<ROVSonar>();
            sonar.sonarRange = 50f;
            sonar.toggleKey = KeyCode.Tab;
        }
        
        Debug.Log("<color=green>OceanSceneEnhancer: Runtime effects initialized on existing ROV.</color>");
    }
}

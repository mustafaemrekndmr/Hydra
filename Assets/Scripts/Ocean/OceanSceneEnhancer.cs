using UnityEngine;

/// <summary>
/// Master controller for all ocean scene enhancements
/// Initializes and manages all atmospheric effects
/// </summary>
public class OceanSceneEnhancer : MonoBehaviour
{
    [Header("Scene References")]
    public Camera mainCamera;
    public Transform rovTransform;
    
    [Header("Enable/Disable Effects")]
    public bool enableParticles = true;
    public bool enableHUD = true;
    public bool enableCameraShake = true;
    public bool enableGodRays = false; // DISABLED - too bright for dark atmosphere
    public bool enableOceanFloor = true;
    public bool enableBioluminescence = true;
    
    [Header("Effect Settings")]
    [Range(0f, 1f)]
    public float overallIntensity = 1f;
    
    private GameObject effectsContainer;
    
    void Start()
    {
        InitializeEnhancements();
    }
    
    void InitializeEnhancements()
    {
        Debug.Log("=== Initializing Ocean Scene Enhancements ===");
        
        // Find references if not assigned
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (rovTransform == null)
            rovTransform = GameObject.Find("ROV")?.transform;
        
        // Create container for all effects
        effectsContainer = new GameObject("OceanEffects");
        
        // 1. Underwater Particles
        if (enableParticles && mainCamera != null)
        {
            GameObject particlesObj = new GameObject("ParticleSystem");
            particlesObj.transform.SetParent(mainCamera.transform);
            particlesObj.transform.localPosition = Vector3.zero;
            
            UnderwaterParticles particles = particlesObj.AddComponent<UnderwaterParticles>();
            particles.particleCount = 200;
            particles.spawnRadius = 20f;
            
            Debug.Log("✓ Underwater particles initialized");
        }
        
        // 2. ROV HUD
        if (enableHUD && mainCamera != null)
        {
            ROVHUD hud = mainCamera.gameObject.AddComponent<ROVHUD>();
            hud.rovTransform = rovTransform;
            if (rovTransform != null)
                hud.rovRigidbody = rovTransform.GetComponent<Rigidbody>();
            
            Debug.Log("✓ ROV HUD initialized");
        }
        
        // 3. Camera Shake
        if (enableCameraShake && mainCamera != null)
        {
            UnderwaterCameraShake shake = mainCamera.gameObject.AddComponent<UnderwaterCameraShake>();
            shake.shakeIntensity = 0.02f * overallIntensity;
            if (rovTransform != null)
                shake.rovRigidbody = rovTransform.GetComponent<Rigidbody>();
            
            Debug.Log("✓ Camera shake initialized");
        }
        
        // 4. God Rays
        if (enableGodRays)
        {
            GameObject godRaysObj = new GameObject("GodRaysSystem");
            godRaysObj.transform.SetParent(effectsContainer.transform);
            
            GodRaysEffect godRays = godRaysObj.AddComponent<GodRaysEffect>();
            godRays.rayCount = 8;
            godRays.surfaceHeight = 0f;
            
            Debug.Log("✓ God rays initialized");
        }
        
        // 5. Ocean Floor
        if (enableOceanFloor)
        {
            GameObject floorObj = new GameObject("FloorGenerator");
            floorObj.transform.SetParent(effectsContainer.transform);
            
            OceanFloorGenerator floor = floorObj.AddComponent<OceanFloorGenerator>();
            floor.floorDepth = -10f;
            floor.rockCount = 30;
            
            Debug.Log("✓ Ocean floor initialized");
        }
        
        // 6. Bioluminescent Life
        if (enableBioluminescence)
        {
            GameObject bioObj = new GameObject("BioluminescentSystem");
            bioObj.transform.SetParent(effectsContainer.transform);
            
            BioluminescentLife bio = bioObj.AddComponent<BioluminescentLife>();
            bio.organismCount = 20;
            bio.minDepth = -15f;
            bio.maxDepth = -50f;
            
            Debug.Log("✓ Bioluminescent life initialized");
        }
        
        Debug.Log("=== Ocean Scene Enhancement Complete ===");
        Debug.Log("Press H to toggle HUD");
        Debug.Log("Press L to toggle ROV lights");
    }
    
    void OnGUI()
    {
        // Show initialization status
        if (Time.time < 5f)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 16;
            style.normal.textColor = new Color(0.3f, 1f, 0.3f);
            style.alignment = TextAnchor.LowerCenter;
            
            GUI.Label(new Rect(0, Screen.height - 50, Screen.width, 40), 
                "Ocean Scene Enhanced - Realistic ROV Simulation Active", style);
        }
    }
}

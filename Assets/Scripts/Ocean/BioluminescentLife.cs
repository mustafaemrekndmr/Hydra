using UnityEngine;

/// <summary>
/// Creates bioluminescent organisms that glow in deep water.
/// Optimized: cached Light refs, material sharing, reduced per-frame allocs.
/// </summary>
public class BioluminescentLife : MonoBehaviour
{
    [Header("Bioluminescence Settings")]
    public int organismCount = 20;
    public float spawnRadius = 30f;
    public float minDepth = -15f;
    public float maxDepth = -50f;
    
    [Header("Glow Settings")]
    public Color glowColor = new Color(0.3f, 0.8f, 1f);
    public float glowIntensity = 2f;
    public float glowRange = 3f;
    public float pulseSpeed = 1f;
    
    [Header("Movement")]
    public float swimSpeed = 0.5f;
    public float wanderRadius = 5f;
    
    [Header("Optimization")]
    public bool proximityActivation = true;
    public float activationDistance = 40f;
    
    private Transform[] organismTransforms;
    private Light[] organismLights;
    private Vector3[] targetPositions;
    private float[] pulseTimes;
    private Transform rovTransform;
    private Material sharedGlowMaterial;
    
    void Start()
    {
        rovTransform = GameObject.Find("ROV")?.transform;
        CreateBioluminescentOrganisms();
    }
    
    void CreateBioluminescentOrganisms()
    {
        GameObject container = new GameObject("BioluminescentLife");
        
        organismTransforms = new Transform[organismCount];
        organismLights = new Light[organismCount];
        targetPositions = new Vector3[organismCount];
        pulseTimes = new float[organismCount];
        
        // Shared material for all organisms (huge perf win)
        sharedGlowMaterial = new Material(Shader.Find("Standard"));
        sharedGlowMaterial.EnableKeyword("_EMISSION");
        sharedGlowMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
        sharedGlowMaterial.color = glowColor;
        
        for (int i = 0; i < organismCount; i++)
        {
            float x = Random.Range(-spawnRadius, spawnRadius);
            float y = Random.Range(minDepth, maxDepth);
            float z = Random.Range(-spawnRadius, spawnRadius);
            Vector3 pos = new Vector3(x, y, z);
            
            GameObject organism = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            organism.name = $"BioOrg_{i}";
            organism.transform.SetParent(container.transform);
            organism.transform.position = pos;
            organism.transform.localScale = Vector3.one * Random.Range(0.1f, 0.3f);
            
            // Use shared material
            organism.GetComponent<Renderer>().sharedMaterial = sharedGlowMaterial;
            
            // Remove collider (not needed)
            Object.Destroy(organism.GetComponent<Collider>());
            
            // Add point light
            Light light = organism.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = glowColor;
            light.intensity = glowIntensity;
            light.range = glowRange;
            light.shadows = LightShadows.None;
            light.renderMode = LightRenderMode.ForceVertex; // Cheaper rendering
            
            // Cache references
            organismTransforms[i] = organism.transform;
            organismLights[i] = light;
            targetPositions[i] = pos;
            pulseTimes[i] = Random.Range(0f, Mathf.PI * 2f);
        }
    }
    
    void Update()
    {
        if (organismTransforms == null) return;
        
        float deltaTime = Time.deltaTime;
        Vector3 rovPos = rovTransform != null ? rovTransform.position : Vector3.zero;
        
        for (int i = 0; i < organismTransforms.Length; i++)
        {
            if (organismTransforms[i] == null) continue;
            
            // Proximity check: skip if too far
            if (proximityActivation && rovTransform != null)
            {
                float sqrDist = (organismTransforms[i].position - rovPos).sqrMagnitude;
                float actDistSqr = activationDistance * activationDistance;
                
                bool inRange = sqrDist < actDistSqr;
                organismLights[i].enabled = inRange;
                
                if (!inRange) continue; // Skip animation for distant organisms
            }
            
            // Pulse glow
            pulseTimes[i] += deltaTime * pulseSpeed;
            float pulse = (Mathf.Sin(pulseTimes[i]) + 1f) * 0.5f;
            organismLights[i].intensity = glowIntensity * (0.5f + pulse * 0.5f);
            
            // Wandering movement
            Vector3 currentPos = organismTransforms[i].position;
            if ((currentPos - targetPositions[i]).sqrMagnitude < 0.25f)
            {
                targetPositions[i] = currentPos + Random.insideUnitSphere * wanderRadius;
                targetPositions[i].y = Mathf.Clamp(targetPositions[i].y, maxDepth, minDepth);
            }
            
            organismTransforms[i].position = Vector3.MoveTowards(
                currentPos,
                targetPositions[i],
                swimSpeed * deltaTime
            );
        }
    }
}

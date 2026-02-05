using UnityEngine;

/// <summary>
/// Creates bioluminescent organisms that glow in deep water
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
    
    private GameObject[] organisms;
    private Vector3[] targetPositions;
    private float[] pulseTimes;
    
    void Start()
    {
        CreateBioluminescentOrganisms();
    }
    
    void CreateBioluminescentOrganisms()
    {
        GameObject container = new GameObject("BioluminescentLife");
        
        organisms = new GameObject[organismCount];
        targetPositions = new Vector3[organismCount];
        pulseTimes = new float[organismCount];
        
        for (int i = 0; i < organismCount; i++)
        {
            // Random position in deep water
            float x = Random.Range(-spawnRadius, spawnRadius);
            float y = Random.Range(minDepth, maxDepth);
            float z = Random.Range(-spawnRadius, spawnRadius);
            Vector3 pos = new Vector3(x, y, z);
            
            // Create organism (small glowing sphere)
            GameObject organism = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            organism.name = $"BioOrganism_{i}";
            organism.transform.SetParent(container.transform);
            organism.transform.position = pos;
            organism.transform.localScale = Vector3.one * Random.Range(0.1f, 0.3f);
            
            // Glowing material
            Renderer renderer = organism.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", glowColor * glowIntensity);
            mat.color = glowColor;
            renderer.material = mat;
            
            // Remove collider
            Collider collider = organism.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
            
            // Add point light
            Light light = organism.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = glowColor;
            light.intensity = glowIntensity;
            light.range = glowRange;
            light.shadows = LightShadows.None;
            
            organisms[i] = organism;
            targetPositions[i] = pos;
            pulseTimes[i] = Random.Range(0f, Mathf.PI * 2f);
        }
        
        Debug.Log($"Created {organismCount} bioluminescent organisms");
    }
    
    void Update()
    {
        if (organisms == null) return;
        
        for (int i = 0; i < organisms.Length; i++)
        {
            if (organisms[i] == null) continue;
            
            // Pulse glow
            pulseTimes[i] += Time.deltaTime * pulseSpeed;
            float pulse = (Mathf.Sin(pulseTimes[i]) + 1f) * 0.5f;
            
            Light light = organisms[i].GetComponent<Light>();
            if (light != null)
            {
                light.intensity = glowIntensity * (0.5f + pulse * 0.5f);
            }
            
            // Slow wandering movement
            if (Vector3.Distance(organisms[i].transform.position, targetPositions[i]) < 0.5f)
            {
                // Pick new target
                targetPositions[i] = organisms[i].transform.position + Random.insideUnitSphere * wanderRadius;
                targetPositions[i].y = Mathf.Clamp(targetPositions[i].y, maxDepth, minDepth);
            }
            
            // Move towards target
            organisms[i].transform.position = Vector3.MoveTowards(
                organisms[i].transform.position,
                targetPositions[i],
                swimSpeed * Time.deltaTime
            );
        }
    }
}

using UnityEngine;

/// <summary>
/// Advanced buoyancy system implementing Archimedes' principle (F = ρgV)
/// Calculates submerged volume and applies realistic water resistance
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class AdvancedBuoyancy : MonoBehaviour
{
    [Header("Buoyancy Settings")]
    [Tooltip("Density of water (kg/m³). Fresh water = 1000, Salt water = 1025")]
    public float waterDensity = 1000f;
    
    [Tooltip("Volume of the object in cubic meters")]
    public float objectVolume = 1f;
    
    [Tooltip("Should volume be calculated automatically from collider?")]
    public bool autoCalculateVolume = true;
    
    [Header("Drag & Resistance")]
    [Tooltip("Linear drag coefficient when submerged")]
    [Range(0f, 10f)] public float waterLinearDrag = 3f;
    
    [Tooltip("Angular drag coefficient when submerged")]
    [Range(0f, 10f)] public float waterAngularDrag = 2f;
    
    [Tooltip("Additional velocity-based drag (increases with speed)")]
    [Range(0f, 5f)] public float velocityDragMultiplier = 0.5f;
    
    [Header("Voxel Sampling")]
    [Tooltip("Number of sample points for volume calculation (higher = more accurate but slower)")]
    [Range(3, 20)] public int voxelResolution = 8;
    
    [Header("Water Reference")]
    public AdvancedWaterShader waterShader;
    public float waterSurfaceY = 0f;
    
    [Header("Effects")]
    public bool createSplashEffects = true;
    public GameObject splashPrefab;
    public GameObject bubblePrefab;
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    
    private Rigidbody rb;
    private Collider objectCollider;
    private float originalLinearDrag;
    private float originalAngularDrag;
    private bool wasSubmerged = false;
    private Vector3[] voxelPoints;
    private float submergedPercentage = 0f;
    
    // Physics constants
    private const float GRAVITY = 9.81f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();
        
        originalLinearDrag = rb.drag;
        originalAngularDrag = rb.angularDrag;
        
        if (autoCalculateVolume)
            CalculateVolumeFromCollider();
            
        GenerateVoxelPoints();
        
        if (waterShader == null)
            waterShader = FindAnyObjectByType<AdvancedWaterShader>();
    }
    
    void FixedUpdate()
    {
        ApplyBuoyancy();
        ApplyWaterResistance();
        CheckSplashEffects();
    }
    
    void CalculateVolumeFromCollider()
    {
        if (objectCollider == null) return;
        
        Bounds bounds = objectCollider.bounds;
        objectVolume = bounds.size.x * bounds.size.y * bounds.size.z;
        
        // Adjust for common collider types
        if (objectCollider is SphereCollider)
        {
            SphereCollider sphere = objectCollider as SphereCollider;
            float radius = sphere.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            objectVolume = (4f / 3f) * Mathf.PI * radius * radius * radius;
        }
        else if (objectCollider is CapsuleCollider)
        {
            CapsuleCollider capsule = objectCollider as CapsuleCollider;
            float radius = capsule.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
            float height = capsule.height * transform.lossyScale.y;
            float cylinderVolume = Mathf.PI * radius * radius * (height - 2 * radius);
            float sphereVolume = (4f / 3f) * Mathf.PI * radius * radius * radius;
            objectVolume = cylinderVolume + sphereVolume;
        }
    }
    
    void GenerateVoxelPoints()
    {
        if (objectCollider == null) return;
        
        Bounds bounds = objectCollider.bounds;
        voxelPoints = new Vector3[voxelResolution * voxelResolution * voxelResolution];
        
        int index = 0;
        for (int x = 0; x < voxelResolution; x++)
        {
            for (int y = 0; y < voxelResolution; y++)
            {
                for (int z = 0; z < voxelResolution; z++)
                {
                    float xPos = Mathf.Lerp(bounds.min.x, bounds.max.x, x / (float)(voxelResolution - 1));
                    float yPos = Mathf.Lerp(bounds.min.y, bounds.max.y, y / (float)(voxelResolution - 1));
                    float zPos = Mathf.Lerp(bounds.min.z, bounds.max.z, z / (float)(voxelResolution - 1));
                    
                    voxelPoints[index] = new Vector3(xPos, yPos, zPos);
                    index++;
                }
            }
        }
    }
    
    void ApplyBuoyancy()
    {
        if (rb == null) return;
        
        // Get water surface height
        float waterHeight = waterSurfaceY;
        if (waterShader != null)
            waterHeight = waterShader.GetWaterHeightAtPosition(transform.position);
        
        // Calculate submerged volume using voxel sampling
        int submergedVoxels = 0;
        Vector3 centerOfBuoyancy = Vector3.zero;
        
        foreach (Vector3 voxel in voxelPoints)
        {
            Vector3 worldVoxel = transform.TransformPoint(voxel - objectCollider.bounds.center);
            
            if (worldVoxel.y < waterHeight)
            {
                submergedVoxels++;
                centerOfBuoyancy += worldVoxel;
            }
        }
        
        if (submergedVoxels == 0)
        {
            submergedPercentage = 0f;
            return;
        }
        
        // Calculate submerged percentage
        submergedPercentage = (float)submergedVoxels / voxelPoints.Length;
        centerOfBuoyancy /= submergedVoxels;
        
        // Calculate buoyancy force: F = ρ * g * V_submerged
        float submergedVolume = objectVolume * submergedPercentage;
        float buoyancyForce = waterDensity * GRAVITY * submergedVolume;
        
        // Apply buoyancy force at center of buoyancy
        Vector3 buoyancyVector = Vector3.up * buoyancyForce;
        rb.AddForceAtPosition(buoyancyVector, centerOfBuoyancy, ForceMode.Force);
        
        // Apply additional stabilization torque for partially submerged objects
        if (submergedPercentage > 0.1f && submergedPercentage < 0.9f)
        {
            Vector3 stabilizationTorque = Vector3.Cross(transform.up, Vector3.up) * buoyancyForce * 0.5f;
            rb.AddTorque(stabilizationTorque, ForceMode.Force);
        }
    }
    
    void ApplyWaterResistance()
    {
        if (submergedPercentage <= 0f)
        {
            // Not submerged, restore original drag
            rb.drag = originalLinearDrag;
            rb.angularDrag = originalAngularDrag;
            return;
        }
        
        // Apply water drag based on submersion
        float dragMultiplier = Mathf.Lerp(1f, waterLinearDrag, submergedPercentage);
        rb.drag = originalLinearDrag * dragMultiplier;
        rb.angularDrag = originalAngularDrag * Mathf.Lerp(1f, waterAngularDrag, submergedPercentage);
        
        // Apply velocity-based drag (quadratic drag: F_drag = -k * v²)
        Vector3 velocityDrag = -rb.velocity.normalized * rb.velocity.sqrMagnitude * velocityDragMultiplier * submergedPercentage;
        rb.AddForce(velocityDrag, ForceMode.Force);
    }
    
    void CheckSplashEffects()
    {
        if (!createSplashEffects) return;
        
        bool isSubmerged = submergedPercentage > 0.1f;
        
        // Entering water
        if (isSubmerged && !wasSubmerged && rb.velocity.magnitude > 1f)
        {
            CreateSplashEffect();
        }
        
        // Exiting water
        if (!isSubmerged && wasSubmerged && rb.velocity.y > 0.5f)
        {
            CreateSplashEffect();
        }
        
        // Create bubbles when fully submerged and moving
        if (submergedPercentage > 0.9f && rb.velocity.magnitude > 2f)
        {
            if (Random.value < 0.1f) // 10% chance per frame
                CreateBubbleEffect();
        }
        
        wasSubmerged = isSubmerged;
    }
    
    void CreateSplashEffect()
    {
        if (splashPrefab == null) return;
        
        float waterHeight = waterSurfaceY;
        if (waterShader != null)
            waterHeight = waterShader.GetWaterHeightAtPosition(transform.position);
        
        Vector3 splashPosition = new Vector3(transform.position.x, waterHeight, transform.position.z);
        GameObject splash = Instantiate(splashPrefab, splashPosition, Quaternion.identity);
        Destroy(splash, 2f);
    }
    
    void CreateBubbleEffect()
    {
        if (bubblePrefab == null) return;
        
        Vector3 bubblePosition = transform.position + Random.insideUnitSphere * 0.5f;
        GameObject bubble = Instantiate(bubblePrefab, bubblePosition, Quaternion.identity);
        Destroy(bubble, 3f);
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || voxelPoints == null) return;
        
        float waterHeight = waterSurfaceY;
        if (waterShader != null && Application.isPlaying)
            waterHeight = waterShader.GetWaterHeightAtPosition(transform.position);
        
        // Draw voxel points
        foreach (Vector3 voxel in voxelPoints)
        {
            if (objectCollider == null) continue;
            
            Vector3 worldVoxel = transform.TransformPoint(voxel - objectCollider.bounds.center);
            
            if (worldVoxel.y < waterHeight)
                Gizmos.color = new Color(0f, 0.5f, 1f, 0.5f); // Submerged - blue
            else
                Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Above water - yellow
            
            Gizmos.DrawSphere(worldVoxel, 0.05f);
        }
        
        // Draw water surface plane
        Gizmos.color = new Color(0f, 0.7f, 1f, 0.2f);
        Gizmos.DrawCube(new Vector3(transform.position.x, waterHeight, transform.position.z), 
                        new Vector3(5f, 0.01f, 5f));
    }
    
    /// <summary>
    /// Get current submersion percentage (0 = fully above water, 1 = fully submerged)
    /// </summary>
    public float GetSubmergedPercentage()
    {
        return submergedPercentage;
    }
}

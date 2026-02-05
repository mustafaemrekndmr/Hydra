using UnityEngine;

/// <summary>
/// Advanced water shader controller with Gerstner waves, caustics, refraction, and depth fog
/// Implements realistic water surface dynamics and underwater rendering
/// </summary>
[ExecuteInEditMode]
public class AdvancedWaterShader : MonoBehaviour
{
    [Header("Water Surface")]
    public Material waterMaterial;
    public MeshFilter waterMeshFilter;
    
    [Header("Gerstner Wave Parameters")]
    [Range(0f, 2f)] public float waveAmplitude = 0.15f;
    [Range(0.1f, 5f)] public float waveFrequency = 1.2f;
    [Range(0f, 2f)] public float waveSpeed = 0.5f;
    [Range(2, 8)] public int waveCount = 4;
    
    [Header("Water Appearance")]
    public Color shallowWaterColor = new Color(0.3f, 0.7f, 0.8f, 0.8f);
    public Color deepWaterColor = new Color(0.05f, 0.2f, 0.4f, 0.95f);
    [Range(0f, 20f)] public float depthFalloff = 5f;
    [Range(0f, 1f)] public float transparency = 0.7f;
    
    [Header("Refraction & Reflection")]
    [Range(0f, 1f)] public float refractionStrength = 0.3f;
    [Range(0f, 1f)] public float reflectionStrength = 0.5f;
    public Cubemap reflectionCubemap;
    
    [Header("Caustics")]
    public Texture2D causticsTexture;
    [Range(0f, 2f)] public float causticsStrength = 0.8f;
    [Range(0.1f, 5f)] public float causticsScale = 2f;
    [Range(0f, 2f)] public float causticsSpeed = 0.3f;
    
    [Header("Specular & Fresnel")]
    [Range(0f, 1f)] public float smoothness = 0.9f;
    [Range(0f, 1f)] public float fresnelPower = 0.5f;
    
    [Header("Foam & Edge Effects")]
    [Range(0f, 5f)] public float foamDepth = 0.3f;
    public Color foamColor = Color.white;
    
    private Mesh waterMesh;
    private Vector3[] originalVertices;
    private Vector3[] displacedVertices;
    private Vector3[] normals;
    
    void Start()
    {
        InitializeWaterMesh();
        SetupWaterMaterial();
    }
    
    void Update()
    {
        UpdateWaterSurface();
        UpdateMaterialProperties();
    }
    
    void InitializeWaterMesh()
    {
        if (waterMeshFilter == null)
            waterMeshFilter = GetComponent<MeshFilter>();
            
        if (waterMeshFilter != null && waterMeshFilter.sharedMesh != null)
        {
            // Edit mode'da mesh leak'i önlemek için sharedMesh kullan
            // Runtime'da kendi mesh kopyamızı oluştur
            if (Application.isPlaying)
            {
                waterMesh = Instantiate(waterMeshFilter.sharedMesh);
                waterMeshFilter.mesh = waterMesh;
            }
            else
            {
                waterMesh = waterMeshFilter.sharedMesh;
            }
            
            originalVertices = waterMesh.vertices;
            displacedVertices = new Vector3[originalVertices.Length];
            normals = new Vector3[originalVertices.Length];
        }
    }
    
    void SetupWaterMaterial()
    {
        if (waterMaterial == null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
                waterMaterial = renderer.material;
        }
        
        if (waterMaterial != null)
        {
            // Enable necessary shader keywords
            waterMaterial.EnableKeyword("_NORMALMAP");
            waterMaterial.EnableKeyword("_EMISSION");
            
            if (reflectionCubemap != null)
                waterMaterial.SetTexture("_Cube", reflectionCubemap);
                
            if (causticsTexture != null)
                waterMaterial.SetTexture("_CausticsTex", causticsTexture);
        }
    }
    
    void UpdateWaterSurface()
    {
        // Edit mode'da mesh modifikasyonu yapma
        if (!Application.isPlaying) return;
        
        if (waterMesh == null || originalVertices == null) return;
        
        float time = Time.time;
        
        // Apply Gerstner waves
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];
            Vector3 displacement = Vector3.zero;
            Vector3 tangent = Vector3.zero;
            Vector3 binormal = Vector3.zero;
            
            // Multiple wave directions for realistic water
            for (int w = 0; w < waveCount; w++)
            {
                float angle = (Mathf.PI * 2f * w) / waveCount;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                
                float wavePhase = waveFrequency * (direction.x * vertex.x + direction.y * vertex.z) - waveSpeed * time;
                float waveSin = Mathf.Sin(wavePhase);
                float waveCos = Mathf.Cos(wavePhase);
                
                // Gerstner wave displacement
                displacement.x += direction.x * waveAmplitude * waveCos;
                displacement.y += waveAmplitude * waveSin;
                displacement.z += direction.y * waveAmplitude * waveCos;
                
                // Calculate tangent and binormal for normal calculation
                float wa = waveAmplitude * waveFrequency;
                tangent.x += -direction.x * direction.x * wa * waveSin;
                tangent.y += direction.x * wa * waveCos;
                tangent.z += -direction.x * direction.y * wa * waveSin;
                
                binormal.x += -direction.x * direction.y * wa * waveSin;
                binormal.y += direction.y * wa * waveCos;
                binormal.z += -direction.y * direction.y * wa * waveSin;
            }
            
            displacedVertices[i] = vertex + displacement / waveCount;
            
            // Calculate normal from tangent and binormal
            tangent = new Vector3(1 - tangent.x, tangent.y, -tangent.z);
            binormal = new Vector3(-binormal.x, binormal.y, 1 - binormal.z);
            normals[i] = Vector3.Cross(binormal, tangent).normalized;
        }
        
        waterMesh.vertices = displacedVertices;
        waterMesh.normals = normals;
        waterMesh.RecalculateBounds();
    }
    
    void UpdateMaterialProperties()
    {
        if (waterMaterial == null) return;
        
        // Update shader properties
        waterMaterial.SetColor("_ShallowColor", shallowWaterColor);
        waterMaterial.SetColor("_DeepColor", deepWaterColor);
        waterMaterial.SetFloat("_DepthFalloff", depthFalloff);
        waterMaterial.SetFloat("_Transparency", transparency);
        
        waterMaterial.SetFloat("_RefractionStrength", refractionStrength);
        waterMaterial.SetFloat("_ReflectionStrength", reflectionStrength);
        
        waterMaterial.SetFloat("_CausticsStrength", causticsStrength);
        waterMaterial.SetFloat("_CausticsScale", causticsScale);
        waterMaterial.SetFloat("_CausticsSpeed", causticsSpeed);
        waterMaterial.SetFloat("_Time", Time.time);
        
        waterMaterial.SetFloat("_Smoothness", smoothness);
        waterMaterial.SetFloat("_FresnelPower", fresnelPower);
        
        waterMaterial.SetFloat("_FoamDepth", foamDepth);
        waterMaterial.SetColor("_FoamColor", foamColor);
    }
    
    /// <summary>
    /// Get water height at world position (for buoyancy calculations)
    /// </summary>
    public float GetWaterHeightAtPosition(Vector3 worldPosition)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        float height = transform.position.y;
        
        float time = Time.time;
        
        // Calculate wave height at position
        for (int w = 0; w < waveCount; w++)
        {
            float angle = (Mathf.PI * 2f * w) / waveCount;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            
            float wavePhase = waveFrequency * (direction.x * localPos.x + direction.y * localPos.z) - waveSpeed * time;
            height += waveAmplitude * Mathf.Sin(wavePhase) / waveCount;
        }
        
        return height;
    }
    
    /// <summary>
    /// Get water normal at world position (for splash effects)
    /// </summary>
    public Vector3 GetWaterNormalAtPosition(Vector3 worldPosition)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        Vector3 normal = Vector3.up;
        
        float time = Time.time;
        Vector3 tangent = Vector3.zero;
        Vector3 binormal = Vector3.zero;
        
        for (int w = 0; w < waveCount; w++)
        {
            float angle = (Mathf.PI * 2f * w) / waveCount;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            
            float wavePhase = waveFrequency * (direction.x * localPos.x + direction.y * localPos.z) - waveSpeed * time;
            float waveSin = Mathf.Sin(wavePhase);
            float waveCos = Mathf.Cos(wavePhase);
            
            float wa = waveAmplitude * waveFrequency;
            tangent.x += -direction.x * direction.x * wa * waveSin;
            tangent.y += direction.x * wa * waveCos;
            tangent.z += -direction.x * direction.y * wa * waveSin;
            
            binormal.x += -direction.x * direction.y * wa * waveSin;
            binormal.y += direction.y * wa * waveCos;
            binormal.z += -direction.y * direction.y * wa * waveSin;
        }
        
        tangent = new Vector3(1 - tangent.x / waveCount, tangent.y / waveCount, -tangent.z / waveCount);
        binormal = new Vector3(-binormal.x / waveCount, binormal.y / waveCount, 1 - binormal.z / waveCount);
        normal = Vector3.Cross(binormal, tangent).normalized;
        
        return transform.TransformDirection(normal);
    }
}

using UnityEngine;

/// <summary>
/// Procedural tile shader for pool surfaces with wetness effects, specular highlights, and grout lines
/// Implements realistic ceramic tile appearance with dynamic wetness
/// </summary>
[ExecuteInEditMode]
public class ProceduralTileShader : MonoBehaviour
{
    [Header("Tile Pattern")]
    [Range(0.1f, 2f)] public float tileSize = 0.3f;
    [Range(0.001f, 0.05f)] public float groutWidth = 0.01f;
    public Color tileColor = new Color(0.85f, 0.9f, 0.95f);
    public Color groutColor = new Color(0.5f, 0.5f, 0.5f);
    
    [Header("Tile Variation")]
    [Range(0f, 0.3f)] public float colorVariation = 0.05f;
    [Range(0f, 1f)] public float roughnessVariation = 0.1f;
    public Texture2D tileNoiseTexture;
    
    [Header("Wetness Effect")]
    [Range(0f, 1f)] public float wetnessAmount = 0.3f;
    [Range(0f, 1f)] public float wetnessHeight = 0.5f; // Height threshold for wetness
    public Color wetTint = new Color(0.7f, 0.8f, 0.9f);
    [Range(0f, 1f)] public float wetSmoothness = 0.95f;
    
    [Header("Specular & Glaze")]
    [Range(0f, 1f)] public float glazeSmoothness = 0.85f;
    [Range(0f, 1f)] public float glazeMetallic = 0.1f;
    [Range(0f, 2f)] public float specularIntensity = 1.2f;
    
    [Header("Normal Mapping")]
    public Texture2D tileNormalMap;
    [Range(0f, 2f)] public float normalStrength = 0.5f;
    
    [Header("Water Proximity")]
    public Transform waterSurface;
    [Range(0f, 5f)] public float wetnessFalloffDistance = 1f;
    
    private Material tileMaterial;
    private Renderer tileRenderer;
    
    void Start()
    {
        SetupMaterial();
    }
    
    void Update()
    {
        UpdateMaterialProperties();
        UpdateWetnessFromWater();
    }
    
    void SetupMaterial()
    {
        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            // Edit mode'da material leak'i önlemek için sharedMaterial kullan
            if (Application.isPlaying)
            {
                // Play mode'da kendi material kopyamızı oluştur
                tileMaterial = new Material(tileRenderer.sharedMaterial);
                tileRenderer.material = tileMaterial;
            }
            else
            {
                // Edit mode'da sharedMaterial kullan
                tileMaterial = tileRenderer.sharedMaterial;
            }
            
            if (tileMaterial != null)
            {
                // Set shader to Standard with detail
                tileMaterial.shader = Shader.Find("Standard");
                
                // Enable necessary features
                tileMaterial.EnableKeyword("_NORMALMAP");
                tileMaterial.EnableKeyword("_DETAIL_MULX2");
            }
        }
    }
    
    void UpdateMaterialProperties()
    {
        // Edit mode'da material modifikasyonu yapma
        if (!Application.isPlaying) return;
        
        if (tileMaterial == null) return;
        
        // Base tile properties
        tileMaterial.SetColor("_Color", tileColor);
        tileMaterial.SetFloat("_Glossiness", Mathf.Lerp(glazeSmoothness, wetSmoothness, wetnessAmount));
        tileMaterial.SetFloat("_Metallic", glazeMetallic);
        
        // Tile pattern (using UV tiling)
        float tiling = 1f / tileSize;
        tileMaterial.mainTextureScale = new Vector2(tiling, tiling);
        
        // Normal map
        if (tileNormalMap != null)
        {
            tileMaterial.SetTexture("_BumpMap", tileNormalMap);
            tileMaterial.SetFloat("_BumpScale", normalStrength);
        }
        
        // Emission for wetness effect
        Color emissionColor = Color.Lerp(Color.black, wetTint * 0.2f, wetnessAmount);
        tileMaterial.SetColor("_EmissionColor", emissionColor);
        tileMaterial.EnableKeyword("_EMISSION");
    }
    
    void UpdateWetnessFromWater()
    {
        if (waterSurface == null) return;
        
        // Calculate distance to water surface
        float distanceToWater = Mathf.Abs(transform.position.y - waterSurface.position.y);
        
        // Update wetness based on proximity to water
        if (distanceToWater < wetnessFalloffDistance)
        {
            float proximityWetness = 1f - (distanceToWater / wetnessFalloffDistance);
            wetnessAmount = Mathf.Lerp(wetnessAmount, proximityWetness, Time.deltaTime * 2f);
        }
        else
        {
            // Dry out slowly when away from water
            wetnessAmount = Mathf.Lerp(wetnessAmount, 0f, Time.deltaTime * 0.5f);
        }
    }
    
    /// <summary>
    /// Apply wetness at specific world position (for splash effects)
    /// </summary>
    public void ApplyWetnessAtPosition(Vector3 worldPosition, float radius, float intensity)
    {
        float distance = Vector3.Distance(transform.position, worldPosition);
        if (distance < radius)
        {
            float wetness = intensity * (1f - distance / radius);
            wetnessAmount = Mathf.Clamp01(wetnessAmount + wetness);
        }
    }
    
    /// <summary>
    /// Generate procedural tile texture with grout lines
    /// </summary>
    public Texture2D GenerateTileTexture(int resolution = 512)
    {
        Texture2D texture = new Texture2D(resolution, resolution);
        
        float groutPixels = groutWidth * resolution / tileSize;
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float u = (float)x / resolution;
                float v = (float)y / resolution;
                
                // Check if we're in grout area
                float tileU = (u * resolution) % (resolution * tileSize);
                float tileV = (v * resolution) % (resolution * tileSize);
                
                bool isGrout = (tileU < groutPixels || tileV < groutPixels);
                
                Color pixelColor;
                if (isGrout)
                {
                    // Grout color with slight variation
                    float variation = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.1f;
                    pixelColor = groutColor * (1f + variation);
                }
                else
                {
                    // Tile color with variation
                    float variation = Mathf.PerlinNoise(x * 0.05f, y * 0.05f) * colorVariation;
                    pixelColor = tileColor * (1f + variation);
                }
                
                texture.SetPixel(x, y, pixelColor);
            }
        }
        
        texture.Apply();
        return texture;
    }
}

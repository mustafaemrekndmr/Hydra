using UnityEngine;

/// <summary>
/// Volumetric lighting system for underwater god rays and atmospheric effects
/// Simulates light scattering through water and air
/// </summary>
[RequireComponent(typeof(Light))]
public class VolumetricLighting : MonoBehaviour
{
    [Header("Volumetric Settings")]
    [Range(0f, 1f)] public float volumetricIntensity = 0.5f;
    [Range(8, 128)] public int sampleCount = 64;
    [Range(0.1f, 10f)] public float scatteringCoefficient = 1.5f;
    
    [Header("God Rays")]
    public bool enableGodRays = true;
    [Range(0f, 1f)] public float godRayIntensity = 0.7f;
    [Range(0.1f, 5f)] public float godRayDecay = 0.95f;
    [Range(0f, 2f)] public float godRayWeight = 0.8f;
    
    [Header("Underwater Scattering")]
    public bool isUnderwater = true;
    public Color underwaterScatterColor = new Color(0.2f, 0.5f, 0.7f);
    [Range(0f, 5f)] public float underwaterDensity = 2f;
    
    [Header("Caustics Integration")]
    public Texture2D causticsTexture;
    [Range(0f, 2f)] public float causticsInfluence = 0.5f;
    [Range(0.1f, 5f)] public float causticsScale = 2f;
    
    [Header("Performance")]
    public bool useHalfResolution = false;
    [Range(1, 4)] public int downSample = 1;
    
    private Light volumetricLight;
    private Material volumetricMaterial;
    private Camera mainCamera;
    private RenderTexture volumetricRT;
    
    void Start()
    {
        volumetricLight = GetComponent<Light>();
        mainCamera = Camera.main;
        
        SetupVolumetricMaterial();
        SetupRenderTexture();
    }
    
    void SetupVolumetricMaterial()
    {
        // Create material for volumetric rendering
        Shader volumetricShader = Shader.Find("Hidden/VolumetricLight");
        if (volumetricShader == null)
        {
            // Fallback to standard shader if custom shader not found
            volumetricShader = Shader.Find("Unlit/Texture");
        }
        
        volumetricMaterial = new Material(volumetricShader);
    }
    
    void SetupRenderTexture()
    {
        if (mainCamera == null) return;
        
        int width = mainCamera.pixelWidth / downSample;
        int height = mainCamera.pixelHeight / downSample;
        
        volumetricRT = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf);
        volumetricRT.filterMode = FilterMode.Bilinear;
        volumetricRT.wrapMode = TextureWrapMode.Clamp;
    }
    
    void Update()
    {
        UpdateVolumetricProperties();
    }
    
    void UpdateVolumetricProperties()
    {
        if (volumetricMaterial == null || volumetricLight == null) return;
        
        // Update material properties
        volumetricMaterial.SetFloat("_Intensity", volumetricIntensity);
        volumetricMaterial.SetInt("_SampleCount", sampleCount);
        volumetricMaterial.SetFloat("_Scattering", scatteringCoefficient);
        
        volumetricMaterial.SetFloat("_GodRayIntensity", godRayIntensity);
        volumetricMaterial.SetFloat("_GodRayDecay", godRayDecay);
        volumetricMaterial.SetFloat("_GodRayWeight", godRayWeight);
        
        if (isUnderwater)
        {
            volumetricMaterial.SetColor("_ScatterColor", underwaterScatterColor);
            volumetricMaterial.SetFloat("_Density", underwaterDensity);
        }
        
        if (causticsTexture != null)
        {
            volumetricMaterial.SetTexture("_CausticsTex", causticsTexture);
            volumetricMaterial.SetFloat("_CausticsInfluence", causticsInfluence);
            volumetricMaterial.SetFloat("_CausticsScale", causticsScale);
        }
        
        // Light properties
        Vector3 lightDir = transform.forward;
        volumetricMaterial.SetVector("_LightDir", new Vector4(lightDir.x, lightDir.y, lightDir.z, 0));
        volumetricMaterial.SetColor("_LightColor", volumetricLight.color * volumetricLight.intensity);
    }
    
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (volumetricMaterial == null || !enableGodRays)
        {
            Graphics.Blit(source, destination);
            return;
        }
        
        // Render volumetric lighting
        RenderTexture temp = RenderTexture.GetTemporary(source.width / downSample, 
                                                         source.height / downSample, 
                                                         0, 
                                                         RenderTextureFormat.ARGBHalf);
        
        // Pass 1: Render volumetric light
        Graphics.Blit(source, temp, volumetricMaterial, 0);
        
        // Pass 2: Blur for softer god rays
        RenderTexture temp2 = RenderTexture.GetTemporary(temp.width, temp.height, 0, temp.format);
        Graphics.Blit(temp, temp2, volumetricMaterial, 1);
        
        // Pass 3: Composite with original image
        volumetricMaterial.SetTexture("_VolumetricTex", temp2);
        Graphics.Blit(source, destination, volumetricMaterial, 2);
        
        RenderTexture.ReleaseTemporary(temp);
        RenderTexture.ReleaseTemporary(temp2);
    }
    
    void OnDestroy()
    {
        if (volumetricRT != null)
            volumetricRT.Release();
            
        if (volumetricMaterial != null)
            DestroyImmediate(volumetricMaterial);
    }
    
    void OnDrawGizmos()
    {
        if (volumetricLight == null) return;
        
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        
        if (volumetricLight.type == LightType.Directional)
        {
            Gizmos.DrawRay(transform.position, transform.forward * 10f);
        }
        else if (volumetricLight.type == LightType.Spot)
        {
            float range = volumetricLight.range;
            float angle = volumetricLight.spotAngle;
            
            Vector3 forward = transform.forward * range;
            Vector3 right = transform.right * Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad) * range;
            Vector3 up = transform.up * Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad) * range;
            
            Gizmos.DrawLine(transform.position, transform.position + forward + right + up);
            Gizmos.DrawLine(transform.position, transform.position + forward + right - up);
            Gizmos.DrawLine(transform.position, transform.position + forward - right + up);
            Gizmos.DrawLine(transform.position, transform.position + forward - right - up);
        }
    }
}

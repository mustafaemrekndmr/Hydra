using UnityEngine;

/// <summary>
/// Creates god rays (light shafts) from surface for atmospheric underwater effect
/// </summary>
public class GodRaysEffect : MonoBehaviour
{
    [Header("God Rays Settings")]
    public int rayCount = 8;
    public float rayLength = 50f;
    public float rayWidth = 2f;
    public Color rayColor = new Color(0.4f, 0.6f, 0.8f, 0.1f);
    public float animationSpeed = 0.5f;
    
    [Header("Positioning")]
    public float surfaceHeight = 0f;
    public float spreadRadius = 30f;
    
    private GameObject[] rays;
    private float animationTime;
    
    void Start()
    {
        CreateGodRays();
    }
    
    void CreateGodRays()
    {
        GameObject raysContainer = new GameObject("GodRays");
        raysContainer.transform.position = new Vector3(0, surfaceHeight, 0);
        
        rays = new GameObject[rayCount];
        
        for (int i = 0; i < rayCount; i++)
        {
            // Create ray
            GameObject ray = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ray.name = $"GodRay_{i}";
            ray.transform.SetParent(raysContainer.transform);
            
            // Position in circle pattern
            float angle = (360f / rayCount) * i;
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * spreadRadius;
            float z = Mathf.Sin(angle * Mathf.Deg2Rad) * spreadRadius;
            
            ray.transform.position = new Vector3(x, surfaceHeight, z);
            ray.transform.rotation = Quaternion.Euler(0, 0, 0);
            ray.transform.localScale = new Vector3(rayWidth, rayLength / 2f, rayWidth);
            
            // Move down
            ray.transform.position = new Vector3(x, surfaceHeight - rayLength / 2f, z);
            
            // Material
            Renderer renderer = ray.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            mat.color = rayColor;
            mat.SetFloat("_Glossiness", 0f);
            renderer.material = mat;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            
            // Remove collider
            Collider collider = ray.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
            
            rays[i] = ray;
        }
        
        Debug.Log($"Created {rayCount} god rays");
    }
    
    void Update()
    {
        if (rays == null || rays.Length == 0) return;
        
        animationTime += Time.deltaTime * animationSpeed;
        
        // Animate rays (subtle movement)
        for (int i = 0; i < rays.Length; i++)
        {
            if (rays[i] == null) continue;
            
            float offset = Mathf.Sin(animationTime + i * 0.5f) * 0.5f;
            Vector3 pos = rays[i].transform.position;
            rays[i].transform.position = new Vector3(pos.x, pos.y + offset * Time.deltaTime, pos.z);
            
            // Rotate slightly
            rays[i].transform.Rotate(Vector3.up, Time.deltaTime * 2f);
        }
    }
}

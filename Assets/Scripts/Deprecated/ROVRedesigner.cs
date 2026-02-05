using UnityEngine;

/// <summary>
/// Rebuilds ROV structure with realistic thruster placement and lighting
/// </summary>
public class ROVRedesigner : MonoBehaviour
{
    [Header("ROV Dimensions")]
    public float hullLength = 1.5f;
    public float hullWidth = 0.8f;
    public float hullHeight = 0.6f;
    
    [Header("Thruster Settings")]
    public float thrusterSize = 0.15f;
    public float thrusterOffset = 0.1f;
    
    [Header("Lighting")]
    public bool addSpotlights = true;
    public float spotlightIntensity = 3f;
    public float spotlightRange = 20f;
    public Color spotlightColor = new Color(1f, 0.95f, 0.8f);
    
    [ContextMenu("Redesign ROV")]
    public void RedesignROV()
    {
        // Find ROV components
        Transform hull = transform.Find("Hull");
        if (hull == null)
        {
            Debug.LogError("Hull not found!");
            return;
        }
        
        // Reposition thrusterlar - daha gerçekçi ROV düzeni
        RepositionThruster(hull, "ThrusterFL", new Vector3(-hullWidth/2 - thrusterOffset, 0.2f, hullLength/2 - 0.2f), new Vector3(0, 0, 90));
        RepositionThruster(hull, "ThrusterFR", new Vector3(hullWidth/2 + thrusterOffset, 0.2f, hullLength/2 - 0.2f), new Vector3(0, 0, 90));
        RepositionThruster(hull, "ThrusterBL", new Vector3(-hullWidth/2 - thrusterOffset, 0.2f, -hullLength/2 + 0.2f), new Vector3(0, 0, 90));
        RepositionThruster(hull, "ThrusterBR", new Vector3(hullWidth/2 + thrusterOffset, 0.2f, -hullLength/2 + 0.2f), new Vector3(0, 0, 90));
        
        // Vertical thrusterlar - üst ve alt
        RepositionThruster(hull, "ThrusterTop", new Vector3(0, hullHeight/2 + thrusterOffset, 0.3f), new Vector3(0, 0, 0));
        RepositionThruster(hull, "ThrusterBottom", new Vector3(0, -hullHeight/2 - thrusterOffset, 0.3f), new Vector3(180, 0, 0));
        
        // Scale thrusterları
        ScaleThruster(hull, "ThrusterFL", new Vector3(thrusterSize, thrusterSize * 2, thrusterSize));
        ScaleThruster(hull, "ThrusterFR", new Vector3(thrusterSize, thrusterSize * 2, thrusterSize));
        ScaleThruster(hull, "ThrusterBL", new Vector3(thrusterSize, thrusterSize * 2, thrusterSize));
        ScaleThruster(hull, "ThrusterBR", new Vector3(thrusterSize, thrusterSize * 2, thrusterSize));
        ScaleThruster(hull, "ThrusterTop", new Vector3(thrusterSize, thrusterSize * 2, thrusterSize));
        ScaleThruster(hull, "ThrusterBottom", new Vector3(thrusterSize, thrusterSize * 2, thrusterSize));
        
        // Add spotlights if enabled
        if (addSpotlights)
        {
            AddROVLights(hull);
        }
        
        Debug.Log("ROV redesigned successfully!");
    }
    
    void RepositionThruster(Transform hull, string thrusterName, Vector3 localPos, Vector3 localRot)
    {
        Transform thruster = hull.Find(thrusterName);
        if (thruster != null)
        {
            thruster.localPosition = localPos;
            thruster.localEulerAngles = localRot;
        }
        else
        {
            Debug.LogWarning($"Thruster {thrusterName} not found!");
        }
    }
    
    void ScaleThruster(Transform hull, string thrusterName, Vector3 scale)
    {
        Transform thruster = hull.Find(thrusterName);
        if (thruster != null)
        {
            thruster.localScale = scale;
        }
    }
    
    void AddROVLights(Transform hull)
    {
        // Remove existing lights first
        Light[] existingLights = hull.GetComponentsInChildren<Light>();
        foreach (Light light in existingLights)
        {
            if (light.gameObject.name.Contains("ROVLight"))
            {
                DestroyImmediate(light.gameObject);
            }
        }
        
        // Add two front spotlights
        CreateSpotlight(hull, "ROVLight_Left", new Vector3(-0.3f, 0.1f, 0.8f), new Vector3(10, -5, 0));
        CreateSpotlight(hull, "ROVLight_Right", new Vector3(0.3f, 0.1f, 0.8f), new Vector3(10, 5, 0));
        
        Debug.Log("Added ROV spotlights");
    }
    
    void CreateSpotlight(Transform parent, string name, Vector3 localPos, Vector3 localRot)
    {
        GameObject lightObj = new GameObject(name);
        lightObj.transform.SetParent(parent);
        lightObj.transform.localPosition = localPos;
        lightObj.transform.localEulerAngles = localRot;
        
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = spotlightColor;
        light.intensity = spotlightIntensity;
        light.range = spotlightRange;
        light.spotAngle = 60f;
        light.innerSpotAngle = 30f;
        light.shadows = LightShadows.None; // Performance
        
        // Add a visual cone (optional)
        GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cone.name = "LightCone";
        cone.transform.SetParent(lightObj.transform);
        cone.transform.localPosition = new Vector3(0, 0, 0.3f);
        cone.transform.localEulerAngles = new Vector3(90, 0, 0);
        cone.transform.localScale = new Vector3(0.1f, 0.15f, 0.1f);
        
        // Make cone semi-transparent yellow
        Renderer coneRenderer = cone.GetComponent<Renderer>();
        Material coneMat = new Material(Shader.Find("Standard"));
        coneMat.color = new Color(1f, 0.9f, 0.3f, 0.5f);
        coneMat.SetFloat("_Mode", 3); // Transparent
        coneMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        coneMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        coneMat.SetInt("_ZWrite", 0);
        coneMat.DisableKeyword("_ALPHATEST_ON");
        coneMat.EnableKeyword("_ALPHABLEND_ON");
        coneMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        coneMat.renderQueue = 3000;
        coneRenderer.material = coneMat;
        
        // Remove collider
        Collider coneCollider = cone.GetComponent<Collider>();
        if (coneCollider != null)
            DestroyImmediate(coneCollider);
    }
}

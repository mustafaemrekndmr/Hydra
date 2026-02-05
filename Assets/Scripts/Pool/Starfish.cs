using UnityEngine;

public class Starfish : MonoBehaviour
{
    [Header("Starfish Settings")]
    public int armCount = 5;
    public float armLength = 0.3f;
    public float armWidth = 0.1f;
    public float bodyRadius = 0.15f;
    
    [Header("Materials")]
    public Material starfishMaterial;
    
    public void Generate()
    {
        // Create central body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.transform.parent = transform;
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(bodyRadius * 2f, bodyRadius * 0.5f, bodyRadius * 2f);
        
        ApplyStarfishMaterial(body);
        
        // Create arms
        float angleStep = 360f / armCount;
        
        for (int i = 0; i < armCount; i++)
        {
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            arm.transform.parent = transform;
            
            float angle = i * angleStep;
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            
            arm.transform.localPosition = direction * bodyRadius;
            arm.transform.localRotation = Quaternion.Euler(90f, angle, 0f);
            arm.transform.localScale = new Vector3(armWidth, armLength * 0.5f, armWidth);
            
            ApplyStarfishMaterial(arm);
            
            // Add tip to arm
            GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tip.transform.parent = arm.transform;
            tip.transform.localPosition = Vector3.up * armLength * 0.8f;
            tip.transform.localScale = Vector3.one * 0.6f;
            
            ApplyStarfishMaterial(tip);
        }
        
        // Flatten to lay on ground
        transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    }
    
    void ApplyStarfishMaterial(GameObject obj)
    {
        if (starfishMaterial != null)
        {
            obj.GetComponent<Renderer>().material = starfishMaterial;
        }
        else
        {
            // Starfish colors (orange, red, purple)
            Color[] starfishColors = new Color[]
            {
                new Color(1f, 0.5f, 0.2f),    // Orange
                new Color(0.9f, 0.3f, 0.2f),  // Red-orange
                new Color(0.7f, 0.3f, 0.6f)   // Purple
            };
            
            Material mat = obj.GetComponent<Renderer>().material;
            mat.color = starfishColors[Random.Range(0, starfishColors.Length)];
        }
    }
    
    void Start()
    {
        if (transform.childCount == 0)
            Generate();
    }
}

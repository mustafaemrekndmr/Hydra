using UnityEngine;

public class ProceduralRock : MonoBehaviour
{
    [Header("Rock Settings")]
    public int subdivisions = 2;
    public float noiseStrength = 0.3f;
    public float minScale = 0.5f;
    public float maxScale = 2f;
    
    [Header("Materials")]
    public Material rockMaterial;
    
    public void Generate()
    {
        // Create base rock from sphere
        GameObject rockObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rockObj.transform.parent = transform;
        rockObj.transform.localPosition = Vector3.zero;
        
        // Random scale
        float scale = Random.Range(minScale, maxScale);
        rockObj.transform.localScale = Vector3.one * scale;
        
        // Random rotation
        rockObj.transform.localRotation = Random.rotation;
        
        // Apply material
        if (rockMaterial != null)
        {
            rockObj.GetComponent<Renderer>().material = rockMaterial;
        }
        else
        {
            // Default gray rocky color
            Material mat = rockObj.GetComponent<Renderer>().material;
            mat.color = new Color(0.4f, 0.4f, 0.45f);
        }
        
        // Add some sub-rocks for cluster effect
        int subRocks = Random.Range(2, 5);
        for (int i = 0; i < subRocks; i++)
        {
            GameObject subRock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            subRock.transform.parent = rockObj.transform;
            
            Vector3 offset = Random.insideUnitSphere * 0.6f;
            subRock.transform.localPosition = offset;
            subRock.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);
            subRock.transform.localRotation = Random.rotation;
            
            if (rockMaterial != null)
                subRock.GetComponent<Renderer>().material = rockMaterial;
            else
                subRock.GetComponent<Renderer>().material.color = new Color(0.35f, 0.35f, 0.4f);
        }
    }
    
    void Start()
    {
        if (transform.childCount == 0)
            Generate();
    }
}

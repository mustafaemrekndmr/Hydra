using UnityEngine;

public class SeaweedPlant : MonoBehaviour
{
    [Header("Seaweed Settings")]
    public int segmentCount = 8;
    public float segmentHeight = 0.3f;
    public float segmentWidth = 0.15f;
    public float swayAmount = 0.2f;
    public float swaySpeed = 1f;
    
    [Header("Materials")]
    public Material seaweedMaterial;
    
    private GameObject[] segments;
    private Vector3[] originalPositions;
    private float swayTime;
    
    public void Generate()
    {
        segments = new GameObject[segmentCount];
        originalPositions = new Vector3[segmentCount];
        
        for (int i = 0; i < segmentCount; i++)
        {
            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            segment.transform.parent = transform;
            
            float yPos = i * segmentHeight;
            segment.transform.localPosition = new Vector3(0f, yPos, 0f);
            segment.transform.localScale = new Vector3(segmentWidth, segmentHeight * 0.5f, segmentWidth);
            
            originalPositions[i] = segment.transform.localPosition;
            segments[i] = segment;
            
            ApplySeaweedMaterial(segment);
            
            // Remove collider for performance
            Destroy(segment.GetComponent<Collider>());
        }
        
        swayTime = Random.Range(0f, 100f); // Random start time for variation
    }
    
    void ApplySeaweedMaterial(GameObject obj)
    {
        if (seaweedMaterial != null)
        {
            obj.GetComponent<Renderer>().material = seaweedMaterial;
        }
        else
        {
            // Seaweed green/brown colors
            Color[] seaweedColors = new Color[]
            {
                new Color(0.2f, 0.6f, 0.3f),  // Green
                new Color(0.3f, 0.7f, 0.4f),  // Light green
                new Color(0.4f, 0.5f, 0.2f)   // Yellow-green
            };
            
            Material mat = obj.GetComponent<Renderer>().material;
            mat.color = seaweedColors[Random.Range(0, seaweedColors.Length)];
        }
    }
    
    void Start()
    {
        if (transform.childCount == 0)
            Generate();
    }
    
    void Update()
    {
        if (segments == null || segments.Length == 0) return;
        
        // Animate swaying
        swayTime += Time.deltaTime * swaySpeed;
        
        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null) continue;
            
            // Increase sway amount towards the top
            float swayFactor = (float)i / segments.Length;
            
            // Use sine wave for smooth swaying
            float swayX = Mathf.Sin(swayTime + i * 0.3f) * swayAmount * swayFactor;
            float swayZ = Mathf.Cos(swayTime * 0.7f + i * 0.2f) * swayAmount * swayFactor * 0.5f;
            
            Vector3 swayOffset = new Vector3(swayX, 0f, swayZ);
            segments[i].transform.localPosition = originalPositions[i] + swayOffset;
            
            // Slight rotation for more natural movement
            float rotationZ = swayX * 20f;
            segments[i].transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
        }
    }
}

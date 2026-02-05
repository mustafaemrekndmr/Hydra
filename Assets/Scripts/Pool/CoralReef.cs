using UnityEngine;

public class CoralReef : MonoBehaviour
{
    [Header("Coral Settings")]
    public int branchCount = 8;
    public float branchLength = 1f;
    public float branchThickness = 0.1f;
    public CoralType coralType = CoralType.Branching;
    
    [Header("Materials")]
    public Material coralMaterial;
    
    public enum CoralType
    {
        Branching,
        BrainCoral,
        FanCoral
    }
    
    public void Generate()
    {
        switch (coralType)
        {
            case CoralType.Branching:
                GenerateBranchingCoral();
                break;
            case CoralType.BrainCoral:
                GenerateBrainCoral();
                break;
            case CoralType.FanCoral:
                GenerateFanCoral();
                break;
        }
    }
    
    void GenerateBranchingCoral()
    {
        for (int i = 0; i < branchCount; i++)
        {
            GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            branch.transform.parent = transform;
            
            // Random upward direction
            Vector3 direction = Random.onUnitSphere;
            if (direction.y < 0) direction.y = -direction.y; // Force upward
            
            branch.transform.localPosition = Vector3.zero;
            branch.transform.up = direction;
            branch.transform.localScale = new Vector3(branchThickness, branchLength * Random.Range(0.7f, 1.3f), branchThickness);
            branch.transform.localPosition += direction * branchLength * 0.5f;
            
            ApplyCoralMaterial(branch);
            
            // Add sub-branches
            if (Random.value > 0.5f)
            {
                GameObject subBranch = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                subBranch.transform.parent = branch.transform;
                
                Vector3 subDir = Random.onUnitSphere;
                if (subDir.y < 0) subDir.y = -subDir.y;
                
                subBranch.transform.localPosition = Vector3.up * 0.5f;
                subBranch.transform.up = subDir;
                subBranch.transform.localScale = new Vector3(branchThickness * 0.7f, branchLength * 0.5f, branchThickness * 0.7f);
                
                ApplyCoralMaterial(subBranch);
            }
        }
    }
    
    void GenerateBrainCoral()
    {
        GameObject brain = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        brain.transform.parent = transform;
        brain.transform.localPosition = Vector3.zero;
        brain.transform.localScale = Vector3.one * Random.Range(0.8f, 1.5f);
        
        ApplyCoralMaterial(brain);
        
        // Add bumps for brain-like texture
        for (int i = 0; i < 5; i++)
        {
            GameObject bump = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bump.transform.parent = brain.transform;
            bump.transform.localPosition = Random.onUnitSphere * 0.4f;
            bump.transform.localScale = Vector3.one * Random.Range(0.2f, 0.4f);
            
            ApplyCoralMaterial(bump);
        }
    }
    
    void GenerateFanCoral()
    {
        // Create fan shape using multiple thin cylinders
        int fanSegments = 8;
        float fanAngle = 120f;
        
        for (int i = 0; i < fanSegments; i++)
        {
            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.transform.parent = transform;
            
            float angle = (i / (float)fanSegments) * fanAngle - (fanAngle / 2f);
            segment.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            segment.transform.localPosition = Vector3.zero;
            segment.transform.localScale = new Vector3(0.05f, branchLength, branchThickness * 2f);
            segment.transform.localPosition += segment.transform.up * branchLength * 0.5f;
            
            ApplyCoralMaterial(segment);
        }
    }
    
    void ApplyCoralMaterial(GameObject obj)
    {
        if (coralMaterial != null)
        {
            obj.GetComponent<Renderer>().material = coralMaterial;
        }
        else
        {
            // Random coral colors
            Color[] coralColors = new Color[]
            {
                new Color(1f, 0.3f, 0.3f),    // Red
                new Color(1f, 0.5f, 0.2f),    // Orange
                new Color(0.8f, 0.2f, 0.8f),  // Purple
                new Color(1f, 0.7f, 0.8f)     // Pink
            };
            
            Material mat = obj.GetComponent<Renderer>().material;
            mat.color = coralColors[Random.Range(0, coralColors.Length)];
        }
    }
    
    void Start()
    {
        if (transform.childCount == 0)
            Generate();
    }
}

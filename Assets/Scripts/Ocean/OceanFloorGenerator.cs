using UnityEngine;

/// <summary>
/// Creates a realistic ocean floor with rocks, sand, and underwater terrain
/// </summary>
public class OceanFloorGenerator : MonoBehaviour
{
    [Header("Ocean Floor Settings")]
    public float floorDepth = -10f;
    public float floorSize = 100f;
    public Color sandColor = new Color(0.4f, 0.35f, 0.25f);
    
    [Header("Rocks")]
    public int rockCount = 30;
    public float minRockSize = 0.5f;
    public float maxRockSize = 3f;
    public Color rockColor = new Color(0.3f, 0.3f, 0.35f);
    
    [Header("Terrain Variation")]
    public float terrainNoiseScale = 0.1f;
    public float terrainHeight = 2f;
    
    void Start()
    {
        CreateOceanFloor();
    }
    
    void CreateOceanFloor()
    {
        GameObject floorContainer = new GameObject("OceanFloor");
        
        // Create main floor plane
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "SandFloor";
        floor.transform.SetParent(floorContainer.transform);
        floor.transform.position = new Vector3(0, floorDepth, 0);
        floor.transform.localScale = new Vector3(floorSize / 10f, 1, floorSize / 10f);
        
        // Floor material
        Renderer floorRenderer = floor.GetComponent<Renderer>();
        Material floorMat = new Material(Shader.Find("Standard"));
        floorMat.color = sandColor;
        floorMat.SetFloat("_Glossiness", 0.1f);
        floorMat.SetFloat("_Metallic", 0f);
        floorRenderer.material = floorMat;
        
        // Add rocks
        CreateRocks(floorContainer.transform);
        
        Debug.Log($"Ocean floor created at depth {floorDepth}m with {rockCount} rocks");
    }
    
    void CreateRocks(Transform parent)
    {
        GameObject rocksContainer = new GameObject("Rocks");
        rocksContainer.transform.SetParent(parent);
        
        for (int i = 0; i < rockCount; i++)
        {
            // Random position on floor
            float x = Random.Range(-floorSize / 2f, floorSize / 2f);
            float z = Random.Range(-floorSize / 2f, floorSize / 2f);
            float y = floorDepth + Random.Range(0f, terrainHeight);
            
            // Create rock
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = $"Rock_{i}";
            rock.transform.SetParent(rocksContainer.transform);
            rock.transform.position = new Vector3(x, y, z);
            
            // Random size and deformation
            float size = Random.Range(minRockSize, maxRockSize);
            float scaleX = size * Random.Range(0.7f, 1.3f);
            float scaleY = size * Random.Range(0.5f, 1.0f);
            float scaleZ = size * Random.Range(0.7f, 1.3f);
            rock.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
            
            // Random rotation
            rock.transform.rotation = Random.rotation;
            
            // Rock material
            Renderer rockRenderer = rock.GetComponent<Renderer>();
            Material rockMat = new Material(Shader.Find("Standard"));
            rockMat.color = rockColor * Random.Range(0.8f, 1.2f);
            rockMat.SetFloat("_Glossiness", Random.Range(0.1f, 0.3f));
            rockMat.SetFloat("_Metallic", 0f);
            rockRenderer.material = rockMat;
        }
    }
}

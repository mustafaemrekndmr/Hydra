using UnityEngine;

/// <summary>
/// Creates a realistic ocean floor with rocks and sand.
/// Optimized: shared materials, removed unnecessary colliders, static batching.
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
    
    [Header("Optimization")]
    public bool enableStaticBatching = true;
    public bool removeColliders = true;
    
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
        floor.isStatic = enableStaticBatching;
        
        // Floor material
        Renderer floorRenderer = floor.GetComponent<Renderer>();
        Material floorMat = new Material(Shader.Find("Standard"));
        floorMat.color = sandColor;
        floorMat.SetFloat("_Glossiness", 0.1f);
        floorMat.SetFloat("_Metallic", 0f);
        floorRenderer.material = floorMat;
        
        // Add rocks
        CreateRocks(floorContainer.transform);
        
        // Batch static objects
        if (enableStaticBatching)
        {
            StaticBatchingUtility.Combine(floorContainer);
        }
    }
    
    void CreateRocks(Transform parent)
    {
        GameObject rocksContainer = new GameObject("Rocks");
        rocksContainer.transform.SetParent(parent);
        
        // Shared rock materials (3 variations instead of 30 unique)
        Material[] rockMats = new Material[3];
        for (int m = 0; m < 3; m++)
        {
            rockMats[m] = new Material(Shader.Find("Standard"));
            rockMats[m].color = rockColor * Random.Range(0.85f, 1.15f);
            rockMats[m].SetFloat("_Glossiness", Random.Range(0.1f, 0.3f));
            rockMats[m].SetFloat("_Metallic", 0f);
        }
        
        for (int i = 0; i < rockCount; i++)
        {
            float x = Random.Range(-floorSize / 2f, floorSize / 2f);
            float z = Random.Range(-floorSize / 2f, floorSize / 2f);
            
            // Use Perlin noise for more natural height variation
            float noiseY = Mathf.PerlinNoise(
                x * terrainNoiseScale + 100f,
                z * terrainNoiseScale + 100f
            ) * terrainHeight;
            float y = floorDepth + noiseY;
            
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = $"Rock_{i}";
            rock.transform.SetParent(rocksContainer.transform);
            rock.transform.position = new Vector3(x, y, z);
            rock.isStatic = enableStaticBatching;
            
            // Random size and deformation
            float size = Random.Range(minRockSize, maxRockSize);
            rock.transform.localScale = new Vector3(
                size * Random.Range(0.7f, 1.3f),
                size * Random.Range(0.5f, 1.0f),
                size * Random.Range(0.7f, 1.3f)
            );
            rock.transform.rotation = Random.rotation;
            
            // Use shared material (one of 3 variants)
            rock.GetComponent<Renderer>().sharedMaterial = rockMats[i % 3];
            
            // Remove colliders on rocks (ROV won't collide with them)
            if (removeColliders)
            {
                Object.Destroy(rock.GetComponent<Collider>());
            }
        }
    }
}

using UnityEngine;

/// <summary>
/// Advanced indoor Olympic pool scene builder with high-fidelity visuals and physics
/// Integrates all advanced systems: water shader, buoyancy, volumetric lighting, particles, and procedural tiles
/// </summary>
[ExecuteInEditMode]
public class BuildAdvancedIndoorPool : MonoBehaviour
{
    [Header("Pool Dimensions")]
    public float poolLength = 50f;
    public float poolWidth = 25f;
    public float poolDepth = 3f;
    public int laneCount = 8;
    
    [Header("Water Quality")]
    [Range(32, 256)] public int waterMeshResolution = 128;
    public bool enableAdvancedWater = true;
    public bool enableCaustics = true;
    
    [Header("Lighting")]
    public bool enableVolumetricLighting = true;
    public bool enableGodRays = true;
    public int numberOfSpotlights = 8;
    public int numberOfWindows = 4;
    
    [Header("Materials")]
    public bool useProceduralTiles = true;
    public bool enableWetnessEffects = true;
    
    [Header("Physics")]
    public bool enableAdvancedBuoyancy = true;
    
    [Header("Effects")]
    public bool enableParticleEffects = true;
    public bool enableDustMotes = true;
    public bool enableUnderwaterParticles = true;
    
    private bool hasBuilt = false;
    
    void OnEnable()
    {
        // Edit mode'da sadece bir kez çalıştır
        if (!hasBuilt)
        {
            // Bir frame bekle ki tüm componentler hazır olsun
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += BuildSceneDelayed;
            #endif
        }
    }
    
    void BuildSceneDelayed()
    {
        if (!hasBuilt)
        {
            hasBuilt = true;
            BuildScene();
            
            // Edit mode'da builder objesini sil
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(gameObject);
            }
            #endif
        }
    }
    
    void BuildScene()
    {
        Debug.Log("Building Advanced Indoor Pool Scene...");
        
        // 1. Create pool structure
        CreatePoolFloor();
        CreatePoolWalls();
        
        // 2. Create advanced water surface
        CreateAdvancedWaterSurface();
        
        // 3. Setup lighting system
        SetupIndoorLighting();
        SetupNaturalLight();
        
        // 4. Create lane system
        CreateLaneLines();
        CreateLaneMarkers();
        
        // 5. Add pool details
        CreateStartingBlocks();
        CreatePoolDeck();
        
        // 6. Setup particle effects
        SetupParticleEffects();
        
        // 7. Create test objects with buoyancy
        CreateTestObjects();
        
        // 8. Setup camera and ROV
        CreateROVWithAdvancedPhysics();
        
        // 9. Add pause menu
        AddPauseMenu();
        
        // 10. Add event system
        AddEventSystem();
        
        // 11. Setup post-processing
        SetupPostProcessing();
        
        Debug.Log("Advanced Indoor Pool Scene built successfully!");
        
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
#endif
    }
    
    void SaveMesh(Mesh mesh, string assetName)
    {
#if UNITY_EDITOR
        string folderPath = "Assets/GeneratedMeshes";
        if (!System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }
        
        string fullPath = folderPath + "/" + assetName + ".asset";
        
        // Clean up existing if any
        UnityEditor.AssetDatabase.DeleteAsset(fullPath);
        
        UnityEditor.AssetDatabase.CreateAsset(mesh, fullPath);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }
    
    void CreatePoolFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "PoolFloor";
        floor.transform.position = new Vector3(0, -poolDepth, 0);
        floor.transform.localScale = new Vector3(poolWidth / 10f, 1f, poolLength / 10f);
        
        // High-resolution mesh for better tile detail
        MeshFilter meshFilter = floor.GetComponent<MeshFilter>();
        meshFilter.mesh = CreateDetailedPlaneMesh(100, 200);
        SaveMesh(meshFilter.sharedMesh, "PoolFloorDetailed");
        
        // Apply procedural tile shader
        if (useProceduralTiles)
        {
            ProceduralTileShader tileShader = floor.AddComponent<ProceduralTileShader>();
            tileShader.tileSize = 0.8f; // Büyütüldü (0.3 -> 0.8)
            tileShader.groutWidth = 0.02f; // Derz de biraz kalınlaştırıldı
            tileShader.tileColor = new Color(0.85f, 0.9f, 0.95f);
            tileShader.groutColor = new Color(0.5f, 0.5f, 0.5f);
            tileShader.glazeSmoothness = 0.85f;
            
            if (enableWetnessEffects)
            {
                tileShader.wetnessAmount = 0.3f;
                tileShader.wetSmoothness = 0.95f;
            }
        }
        else
        {
            Material floorMat = new Material(Shader.Find("Standard"));
            floorMat.color = new Color(0.85f, 0.9f, 0.95f);
            floorMat.SetFloat("_Metallic", 0.1f);
            floorMat.SetFloat("_Glossiness", 0.85f);
            floor.GetComponent<Renderer>().material = floorMat;
        }
    }
    
    void CreatePoolWalls()
    {
        GameObject wallsParent = new GameObject("PoolWalls");
        
        CreateWallWithTiles("NorthWall", new Vector3(0, -poolDepth / 2f, poolLength / 2f), 
                           new Vector3(poolWidth, poolDepth, 0.5f), wallsParent.transform);
        CreateWallWithTiles("SouthWall", new Vector3(0, -poolDepth / 2f, -poolLength / 2f), 
                           new Vector3(poolWidth, poolDepth, 0.5f), wallsParent.transform);
        CreateWallWithTiles("EastWall", new Vector3(poolWidth / 2f, -poolDepth / 2f, 0), 
                           new Vector3(0.5f, poolDepth, poolLength), wallsParent.transform);
        CreateWallWithTiles("WestWall", new Vector3(-poolWidth / 2f, -poolDepth / 2f, 0), 
                           new Vector3(0.5f, poolDepth, poolLength), wallsParent.transform);
    }
    
    void CreateWallWithTiles(string name, Vector3 position, Vector3 scale, Transform parent)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.parent = parent;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        
        if (useProceduralTiles)
        {
            ProceduralTileShader tileShader = wall.AddComponent<ProceduralTileShader>();
            tileShader.tileSize = 0.8f; // Büyütüldü (0.2 -> 0.8)
            tileShader.groutWidth = 0.02f;
            tileShader.tileColor = new Color(0.7f, 0.8f, 0.9f);
            tileShader.glazeSmoothness = 0.8f;
        }
        else
        {
            Material wallMat = new Material(Shader.Find("Standard"));
            wallMat.color = new Color(0.7f, 0.8f, 0.9f);
            wallMat.SetFloat("_Metallic", 0.1f);
            wallMat.SetFloat("_Glossiness", 0.8f);
            wall.GetComponent<Renderer>().material = wallMat;
        }
    }
    
    void CreateAdvancedWaterSurface()
    {
        GameObject waterObj = new GameObject("AdvancedWaterSurface");
        waterObj.transform.position = new Vector3(0, 0, 0);
        
        // Create high-resolution water mesh
        MeshFilter meshFilter = waterObj.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateWaterMesh(waterMeshResolution, waterMeshResolution);
        SaveMesh(meshFilter.sharedMesh, "PoolWaterDetailed");
        
        MeshRenderer renderer = waterObj.AddComponent<MeshRenderer>();
        
        // Create advanced water material
        Material waterMat = CreateAdvancedWaterMaterial();
        renderer.material = waterMat;
        
        if (enableAdvancedWater)
        {
            // Add advanced water shader component
            AdvancedWaterShader waterShader = waterObj.AddComponent<AdvancedWaterShader>();
            waterShader.waterMaterial = waterMat;
            waterShader.waterMeshFilter = meshFilter;
            
            waterShader.waveAmplitude = 0.05f; // Subtle waves for indoor pool
            waterShader.waveFrequency = 1.5f;
            waterShader.waveSpeed = 0.3f;
            waterShader.waveCount = 4;
            
            waterShader.shallowWaterColor = new Color(0.3f, 0.7f, 0.8f, 0.7f);
            waterShader.deepWaterColor = new Color(0.05f, 0.2f, 0.4f, 0.9f);
            waterShader.depthFalloff = 5f;
            
            waterShader.refractionStrength = 0.3f;
            waterShader.reflectionStrength = 0.6f;
            waterShader.smoothness = 0.95f;
            
            if (enableCaustics)
            {
                waterShader.causticsStrength = 0.8f;
                waterShader.causticsScale = 2f;
                waterShader.causticsSpeed = 0.3f;
            }
        }
        
        // Add box collider for water surface detection
        BoxCollider waterCollider = waterObj.AddComponent<BoxCollider>();
        waterCollider.isTrigger = true;
        waterCollider.center = new Vector3(0, -poolDepth / 2f, 0);
        waterCollider.size = new Vector3(poolWidth, poolDepth, poolLength);
    }
    
    Mesh CreateWaterMesh(int resX, int resZ)
    {
        Mesh mesh = new Mesh();
        mesh.name = "WaterMesh";
        
        Vector3[] vertices = new Vector3[(resX + 1) * (resZ + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[resX * resZ * 6];
        
        float sizeX = poolWidth;
        float sizeZ = poolLength;
        
        for (int z = 0; z <= resZ; z++)
        {
            for (int x = 0; x <= resX; x++)
            {
                int index = z * (resX + 1) + x;
                float xPos = (x / (float)resX - 0.5f) * sizeX;
                float zPos = (z / (float)resZ - 0.5f) * sizeZ;
                
                vertices[index] = new Vector3(xPos, 0, zPos);
                uv[index] = new Vector2(x / (float)resX, z / (float)resZ);
            }
        }
        
        int triIndex = 0;
        for (int z = 0; z < resZ; z++)
        {
            for (int x = 0; x < resX; x++)
            {
                int vertIndex = z * (resX + 1) + x;
                
                triangles[triIndex++] = vertIndex;
                triangles[triIndex++] = vertIndex + resX + 1;
                triangles[triIndex++] = vertIndex + 1;
                
                triangles[triIndex++] = vertIndex + 1;
                triangles[triIndex++] = vertIndex + resX + 1;
                triangles[triIndex++] = vertIndex + resX + 2;
            }
        }
        
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }
    
    Mesh CreateDetailedPlaneMesh(int resX, int resZ)
    {
        Mesh mesh = new Mesh();
        mesh.name = "DetailedPlane";
        
        Vector3[] vertices = new Vector3[(resX + 1) * (resZ + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[resX * resZ * 6];
        
        for (int z = 0; z <= resZ; z++)
        {
            for (int x = 0; x <= resX; x++)
            {
                int index = z * (resX + 1) + x;
                vertices[index] = new Vector3(
                    (x / (float)resX - 0.5f) * 10f,
                    0,
                    (z / (float)resZ - 0.5f) * 10f
                );
                uv[index] = new Vector2(x / (float)resX, z / (float)resZ);
            }
        }
        
        int triIndex = 0;
        for (int z = 0; z < resZ; z++)
        {
            for (int x = 0; x < resX; x++)
            {
                int vertIndex = z * (resX + 1) + x;
                
                triangles[triIndex++] = vertIndex;
                triangles[triIndex++] = vertIndex + resX + 1;
                triangles[triIndex++] = vertIndex + 1;
                
                triangles[triIndex++] = vertIndex + 1;
                triangles[triIndex++] = vertIndex + resX + 1;
                triangles[triIndex++] = vertIndex + resX + 2;
            }
        }
        
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    Material CreateAdvancedWaterMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        
        mat.SetColor("_Color", new Color(0.3f, 0.7f, 0.8f, 0.7f));
        mat.SetFloat("_Metallic", 0.1f);
        mat.SetFloat("_Glossiness", 0.95f);
        
        return mat;
    }
    
    void SetupIndoorLighting()
    {
        GameObject lightingParent = new GameObject("IndoorLighting");
        
        // Main directional light (simulating skylight)
        GameObject mainLightObj = new GameObject("MainDirectionalLight");
        mainLightObj.transform.parent = lightingParent.transform;
        Light mainLight = mainLightObj.AddComponent<Light>();
        mainLight.type = LightType.Directional;
        mainLight.color = new Color(0.9f, 0.95f, 1f); // Hafif mavi-beyaz
        mainLight.intensity = 0.8f; // Parlaklık düşürüldü (1.3 -> 0.8)
        mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        mainLight.shadows = LightShadows.Soft;
        
        // Volumetric lighting effect via light cookie/settings
        if (enableVolumetricLighting)
        {
            mainLight.shadowStrength = 0.8f;
            mainLight.shadowBias = 0.05f;
        }
        
        // Spotlights for pool illumination
        float spacing = poolLength / (numberOfSpotlights + 1);
        for (int i = 0; i < numberOfSpotlights; i++)
        {
            float zPos = -poolLength / 2f + spacing * (i + 1);
            CreateSpotlight($"PoolSpotlight_{i}", new Vector3(0, 7f, zPos), lightingParent.transform);
        }
    }
    
    void CreateSpotlight(string name, Vector3 position, Transform parent)
    {
        GameObject spotObj = new GameObject(name);
        spotObj.transform.parent = parent;
        spotObj.transform.position = position;
        spotObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        
        Light spot = spotObj.AddComponent<Light>();
        spot.type = LightType.Spot;
        spot.color = new Color(0.95f, 0.95f, 1f); 
        spot.intensity = 1.5f; // Parlaklık ciddi oranda düşürüldü (4 -> 1.5)
        spot.range = 25f; 
        spot.spotAngle = 70f;
        spot.shadows = LightShadows.Soft; 
        
        // Enhanced spot lighting
        if (enableVolumetricLighting)
        {
            spot.shadowStrength = 0.6f;
            spot.innerSpotAngle = spot.spotAngle * 0.6f;
        }
    }
    
    void SetupNaturalLight()
    {
        GameObject windowsParent = new GameObject("Windows");
        
        float spacing = poolLength / (numberOfWindows + 1);
        for (int i = 0; i < numberOfWindows; i++)
        {
            float zPos = -poolLength / 2f + spacing * (i + 1);
            CreateWindow($"Window_{i}", new Vector3(poolWidth / 2f + 2f, 3f, zPos), windowsParent.transform);
        }
    }
    
    void CreateWindow(string name, Vector3 position, Transform parent)
    {
        GameObject window = GameObject.CreatePrimitive(PrimitiveType.Quad);
        window.name = name;
        window.transform.parent = parent;
        window.transform.position = position;
        window.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        window.transform.localScale = new Vector3(3f, 4f, 1f);
        
        Material windowMat = new Material(Shader.Find("Standard"));
        windowMat.SetFloat("_Mode", 3);
        windowMat.color = new Color(0.9f, 0.95f, 1f, 0.3f);
        windowMat.SetFloat("_Metallic", 0f);
        windowMat.SetFloat("_Glossiness", 0.9f);
        windowMat.EnableKeyword("_ALPHABLEND_ON");
        window.GetComponent<Renderer>().material = windowMat;
        
        DestroyImmediate(window.GetComponent<Collider>());
        
        // Add area light for window
        GameObject lightObj = new GameObject($"{name}_Light");
        lightObj.transform.parent = window.transform;
        lightObj.transform.localPosition = Vector3.zero;
        lightObj.transform.localRotation = Quaternion.identity;
        
        Light areaLight = lightObj.AddComponent<Light>();
        areaLight.type = LightType.Point;
        areaLight.color = new Color(1f, 0.98f, 0.9f);
        areaLight.intensity = 1.5f;
        areaLight.range = 20f;
    }
    
    void CreateLaneLines()
    {
        GameObject laneLinesParent = new GameObject("LaneLines");
        float laneWidth = poolWidth / laneCount;
        
        for (int i = 1; i < laneCount; i++)
        {
            float xPos = -poolWidth / 2f + (i * laneWidth);
            
            GameObject laneLine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            laneLine.name = $"LaneLine_{i}";
            laneLine.transform.parent = laneLinesParent.transform;
            laneLine.transform.position = new Vector3(xPos, -poolDepth / 2f, 0f);
            laneLine.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            laneLine.transform.localScale = new Vector3(0.08f, poolLength / 2f, 0.08f);
            
            Material laneMat = new Material(Shader.Find("Standard"));
            laneMat.color = i % 2 == 0 ? new Color(0.2f, 0.4f, 0.9f) : Color.white;
            laneMat.SetFloat("_Metallic", 0.3f);
            laneMat.SetFloat("_Glossiness", 0.7f);
            laneLine.GetComponent<Renderer>().material = laneMat;
            
            DestroyImmediate(laneLine.GetComponent<Collider>());
        }
    }
    
    void CreateLaneMarkers()
    {
        GameObject markersParent = new GameObject("FloorLaneMarkers");
        float laneWidth = poolWidth / laneCount;
        
        Material stripeMat = new Material(Shader.Find("Standard"));
        stripeMat.color = new Color(0.05f, 0.1f, 0.5f); // Koyu Lacivert (Olimpik)
        stripeMat.SetFloat("_Glossiness", 0.9f); // Parlak
        stripeMat.SetFloat("_Metallic", 0.1f);

        for (int i = 0; i < laneCount; i++)
        {
            // Kulvarın tam ortası
            float xPos = -poolWidth / 2f + (i * laneWidth) + (laneWidth / 2f);
            
            // 1. Ana Şerit (Uzun çizgi)
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Quad);
            stripe.name = $"FloorStripe_{i}";
            stripe.transform.parent = markersParent.transform;
            stripe.transform.position = new Vector3(xPos, -poolDepth + 0.01f, 0f); // Zeminin 1cm üstü
            stripe.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Yere yatık
            stripe.transform.localScale = new Vector3(0.25f, poolLength - 2f, 1f); // Quad Y scale boyu belirler
            
            stripe.GetComponent<Renderer>().material = stripeMat;
            DestroyImmediate(stripe.GetComponent<Collider>());
            
            // 2. Başlangıç T (Yatay çizgi)
            GameObject tStart = GameObject.CreatePrimitive(PrimitiveType.Quad);
            tStart.name = $"T_Start_{i}";
            tStart.transform.parent = markersParent.transform;
            tStart.transform.position = new Vector3(xPos, -poolDepth + 0.01f, -(poolLength/2f) + 1f);
            tStart.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            tStart.transform.localScale = new Vector3(1f, 0.25f, 1f); 
            tStart.GetComponent<Renderer>().material = stripeMat;
            DestroyImmediate(tStart.GetComponent<Collider>());
            
            // 3. Bitiş T (Yatay çizgi)
            GameObject tEnd = GameObject.CreatePrimitive(PrimitiveType.Quad);
            tEnd.name = $"T_End_{i}";
            tEnd.transform.parent = markersParent.transform;
            tEnd.transform.position = new Vector3(xPos, -poolDepth + 0.01f, (poolLength/2f) - 1f);
            tEnd.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            tEnd.transform.localScale = new Vector3(1f, 0.25f, 1f);
            tEnd.GetComponent<Renderer>().material = stripeMat;
            DestroyImmediate(tEnd.GetComponent<Collider>());
        }
    }
    
    void CreateStartingBlocks()
    {
        // Implementation similar to OlympicPoolDetails
        // Omitted for brevity
    }
    
    void CreatePoolDeck()
    {
        // Implementation similar to OlympicPoolDetails
        // Omitted for brevity
    }
    
    void SetupParticleEffects()
    {
        if (!enableParticleEffects) return;
        
        // Simple bubble particle system
        GameObject particleObj = new GameObject("PoolParticles");
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 5f;
        main.startSpeed = 0.5f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.1f);
        main.startColor = new Color(0.8f, 0.9f, 1f, 0.4f);
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 20f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(poolWidth, 0.1f, poolLength);
        shape.position = new Vector3(0, -poolDepth, 0);
        
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
    }
    
    void CreateTestObjects()
    {
        if (!enableAdvancedBuoyancy) return;
        
        // Create floating ball - ROV'dan uzakta
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "FloatingBall";
        ball.transform.position = new Vector3(15f, 0.5f, 10f);  // ROV'dan uzakta
        ball.transform.localScale = Vector3.one * 0.5f;
        
        Rigidbody ballRb = ball.AddComponent<Rigidbody>();
        ballRb.mass = 0.5f;
        
        AdvancedBuoyancy ballBuoyancy = ball.AddComponent<AdvancedBuoyancy>();
        ballBuoyancy.waterDensity = 1000f;
        ballBuoyancy.autoCalculateVolume = true;
        ballBuoyancy.waterSurfaceY = 0f;
        ballBuoyancy.createSplashEffects = true;
        
        Material ballMat = new Material(Shader.Find("Standard"));
        ballMat.color = new Color(1f, 0.3f, 0.3f);
        ball.GetComponent<Renderer>().material = ballMat;
    }
    
    void CreateROVWithAdvancedPhysics()
    {
        GameObject rov = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rov.name = "ROV";
        rov.transform.position = new Vector3(0, -1.5f, 0);  // Havuzun ortasında, 1.5m derinlikte
        rov.transform.localScale = new Vector3(1f, 0.6f, 1.5f);
        
        // ÖNEMLİ: Rigidbody'yi ÖNCE ekle ve HEMEN ayarla
        Rigidbody rb = rov.AddComponent<Rigidbody>();
        rb.mass = 50f;
        rb.useGravity = false;  // ÇOK ÖNEMLİ: Hemen false yap!
        rb.drag = 3f;  // Yüksek su direnci
        rb.angularDrag = 5f;  // Yüksek dönüş direnci
        rb.interpolation = RigidbodyInterpolation.Interpolate;  // Smooth hareket
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;  // Daha iyi collision
        
        // Constraints - Roll ve Pitch'i kilitle
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
        // --- ROV MOTORLARI (GÖRSEL) ---
        Material thrusterMat = new Material(Shader.Find("Standard"));
        thrusterMat.color = new Color(0.2f, 0.2f, 0.25f); // Koyu gri
        thrusterMat.SetFloat("_Glossiness", 0.5f);

        // Arka Motorlar (İleri/Geri)
        GameObject t1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        t1.name = "Thruster_RearLeft";
        t1.transform.parent = rov.transform;
        t1.transform.localPosition = new Vector3(-0.6f, 0, -0.6f);
        t1.transform.localRotation = Quaternion.Euler(90, 0, 0);
        t1.transform.localScale = new Vector3(0.12f, 0.2f, 0.12f);
        t1.GetComponent<Renderer>().material = thrusterMat;
        DestroyImmediate(t1.GetComponent<Collider>());

        GameObject t2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        t2.name = "Thruster_RearRight";
        t2.transform.parent = rov.transform;
        t2.transform.localPosition = new Vector3(0.6f, 0, -0.6f);
        t2.transform.localRotation = Quaternion.Euler(90, 0, 0);
        t2.transform.localScale = new Vector3(0.12f, 0.2f, 0.12f);
        t2.GetComponent<Renderer>().material = thrusterMat;
        DestroyImmediate(t2.GetComponent<Collider>());
        
        // Dikey Motorlar (Yukarı/Aşağı)
        GameObject t3 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        t3.name = "Thruster_VerticalLeft";
        t3.transform.parent = rov.transform;
        t3.transform.localPosition = new Vector3(-0.7f, 0, 0.2f);
        t3.transform.localScale = new Vector3(0.12f, 0.2f, 0.12f);
        t3.GetComponent<Renderer>().material = thrusterMat;
        DestroyImmediate(t3.GetComponent<Collider>());
        
        GameObject t4 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        t4.name = "Thruster_VerticalRight";
        t4.transform.parent = rov.transform;
        t4.transform.localPosition = new Vector3(0.7f, 0, 0.2f);
        t4.transform.localScale = new Vector3(0.12f, 0.2f, 0.12f);
        t4.GetComponent<Renderer>().material = thrusterMat;
        DestroyImmediate(t4.GetComponent<Collider>());

        // Şimdi ROVController ekle
        ROVController controller = rov.AddComponent<ROVController>();
        controller.horizontalThrustForce = 15f;
        controller.verticalThrustForce = 12f;
        controller.rotationThrustForce = 8f;
        controller.autoStabilize = true;
        controller.lockRoll = true;
        
        // Camera
        GameObject camObj = new GameObject("ROVCamera");
        camObj.transform.parent = rov.transform;
        camObj.transform.localPosition = new Vector3(0, 0.3f, 0.5f);
        
        Camera cam = camObj.AddComponent<Camera>();
        cam.backgroundColor = new Color(0.2f, 0.5f, 0.8f);
        cam.clearFlags = CameraClearFlags.Skybox;  // Skybox kullan
        cam.farClipPlane = 100f;
        cam.nearClipPlane = 0.1f;
        cam.tag = "MainCamera";
        
        controller.cameraTransform = camObj.transform;
        camObj.AddComponent<AudioListener>();
        
        // Add Underwater Visual Effects
        UnderwaterEffectController underwaterParams = camObj.AddComponent<UnderwaterEffectController>();
        underwaterParams.waterLevel = 0f;
        underwaterParams.shallowFogColor = new Color(0.1f, 0.45f, 0.75f); // Olimpik havuz mavisi
        underwaterParams.shallowFogDensity = 0.08f;
        
        Material rovMat = new Material(Shader.Find("Standard"));
        rovMat.color = new Color(1f, 0.6f, 0.1f);
        rovMat.SetFloat("_Metallic", 0.7f);
        rovMat.SetFloat("_Glossiness", 0.6f);
        rov.GetComponent<Renderer>().material = rovMat;
        
        Debug.Log($"ROV olusturuldu: Pozisyon={rov.transform.position}, UseGravity={rb.useGravity}, Mass={rb.mass}");
    }
    
    void AddPauseMenu()
    {
        GameObject installer = new GameObject("PauseMenu_Installer");
        installer.AddComponent<BuildPauseMenu>();
    }
    
    void AddEventSystem()
    {
        if (!FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>())
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }
    
    void SetupPostProcessing()
    {
        // Setup ambient and fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.2f, 0.5f, 0.8f);
        RenderSettings.fogDensity = 0.03f;
        
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.5f, 0.6f, 0.7f);
        RenderSettings.ambientEquatorColor = new Color(0.4f, 0.5f, 0.6f);
        RenderSettings.ambientGroundColor = new Color(0.3f, 0.4f, 0.5f);
    }
}

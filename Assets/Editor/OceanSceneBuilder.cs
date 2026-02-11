using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool that builds the ROV and ChargingStation as persistent scene objects.
/// Run via: Tools > Build Ocean Scene Objects
/// Objects appear in Hierarchy and are saved with the scene.
/// </summary>
public class OceanSceneBuilder : EditorWindow
{
    [MenuItem("Tools/Build Ocean Scene Objects")]
    static void BuildAll()
    {
        // ═══════════════════════════════════════
        // Clean up existing objects first
        // ═══════════════════════════════════════
        DestroyExisting("ROV");
        DestroyExisting("ChargingStation");
        
        // ═══════════════════════════════════════
        // Find water surface for positioning
        // ═══════════════════════════════════════
        GameObject ws = GameObject.Find("WaterSurface");
        float waterY = ws != null ? ws.transform.position.y : 10f;
        
        // ═══════════════════════════════════════
        // 1. BUILD ROV
        // ═══════════════════════════════════════
        GameObject rov = BuildROV(new Vector3(0, waterY - 3f, 0));
        
        // ═══════════════════════════════════════
        // 2. BUILD CHARGING STATION
        // ═══════════════════════════════════════
        Vector3 stationPos = new Vector3(20f, -5f, 15f);
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(stationPos.x, 50f, stationPos.z), Vector3.down, out hit, 100f))
            stationPos.y = hit.point.y + 0.2f;
        
        GameObject station = BuildChargingStation(stationPos);
        
        // ═══════════════════════════════════════
        // Mark scene dirty so Unity knows to save
        // ═══════════════════════════════════════
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log($"<color=green>Ocean Scene Objects built! ROV at {rov.transform.position}, Station at {station.transform.position}</color>");
        
        // Select the ROV in hierarchy
        Selection.activeGameObject = rov;
    }
    
    static void DestroyExisting(string name)
    {
        GameObject existing = GameObject.Find(name);
        while (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
            existing = GameObject.Find(name);
        }
    }
    
    // ═══════════════════════════════════════════════════════
    //  ROV BUILDER
    // ═══════════════════════════════════════════════════════
    static GameObject BuildROV(Vector3 position)
    {
        // ── ROOT ──
        GameObject rov = new GameObject("ROV");
        Undo.RegisterCreatedObjectUndo(rov, "Create ROV");
        rov.transform.position = position;
        
        // Rigidbody
        Rigidbody rb = rov.AddComponent<Rigidbody>();
        rb.mass = 50f;
        rb.drag = 2f;
        rb.angularDrag = 5f;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        // Main collider
        BoxCollider col = rov.AddComponent<BoxCollider>();
        col.size = new Vector3(1.2f, 0.6f, 1.6f);
        col.center = Vector3.zero;
        
        // ── HULL (visual body) ──
        GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hull.name = "Hull";
        hull.transform.SetParent(rov.transform);
        hull.transform.localPosition = Vector3.zero;
        hull.transform.localRotation = Quaternion.identity;
        hull.transform.localScale = new Vector3(1.0f, 0.5f, 1.4f);
        Object.DestroyImmediate(hull.GetComponent<Collider>());
        
        Material hullMat = new Material(Shader.Find("Standard"));
        hullMat.color = new Color(1f, 0.6f, 0.1f);
        hullMat.SetFloat("_Metallic", 0.7f);
        hullMat.SetFloat("_Glossiness", 0.6f);
        hull.GetComponent<Renderer>().sharedMaterial = hullMat;
        
        // Dome (behind camera, won't obstruct view)
        GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dome.name = "Dome";
        dome.transform.SetParent(hull.transform);
        dome.transform.localPosition = new Vector3(0, 0.4f, -0.2f);
        dome.transform.localScale = new Vector3(0.5f, 0.3f, 0.4f);
        Object.DestroyImmediate(dome.GetComponent<Collider>());
        Material domeMat = new Material(Shader.Find("Standard"));
        domeMat.color = new Color(0.2f, 0.25f, 0.3f);
        domeMat.SetFloat("_Metallic", 0.3f);
        domeMat.SetFloat("_Glossiness", 0.9f);
        dome.GetComponent<Renderer>().sharedMaterial = domeMat;
        
        // Nose (front, below camera line)
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nose.name = "Nose";
        nose.transform.SetParent(hull.transform);
        nose.transform.localPosition = new Vector3(0, -0.1f, 0.55f);
        nose.transform.localScale = new Vector3(0.5f, 0.3f, 0.3f);
        Object.DestroyImmediate(nose.GetComponent<Collider>());
        nose.GetComponent<Renderer>().sharedMaterial = hullMat;
        
        // Side pontoons (pushed to sides, won't block view)
        Material pontoonMat = new Material(Shader.Find("Standard"));
        pontoonMat.color = new Color(0.2f, 0.2f, 0.25f);
        pontoonMat.SetFloat("_Metallic", 0.8f);
        
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject pontoon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pontoon.name = side < 0 ? "Pontoon_Left" : "Pontoon_Right";
            pontoon.transform.SetParent(hull.transform);
            pontoon.transform.localPosition = new Vector3(side * 0.65f, -0.2f, -0.1f);
            pontoon.transform.localRotation = Quaternion.Euler(0, 0, 90);
            pontoon.transform.localScale = new Vector3(0.12f, 0.35f, 0.12f);
            Object.DestroyImmediate(pontoon.GetComponent<Collider>());
            pontoon.GetComponent<Renderer>().sharedMaterial = pontoonMat;
        }
        
        // Thruster nozzles
        Material thrMat = new Material(Shader.Find("Standard"));
        thrMat.color = new Color(0.15f, 0.15f, 0.15f);
        thrMat.SetFloat("_Metallic", 0.9f);
        
        Vector3[] thrusterPos = {
            new Vector3(-0.5f, -0.2f, 0.5f), new Vector3(0.5f, -0.2f, 0.5f),
            new Vector3(-0.5f, -0.2f, -0.5f), new Vector3(0.5f, -0.2f, -0.5f)
        };
        for (int i = 0; i < thrusterPos.Length; i++)
        {
            GameObject thr = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            thr.name = $"Thruster_{i}";
            thr.transform.SetParent(hull.transform);
            thr.transform.localPosition = thrusterPos[i];
            thr.transform.localScale = new Vector3(0.12f, 0.1f, 0.12f);
            Object.DestroyImmediate(thr.GetComponent<Collider>());
            thr.GetComponent<Renderer>().sharedMaterial = thrMat;
        }
        
        // ── CAMERA (above and behind hull front — clear view forward) ──
        GameObject camObj = new GameObject("ROVCamera");
        camObj.transform.SetParent(rov.transform);
        camObj.transform.localPosition = new Vector3(0, 0.45f, 0.75f);
        camObj.transform.localRotation = Quaternion.Euler(8f, 0, 0);
        
        Camera cam = camObj.AddComponent<Camera>();
        cam.fieldOfView = 68f;
        cam.nearClipPlane = 0.3f;  // Clips through nearby hull geometry
        cam.farClipPlane = 150f;
        cam.tag = "MainCamera";
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.04f, 0.15f, 0.28f);
        
        camObj.AddComponent<AudioListener>();
        
        // ── FRONT SPOTLIGHTS (children of hull) ──
        Material lightMat = new Material(Shader.Find("Standard"));
        lightMat.EnableKeyword("_EMISSION");
        lightMat.color = Color.white;
        lightMat.SetColor("_EmissionColor", Color.white * 2f);
        
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject spotObj = new GameObject(side < 0 ? "SpotLight_Front_L" : "SpotLight_Front_R");
            spotObj.transform.SetParent(hull.transform);
            spotObj.transform.localPosition = new Vector3(side * 0.35f, 0.05f, 0.7f);
            spotObj.transform.localRotation = Quaternion.Euler(10f, 0, 0);
            
            Light spot = spotObj.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.color = new Color(0.85f, 0.95f, 1f);
            spot.intensity = 6f;
            spot.range = 35f;
            spot.spotAngle = 50f;
            spot.shadows = LightShadows.Soft;
            
            // Housing visual
            GameObject housing = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            housing.name = "Housing";
            housing.transform.SetParent(spotObj.transform);
            housing.transform.localPosition = Vector3.zero;
            housing.transform.localScale = new Vector3(0.1f, 0.1f, 0.08f);
            Object.DestroyImmediate(housing.GetComponent<Collider>());
            housing.GetComponent<Renderer>().sharedMaterial = lightMat;
        }
        
        // Work light (bottom)
        GameObject workObj = new GameObject("WorkLight_Bottom");
        workObj.transform.SetParent(hull.transform);
        workObj.transform.localPosition = new Vector3(0, -0.35f, 0.2f);
        workObj.transform.localRotation = Quaternion.Euler(90f, 0, 0);
        Light work = workObj.AddComponent<Light>();
        work.type = LightType.Spot;
        work.color = new Color(0.9f, 0.95f, 1f);
        work.intensity = 4f;
        work.range = 20f;
        work.spotAngle = 70f;
        work.shadows = LightShadows.None;
        
        // Ambient glow
        GameObject ambObj = new GameObject("AmbientLight_ROV");
        ambObj.transform.SetParent(hull.transform);
        ambObj.transform.localPosition = Vector3.zero;
        Light amb = ambObj.AddComponent<Light>();
        amb.type = LightType.Point;
        amb.color = new Color(0.7f, 0.85f, 1f);
        amb.intensity = 1.5f;
        amb.range = 8f;
        amb.shadows = LightShadows.None;
        
        // ── SCRIPTS ──
        ROVController controller = rov.AddComponent<ROVController>();
        controller.cameraTransform = camObj.transform;
        rov.AddComponent<ROVLightController>();
        
        return rov;
    }
    
    // ═══════════════════════════════════════════════════════
    //  CHARGING STATION BUILDER  
    // ═══════════════════════════════════════════════════════
    static GameObject BuildChargingStation(Vector3 position)
    {
        GameObject station = new GameObject("ChargingStation");
        Undo.RegisterCreatedObjectUndo(station, "Create ChargingStation");
        station.transform.position = position;
        
        // Add collider for sonar detection
        SphereCollider sc = station.AddComponent<SphereCollider>();
        sc.radius = 2f;
        sc.isTrigger = false;
        
        Color beaconColor = new Color(1f, 0.15f, 0.1f);
        
        // ── Base platform ──
        GameObject basePlat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        basePlat.name = "Base";
        basePlat.transform.SetParent(station.transform);
        basePlat.transform.localPosition = Vector3.zero;
        basePlat.transform.localScale = new Vector3(4f, 0.4f, 4f);
        Material baseMat = new Material(Shader.Find("Standard"));
        baseMat.color = new Color(0.15f, 0.15f, 0.2f);
        baseMat.SetFloat("_Metallic", 0.9f);
        baseMat.SetFloat("_Glossiness", 0.7f);
        basePlat.GetComponent<Renderer>().sharedMaterial = baseMat;
        
        // Landing pad ring
        GameObject padRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        padRing.name = "LandingPad";
        padRing.transform.SetParent(station.transform);
        padRing.transform.localPosition = new Vector3(0, 0.22f, 0);
        padRing.transform.localScale = new Vector3(2.5f, 0.05f, 2.5f);
        Material padMat = new Material(Shader.Find("Standard"));
        padMat.EnableKeyword("_EMISSION");
        padMat.color = new Color(0.3f, 0.05f, 0.05f);
        padMat.SetColor("_EmissionColor", beaconColor * 0.5f);
        padRing.GetComponent<Renderer>().sharedMaterial = padMat;
        
        // Central pillar
        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = "Pillar";
        pillar.transform.SetParent(station.transform);
        pillar.transform.localPosition = new Vector3(0, 1.2f, 0);
        pillar.transform.localScale = new Vector3(0.4f, 1.0f, 0.4f);
        Object.DestroyImmediate(pillar.GetComponent<Collider>());
        Material pillarMat = new Material(Shader.Find("Standard"));
        pillarMat.color = new Color(0.2f, 0.2f, 0.25f);
        pillarMat.SetFloat("_Metallic", 0.8f);
        pillar.GetComponent<Renderer>().sharedMaterial = pillarMat;
        
        // Beacon sphere
        GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        beacon.name = "Beacon";
        beacon.transform.SetParent(station.transform);
        beacon.transform.localPosition = new Vector3(0, 2.5f, 0);
        beacon.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        Object.DestroyImmediate(beacon.GetComponent<Collider>());
        Material beaconMat = new Material(Shader.Find("Standard"));
        beaconMat.EnableKeyword("_EMISSION");
        beaconMat.SetColor("_Color", beaconColor);
        beaconMat.SetColor("_EmissionColor", beaconColor * 3f);
        beacon.GetComponent<Renderer>().sharedMaterial = beaconMat;
        
        // Beacon light
        GameObject lightObj = new GameObject("BeaconLight");
        lightObj.transform.SetParent(station.transform);
        lightObj.transform.localPosition = new Vector3(0, 2.5f, 0);
        Light beaconLight = lightObj.AddComponent<Light>();
        beaconLight.type = LightType.Point;
        beaconLight.color = beaconColor;
        beaconLight.intensity = 4f;
        beaconLight.range = 40f;
        beaconLight.shadows = LightShadows.None;
        
        // Antennas
        for (int i = 0; i < 3; i++)
        {
            float angle = i * 120f * Mathf.Deg2Rad;
            GameObject ant = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ant.name = $"Antenna_{i}";
            ant.transform.SetParent(station.transform);
            ant.transform.localPosition = new Vector3(Mathf.Cos(angle) * 1.5f, 1f, Mathf.Sin(angle) * 1.5f);
            ant.transform.localScale = new Vector3(0.08f, 1f, 0.08f);
            Object.DestroyImmediate(ant.GetComponent<Collider>());
            Material antMat = new Material(Shader.Find("Standard"));
            antMat.color = new Color(0.3f, 0.1f, 0.1f);
            antMat.SetFloat("_Metallic", 0.7f);
            ant.GetComponent<Renderer>().sharedMaterial = antMat;
            
            GameObject tipLight = new GameObject($"TipLight_{i}");
            tipLight.transform.SetParent(ant.transform);
            tipLight.transform.localPosition = new Vector3(0, 0.5f, 0);
            Light tip = tipLight.AddComponent<Light>();
            tip.type = LightType.Point;
            tip.color = beaconColor;
            tip.intensity = 0.8f;
            tip.range = 5f;
        }
        
        // Corner markers
        Material markMat = new Material(Shader.Find("Standard"));
        markMat.EnableKeyword("_EMISSION");
        markMat.color = new Color(0.8f, 0.2f, 0.1f);
        markMat.SetColor("_EmissionColor", beaconColor);
        
        for (int i = 0; i < 4; i++)
        {
            float a = (i * 90f + 45f) * Mathf.Deg2Rad;
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = $"Marker_{i}";
            marker.transform.SetParent(station.transform);
            marker.transform.localPosition = new Vector3(Mathf.Cos(a) * 1.7f, 0.6f, Mathf.Sin(a) * 1.7f);
            marker.transform.localScale = new Vector3(0.06f, 0.4f, 0.06f);
            Object.DestroyImmediate(marker.GetComponent<Collider>());
            marker.GetComponent<Renderer>().sharedMaterial = markMat;
        }
        
        // Add runtime script (handles charging, pulse, etc)
        station.AddComponent<ChargingStation>();
        
        return station;
    }
}

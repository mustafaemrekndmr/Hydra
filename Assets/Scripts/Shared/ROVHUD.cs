using UnityEngine;

/// <summary>
/// Professional ROV HUD with depth (from water surface), speed, heading, battery, pitch/roll indicators.
/// Optimized: cached styles & textures (allocated once), compass with cardinal directions.
/// Depth is calculated based on the WaterSurface GameObject's Y position.
/// </summary>
public class ROVHUD : MonoBehaviour
{
    [Header("References")]
    public Transform rovTransform;
    public Rigidbody rovRigidbody;
    
    [Header("HUD Settings")]
    public bool showHUD = true;
    public KeyCode toggleKey = KeyCode.H;
    
    [Header("Telemetry")]
    public float waterSurfaceY = 20f; // Auto-detected from WaterSurface object
    public float maxBatteryLife = 300f;
    public float warningDepth = 15f;
    public float dangerDepth = 25f;
    
    private float currentBattery;
    private float missionTime;
    private bool isBeingCharged = false;
    
    /// <summary>Battery percentage (0-100)</summary>
    public float BatteryPercent => Mathf.Max(0f, (currentBattery / maxBatteryLife) * 100f);
    
    /// <summary>True when battery is completely empty</summary>
    public bool IsBatteryDead => currentBattery <= 0f;
    
    /// <summary>Add battery charge from external source (e.g. charging station)</summary>
    public void AddBattery(float amount)
    {
        currentBattery = Mathf.Min(currentBattery + amount, maxBatteryLife);
        isBeingCharged = true;
    }
    
    // Cached styles
    private GUIStyle hudStyle;
    private GUIStyle titleStyle;
    private GUIStyle warningStyle;
    private GUIStyle dangerStyle;
    private GUIStyle dimStyle;
    private GUIStyle centerStyle;
    private bool stylesInitialized = false;
    
    // Cached textures (created once, destroyed on exit)
    private Texture2D borderTex;
    private Texture2D fillTex;
    private bool texturesCreated = false;
    
    // Cached values (updated periodically, not every frame)
    private float cachedDepth;
    private float cachedSpeed;
    private float cachedHeading;
    private float cachedAltitude; // distance to seafloor
    private float cacheTimer;
    private const float CACHE_INTERVAL = 0.1f;
    
    void Start()
    {
        currentBattery = maxBatteryLife;
        
        if (rovTransform == null)
            rovTransform = GameObject.Find("ROV")?.transform;
        
        if (rovRigidbody == null && rovTransform != null)
            rovRigidbody = rovTransform.GetComponent<Rigidbody>();
        
        // Auto-detect water surface level
        GameObject waterSurface = GameObject.Find("WaterSurface");
        if (waterSurface != null)
        {
            waterSurfaceY = waterSurface.transform.position.y;
            Debug.Log($"ROVHUD: Water surface detected at Y={waterSurfaceY}");
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            showHUD = !showHUD;
        
        if (currentBattery > 0)
            currentBattery -= Time.deltaTime;
        
        // Reset charging flag each frame (ChargingStation sets it via AddBattery)
        isBeingCharged = false;
        
        missionTime += Time.deltaTime;
        
        // Update cached values periodically
        cacheTimer += Time.deltaTime;
        if (cacheTimer >= CACHE_INTERVAL && rovTransform != null)
        {
            cacheTimer = 0f;
            cachedDepth = Mathf.Max(0f, waterSurfaceY - rovTransform.position.y);
            cachedSpeed = rovRigidbody != null ? rovRigidbody.velocity.magnitude : 0f;
            cachedHeading = rovTransform.eulerAngles.y;
            
            // Altitude: raycast down to find seafloor distance
            RaycastHit hit;
            if (Physics.Raycast(rovTransform.position, Vector3.down, out hit, 200f))
                cachedAltitude = hit.distance;
            else
                cachedAltitude = -1f; // Unknown
        }
    }
    
    void InitStyles()
    {
        if (stylesInitialized) return;
        stylesInitialized = true;
        
        hudStyle = new GUIStyle();
        hudStyle.fontSize = 13;
        hudStyle.normal.textColor = new Color(0.3f, 1f, 0.5f);
        hudStyle.fontStyle = FontStyle.Bold;
        
        titleStyle = new GUIStyle(hudStyle);
        titleStyle.fontSize = 15;
        titleStyle.normal.textColor = new Color(0.4f, 1f, 1f);
        
        warningStyle = new GUIStyle(hudStyle);
        warningStyle.normal.textColor = new Color(1f, 0.8f, 0.2f);
        
        dangerStyle = new GUIStyle(hudStyle);
        dangerStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);
        
        dimStyle = new GUIStyle(hudStyle);
        dimStyle.fontSize = 11;
        dimStyle.normal.textColor = new Color(0.5f, 0.7f, 0.5f);
        
        centerStyle = new GUIStyle(hudStyle);
        centerStyle.alignment = TextAnchor.MiddleCenter;
    }
    
    void CreateTextures()
    {
        if (texturesCreated) return;
        texturesCreated = true;
        
        borderTex = new Texture2D(1, 1);
        borderTex.SetPixel(0, 0, new Color(0.2f, 0.6f, 0.4f, 0.5f));
        borderTex.Apply();
        
        fillTex = new Texture2D(1, 1);
        fillTex.SetPixel(0, 0, Color.white); // We'll tint via GUI.color
        fillTex.Apply();
    }
    
    void OnGUI()
    {
        if (!showHUD || rovTransform == null) return;
        
        InitStyles();
        CreateTextures();
        
        DrawTelemetryPanel();
        DrawCompass();
        DrawDepthBar();
        DrawCrosshair();
        DrawControlsHint();
    }
    
    void DrawTelemetryPanel()
    {
        float panelW = 260f, panelH = 240f;
        float x = 10f, y = 10f;
        
        // Background
        GUI.color = new Color(0, 0.02f, 0.05f, 0.75f);
        GUI.Box(new Rect(x, y, panelW, panelH), "");
        GUI.color = Color.white;
        
        DrawBorder(new Rect(x, y, panelW, panelH));
        
        float ly = y + 8f;
        float lh = 22f;
        float lx = x + 12f;
        
        // Title
        GUI.Label(new Rect(lx, ly, 240, 20), "══ ROV TELEMETRY ══", titleStyle);
        ly += lh + 4f;
        
        // Depth
        GUIStyle depthStyle = hudStyle;
        if (cachedDepth > dangerDepth) depthStyle = dangerStyle;
        else if (cachedDepth > warningDepth) depthStyle = warningStyle;
        GUI.Label(new Rect(lx, ly, 240, 20), $"DEPTH    {cachedDepth,7:F1} m", depthStyle);
        ly += lh;
        
        // Altitude (distance to seafloor)
        string altText = cachedAltitude >= 0 ? $"{cachedAltitude:F1} m" : "N/A";
        GUI.Label(new Rect(lx, ly, 240, 20), $"ALT      {altText,7}", dimStyle);
        ly += lh;
        
        // Speed
        GUI.Label(new Rect(lx, ly, 240, 20), $"SPEED    {cachedSpeed,7:F2} m/s", hudStyle);
        ly += lh;
        
        // Heading
        string headingDir = GetCardinalDirection(cachedHeading);
        GUI.Label(new Rect(lx, ly, 240, 20), $"HEADING  {cachedHeading,5:F0}° {headingDir}", hudStyle);
        ly += lh;
        
        // Battery
        float batteryPercent = Mathf.Max(0f, (currentBattery / maxBatteryLife) * 100f);
        GUIStyle batteryStyle = batteryPercent < 10f ? dangerStyle : (batteryPercent < 25f ? warningStyle : hudStyle);
        string batteryBar = GetProgressBar(batteryPercent / 100f, 10);
        GUI.Label(new Rect(lx, ly, 240, 20), $"BATTERY  {batteryBar} {batteryPercent:F0}%", batteryStyle);
        ly += lh;
        
        // Mission time
        int mins = Mathf.FloorToInt(missionTime / 60f);
        int secs = Mathf.FloorToInt(missionTime % 60f);
        GUI.Label(new Rect(lx, ly, 240, 20), $"TIME     {mins:D2}:{secs:D2}", dimStyle);
        ly += lh;
        
        // Status
        string status;
        GUIStyle statusStyle;
        if (currentBattery <= 0f)
        {
            status = "POWER DEAD";
            statusStyle = dangerStyle;
        }
        else if (isBeingCharged)
        {
            status = "CHARGING";
            statusStyle = warningStyle;
        }
        else if (batteryPercent < 10f)
        {
            status = "LOW POWER";
            statusStyle = dangerStyle;
        }
        else
        {
            status = "OPERATIONAL";
            statusStyle = hudStyle;
        }
        GUI.Label(new Rect(lx, ly, 240, 20), $"STATUS   {status}", statusStyle);
    }
    
    void DrawCompass()
    {
        float cx = Screen.width - 130f;
        float cy = 15f;
        float size = 110f;
        
        // Background
        GUI.color = new Color(0, 0.02f, 0.05f, 0.75f);
        GUI.Box(new Rect(cx, cy, size, size), "");
        GUI.color = Color.white;
        DrawBorder(new Rect(cx, cy, size, size));
        
        float centerX = cx + size / 2f;
        float centerY = cy + size / 2f;
        
        // Cardinal directions rotating
        string[] dirs = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        float[] angles = { 0, 45, 90, 135, 180, 225, 270, 315 };
        
        for (int i = 0; i < dirs.Length; i++)
        {
            float angle = (angles[i] - cachedHeading) * Mathf.Deg2Rad;
            float radius = 38f;
            float px = centerX + Mathf.Sin(angle) * radius - 8f;
            float py = centerY - Mathf.Cos(angle) * radius - 8f;
            
            GUIStyle dirStyle = (i == 0) ? warningStyle : dimStyle;
            if (i % 2 == 0) dirStyle = (i == 0) ? warningStyle : hudStyle;
            
            GUI.Label(new Rect(px, py, 20, 20), dirs[i], dirStyle);
        }
        
        // Center indicator
        GUI.Label(new Rect(centerX - 4, centerY - 6, 20, 15), "▲", titleStyle);
        
        // Heading readout
        GUI.Label(new Rect(cx + 20, cy + size - 20, 80, 20), $"{cachedHeading:F0}°", hudStyle);
    }
    
    void DrawDepthBar()
    {
        float barX = Screen.width - 40f;
        float barY = 140f;
        float barH = Screen.height - 300f;
        float barW = 20f;
        float maxDisplayDepth = 30f; // total range shown
        
        // ── Background ──
        GUI.color = new Color(0, 0.02f, 0.05f, 0.8f);
        GUI.Box(new Rect(barX - 4, barY - 4, barW + 8, barH + 8), "");
        GUI.color = Color.white;
        
        // ── Gradient fill (full bar, colored by zone) ──
        int segments = 20;
        float segH = barH / segments;
        for (int i = 0; i < segments; i++)
        {
            float segDepth = (float)i / segments * maxDisplayDepth;
            float segY = barY + i * segH;
            
            Color segColor;
            if (segDepth > dangerDepth)
                segColor = new Color(0.8f, 0.15f, 0.15f, 0.3f);
            else if (segDepth > warningDepth)
                segColor = new Color(0.8f, 0.6f, 0.1f, 0.25f);
            else
                segColor = new Color(0.1f, 0.4f, 0.2f, 0.2f);
            
            GUI.color = segColor;
            GUI.DrawTexture(new Rect(barX, segY, barW, segH), fillTex);
        }
        GUI.color = Color.white;
        
        // ── Tick marks every 5m ──
        for (float d = 0; d <= maxDisplayDepth; d += 5f)
        {
            float tickY = barY + (d / maxDisplayDepth) * barH;
            
            // Tick line
            GUI.color = new Color(0.3f, 0.6f, 0.4f, 0.5f);
            GUI.DrawTexture(new Rect(barX - 6, tickY, barW + 6, 1), fillTex);
            GUI.color = Color.white;
            
            // Label
            GUI.Label(new Rect(barX - 40, tickY - 7, 35, 14), $"{d:F0}m", dimStyle);
        }
        
        // ── ROV depth indicator (triangle + depth readout) ──
        float depthRatio = Mathf.Clamp01(cachedDepth / maxDisplayDepth);
        float indicatorY = barY + depthRatio * barH;
        
        // Indicator color
        Color indColor;
        if (cachedDepth > dangerDepth)
            indColor = new Color(1f, 0.3f, 0.3f);
        else if (cachedDepth > warningDepth)
            indColor = new Color(1f, 0.8f, 0.2f);
        else
            indColor = new Color(0.3f, 1f, 0.5f);
        
        // Horizontal line across bar at current depth
        GUI.color = indColor;
        GUI.DrawTexture(new Rect(barX - 8, indicatorY - 1, barW + 12, 3), fillTex);
        GUI.color = Color.white;
        
        // Depth number next to indicator
        GUIStyle depthNumStyle = new GUIStyle(hudStyle);
        depthNumStyle.normal.textColor = indColor;
        depthNumStyle.fontSize = 12;
        depthNumStyle.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(barX - 75, indicatorY - 9, 60, 18), $"{cachedDepth:F1}m", depthNumStyle);
        
        // Title
        GUIStyle barTitle = new GUIStyle(dimStyle);
        barTitle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(barX - 10, barY - 20, barW + 20, 14), "DEPTH", barTitle);
    }
    
    void DrawCrosshair()
    {
        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;
        
        GUI.color = new Color(0.3f, 1f, 0.5f, 0.3f);
        GUI.DrawTexture(new Rect(cx - 12, cy - 0.5f, 24, 1), fillTex);
        GUI.DrawTexture(new Rect(cx - 0.5f, cy - 12, 1, 24), fillTex);
        
        // Small center dot
        GUI.color = new Color(0.3f, 1f, 0.5f, 0.6f);
        GUI.DrawTexture(new Rect(cx - 1.5f, cy - 1.5f, 3, 3), fillTex);
        GUI.color = Color.white;
    }
    
    void DrawControlsHint()
    {
        float y = Screen.height - 35f;
        GUI.Label(new Rect(10, y, 700, 25),
            "[H]HUD  [L]Lights  [F]WorkLight  [Tab]Sonar  [Space]Hold  [WASD]Move  [QE]Up/Down",
            dimStyle);
    }
    
    void DrawBorder(Rect rect)
    {
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 1), borderTex);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - 1, rect.width, 1), borderTex);
        GUI.DrawTexture(new Rect(rect.x, rect.y, 1, rect.height), borderTex);
        GUI.DrawTexture(new Rect(rect.xMax - 1, rect.y, 1, rect.height), borderTex);
    }
    
    string GetCardinalDirection(float heading)
    {
        if (heading >= 337.5f || heading < 22.5f) return "N";
        if (heading >= 22.5f && heading < 67.5f) return "NE";
        if (heading >= 67.5f && heading < 112.5f) return "E";
        if (heading >= 112.5f && heading < 157.5f) return "SE";
        if (heading >= 157.5f && heading < 202.5f) return "S";
        if (heading >= 202.5f && heading < 247.5f) return "SW";
        if (heading >= 247.5f && heading < 292.5f) return "W";
        return "NW";
    }
    
    string GetProgressBar(float ratio, int length)
    {
        int filled = Mathf.RoundToInt(ratio * length);
        string bar = "[";
        for (int i = 0; i < length; i++)
            bar += (i < filled) ? "█" : "░";
        bar += "]";
        return bar;
    }
    
    void OnDestroy()
    {
        if (borderTex != null) Destroy(borderTex);
        if (fillTex != null) Destroy(fillTex);
    }
}

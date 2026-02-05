using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ROV Heads-Up Display showing depth, speed, battery, and other telemetry
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
    public float waterSurfaceLevel = 0f;
    public float maxBatteryLife = 300f; // 5 minutes
    private float currentBattery;
    
    private GUIStyle hudStyle;
    private GUIStyle titleStyle;
    private GUIStyle warningStyle;
    
    void Start()
    {
        currentBattery = maxBatteryLife;
        
        if (rovTransform == null)
            rovTransform = GameObject.Find("ROV")?.transform;
        
        if (rovRigidbody == null && rovTransform != null)
            rovRigidbody = rovTransform.GetComponent<Rigidbody>();
        
        SetupStyles();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            showHUD = !showHUD;
        }
        
        // Drain battery slowly
        if (currentBattery > 0)
        {
            currentBattery -= Time.deltaTime;
        }
    }
    
    void SetupStyles()
    {
        hudStyle = new GUIStyle();
        hudStyle.fontSize = 14;
        hudStyle.normal.textColor = new Color(0.3f, 1f, 0.3f); // Green terminal color
        hudStyle.fontStyle = FontStyle.Bold;
        hudStyle.alignment = TextAnchor.UpperLeft;
        
        titleStyle = new GUIStyle(hudStyle);
        titleStyle.fontSize = 18;
        titleStyle.normal.textColor = new Color(0.5f, 1f, 1f); // Cyan
        
        warningStyle = new GUIStyle(hudStyle);
        warningStyle.normal.textColor = new Color(1f, 0.3f, 0.3f); // Red
    }
    
    void OnGUI()
    {
        if (!showHUD || rovTransform == null) return;
        
        // Background panel
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.Box(new Rect(10, 10, 300, 250), "");
        GUI.color = Color.white;
        
        float yPos = 20;
        float lineHeight = 25;
        
        // Title
        GUI.Label(new Rect(20, yPos, 280, 30), "═══ ROV TELEMETRY ═══", titleStyle);
        yPos += lineHeight + 10;
        
        // Depth
        float depth = Mathf.Abs(rovTransform.position.y - waterSurfaceLevel);
        GUI.Label(new Rect(20, yPos, 280, 20), $"DEPTH: {depth:F1} m", hudStyle);
        yPos += lineHeight;
        
        // Speed
        float speed = rovRigidbody != null ? rovRigidbody.velocity.magnitude : 0f;
        GUI.Label(new Rect(20, yPos, 280, 20), $"SPEED: {speed:F2} m/s", hudStyle);
        yPos += lineHeight;
        
        // Position
        Vector3 pos = rovTransform.position;
        GUI.Label(new Rect(20, yPos, 280, 20), $"POS: X:{pos.x:F1} Y:{pos.y:F1} Z:{pos.z:F1}", hudStyle);
        yPos += lineHeight;
        
        // Heading (rotation)
        float heading = rovTransform.eulerAngles.y;
        GUI.Label(new Rect(20, yPos, 280, 20), $"HEADING: {heading:F0}°", hudStyle);
        yPos += lineHeight;
        
        // Battery
        float batteryPercent = (currentBattery / maxBatteryLife) * 100f;
        GUIStyle batteryStyle = batteryPercent < 20f ? warningStyle : hudStyle;
        GUI.Label(new Rect(20, yPos, 280, 20), $"BATTERY: {batteryPercent:F0}%", batteryStyle);
        yPos += lineHeight;
        
        // Status indicators
        GUI.Label(new Rect(20, yPos, 280, 20), "STATUS: OPERATIONAL", hudStyle);
        yPos += lineHeight;
        
        // Controls hint
        GUI.Label(new Rect(20, yPos, 280, 20), $"[{toggleKey}] Toggle HUD", hudStyle);
        
        // Compass (simple)
        DrawCompass();
    }
    
    void DrawCompass()
    {
        if (rovTransform == null) return;
        
        float compassX = Screen.width - 120;
        float compassY = 20;
        float compassSize = 100;
        
        // Background circle
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.Box(new Rect(compassX, compassY, compassSize, compassSize), "");
        GUI.color = Color.white;
        
        // Compass label
        GUI.Label(new Rect(compassX + 30, compassY + 35, 100, 30), "N", titleStyle);
        
        // Heading indicator
        float heading = rovTransform.eulerAngles.y;
        GUI.Label(new Rect(compassX + 20, compassY + 60, 100, 30), $"{heading:F0}°", hudStyle);
    }
}

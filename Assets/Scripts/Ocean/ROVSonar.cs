using UnityEngine;

/// <summary>
/// ROV Sonar system for detecting nearby objects
/// Provides visual and audio feedback for navigation in dark water
/// </summary>
public class ROVSonar : MonoBehaviour
{
    [Header("Sonar Settings")]
    public bool sonarEnabled = true;
    public KeyCode toggleKey = KeyCode.S;
    public float sonarRange = 20f;
    public float sonarPingInterval = 2f;
    public LayerMask detectionLayers = -1;
    
    [Header("Visual Feedback")]
    public bool showVisualPing = true;
    public Color pingColor = new Color(0.3f, 1f, 0.3f, 0.5f);
    public float pingDuration = 1f;
    
    [Header("Audio Feedback")]
    public bool enableAudio = true;
    public float audioVolume = 0.3f;
    
    [Header("Detection")]
    public int maxDetections = 10;
    public bool showDetectionMarkers = true;
    public Color markerColor = new Color(1f, 0.5f, 0f);
    
    private float nextPingTime;
    private Collider[] detectedObjects;
    private float[] detectionDistances;
    private Vector3[] detectionPositions;
    private int detectionCount;
    private float pingAnimationTime;
    
    void Start()
    {
        detectedObjects = new Collider[maxDetections];
        detectionDistances = new float[maxDetections];
        detectionPositions = new Vector3[maxDetections];
        nextPingTime = Time.time + sonarPingInterval;
    }
    
    void Update()
    {
        // Toggle sonar
        if (Input.GetKeyDown(toggleKey))
        {
            sonarEnabled = !sonarEnabled;
            Debug.Log($"Sonar {(sonarEnabled ? "ENABLED" : "DISABLED")}");
        }
        
        if (!sonarEnabled) return;
        
        // Ping at intervals
        if (Time.time >= nextPingTime)
        {
            PerformSonarPing();
            nextPingTime = Time.time + sonarPingInterval;
        }
        
        // Update ping animation
        if (pingAnimationTime > 0)
        {
            pingAnimationTime -= Time.deltaTime;
        }
    }
    
    void PerformSonarPing()
    {
        // Detect objects in range
        detectionCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            sonarRange,
            detectedObjects,
            detectionLayers
        );
        
        // Store detection info
        for (int i = 0; i < detectionCount; i++)
        {
            if (detectedObjects[i] != null && detectedObjects[i].transform != transform)
            {
                detectionPositions[i] = detectedObjects[i].transform.position;
                detectionDistances[i] = Vector3.Distance(transform.position, detectionPositions[i]);
            }
        }
        
        // Visual feedback
        if (showVisualPing)
        {
            pingAnimationTime = pingDuration;
        }
        
        // Audio feedback (simple beep based on closest object)
        if (enableAudio && detectionCount > 0)
        {
            float closestDistance = float.MaxValue;
            for (int i = 0; i < detectionCount; i++)
            {
                if (detectionDistances[i] < closestDistance)
                    closestDistance = detectionDistances[i];
            }
            
            // Pitch based on distance (closer = higher pitch)
            float pitch = 1f + (1f - (closestDistance / sonarRange));
            // Simple audio feedback (you can add AudioSource for better sound)
            Debug.Log($"PING! {detectionCount} objects detected. Closest: {closestDistance:F1}m");
        }
    }
    
    void OnDrawGizmos()
    {
        if (!sonarEnabled) return;
        
        // Draw sonar range
        Gizmos.color = new Color(pingColor.r, pingColor.g, pingColor.b, 0.1f);
        Gizmos.DrawWireSphere(transform.position, sonarRange);
        
        // Draw detected objects
        if (showDetectionMarkers && Application.isPlaying)
        {
            Gizmos.color = markerColor;
            for (int i = 0; i < detectionCount; i++)
            {
                if (detectedObjects[i] != null)
                {
                    Gizmos.DrawLine(transform.position, detectionPositions[i]);
                    Gizmos.DrawWireSphere(detectionPositions[i], 0.5f);
                }
            }
        }
    }
    
    void OnGUI()
    {
        if (!sonarEnabled) return;
        
        // Sonar HUD
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.normal.textColor = pingColor;
        style.fontStyle = FontStyle.Bold;
        
        // Sonar status
        GUI.Label(new Rect(Screen.width - 220, 20, 200, 30), $"SONAR: ACTIVE", style);
        GUI.Label(new Rect(Screen.width - 220, 45, 200, 30), $"Range: {sonarRange:F0}m", style);
        GUI.Label(new Rect(Screen.width - 220, 70, 200, 30), $"Contacts: {detectionCount}", style);
        
        // Ping animation
        if (pingAnimationTime > 0)
        {
            float alpha = pingAnimationTime / pingDuration;
            GUI.color = new Color(pingColor.r, pingColor.g, pingColor.b, alpha);
            
            float pingSize = (1f - alpha) * 100f + 50f;
            GUI.Box(new Rect(Screen.width - 150 - pingSize/2, 100, pingSize, pingSize), "");
            GUI.color = Color.white;
        }
        
        // Toggle hint
        style.fontSize = 12;
        style.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        GUI.Label(new Rect(Screen.width - 220, 95, 200, 30), $"[{toggleKey}] Toggle", style);
    }
}

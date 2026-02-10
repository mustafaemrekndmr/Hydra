using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Realistic ROV Sonar system with radar-style sweep display.
/// Draws a circular sonar map with rotating sweep line and fading contact blips.
/// Toggle: Tab key. Shows bearing, distance, and object classification.
/// </summary>
public class ROVSonar : MonoBehaviour
{
    [Header("Sonar Settings")]
    public bool sonarEnabled = true;
    public KeyCode toggleKey = KeyCode.Tab;
    public float sonarRange = 50f;
    public float sweepSpeed = 90f; // degrees per second
    public float pingInterval = 3f;
    public LayerMask detectionLayers = -1;
    
    [Header("Display Settings")]
    public float displaySize = 180f;
    public float displayMarginX = 15f;
    public float displayMarginBottom = 55f; // from bottom of screen
    
    [Header("Audio")]
    public bool enablePingSound = true;
    [Range(0f, 1f)] public float pingVolume = 0.15f;
    
    // Internal state
    private float sweepAngle = 0f;
    private float nextPingTime;
    private List<SonarContact> contacts = new List<SonarContact>();
    private Texture2D sonarBgTex;
    private Texture2D blipTex;
    private Texture2D sweepTex;
    private Texture2D ringTex;
    private GUIStyle labelStyle;
    private GUIStyle titleStyle;
    private GUIStyle dimStyle;
    private bool texturesCreated = false;
    private AudioSource audioSource;
    
    // Ping audio
    private float pingToneTimer = 0f;
    private const float PING_TONE_DURATION = 0.08f;
    
    private struct SonarContact
    {
        public Vector3 worldPosition;
        public float distance;
        public float bearing; // angle from ROV forward in degrees
        public float detectedTime;
        public string label;
        public ContactType type;
    }
    
    public enum ContactType
    {
        Unknown,
        Terrain,
        Object,
        Living
    }
    
    void Start()
    {
        nextPingTime = Time.time + 0.5f;
        CreateTextures();
        InitStyles();
        
        // Setup audio source for ping
        if (enablePingSound)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = pingVolume;
        }
    }
    
    void CreateTextures()
    {
        if (texturesCreated) return;
        texturesCreated = true;
        
        // Sonar background (dark green-black)
        sonarBgTex = new Texture2D(1, 1);
        sonarBgTex.SetPixel(0, 0, new Color(0.01f, 0.04f, 0.02f, 0.85f));
        sonarBgTex.Apply();
        
        // Blip texture (bright green dot)
        blipTex = new Texture2D(1, 1);
        blipTex.SetPixel(0, 0, new Color(0.2f, 1f, 0.3f, 1f));
        blipTex.Apply();
        
        // Sweep line (bright green)
        sweepTex = new Texture2D(1, 1);
        sweepTex.SetPixel(0, 0, new Color(0.1f, 0.8f, 0.2f, 0.7f));
        sweepTex.Apply();
        
        // Ring texture (dim green)
        ringTex = new Texture2D(1, 1);
        ringTex.SetPixel(0, 0, new Color(0.05f, 0.25f, 0.1f, 0.5f));
        ringTex.Apply();
    }
    
    void InitStyles()
    {
        labelStyle = new GUIStyle();
        labelStyle.fontSize = 11;
        labelStyle.normal.textColor = new Color(0.2f, 0.9f, 0.3f);
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        
        titleStyle = new GUIStyle(labelStyle);
        titleStyle.fontSize = 12;
        titleStyle.normal.textColor = new Color(0.3f, 1f, 0.4f);
        
        dimStyle = new GUIStyle(labelStyle);
        dimStyle.fontSize = 10;
        dimStyle.normal.textColor = new Color(0.1f, 0.5f, 0.2f);
    }
    
    void Update()
    {
        // Toggle with Tab
        if (Input.GetKeyDown(toggleKey))
        {
            sonarEnabled = !sonarEnabled;
        }
        
        if (!sonarEnabled) return;
        
        // Rotate sweep
        sweepAngle += sweepSpeed * Time.deltaTime;
        if (sweepAngle >= 360f)
            sweepAngle -= 360f;
        
        // Periodic ping detection
        if (Time.time >= nextPingTime)
        {
            PerformPing();
            nextPingTime = Time.time + pingInterval;
            pingToneTimer = PING_TONE_DURATION;
        }
        
        // Play ping tone
        if (pingToneTimer > 0f)
        {
            pingToneTimer -= Time.deltaTime;
        }
        
        // Remove old contacts (fade after 2 full sweeps)
        float maxAge = (360f / sweepSpeed) * 2f;
        contacts.RemoveAll(c => Time.time - c.detectedTime > maxAge);
    }
    
    void PerformPing()
    {
        contacts.Clear();
        
        Collider[] hits = Physics.OverlapSphere(transform.position, sonarRange, detectionLayers);
        
        foreach (Collider hit in hits)
        {
            // Skip self, triggers, children
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;
            if (hit.isTrigger) continue;
            
            Vector3 toTarget = hit.transform.position - transform.position;
            float dist = toTarget.magnitude;
            
            if (dist < 1f) continue; // too close
            
            // Calculate bearing relative to ROV's forward direction (on XZ plane)
            Vector3 flatForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
            Vector3 flatToTarget = new Vector3(toTarget.x, 0, toTarget.z).normalized;
            float bearing = Vector3.SignedAngle(flatForward, flatToTarget, Vector3.up);
            
            // Classify contact
            ContactType type = ContactType.Object;
            if (hit.GetComponent<Terrain>() != null || hit.GetComponent<TerrainCollider>() != null)
                type = ContactType.Terrain;
            else if (hit.GetComponent<Light>() != null)
                type = ContactType.Living; // bioluminescent
            
            string label = hit.gameObject.name;
            if (label.Length > 12) label = label.Substring(0, 12);
            
            contacts.Add(new SonarContact
            {
                worldPosition = hit.transform.position,
                distance = dist,
                bearing = bearing,
                detectedTime = Time.time,
                label = label,
                type = type
            });
        }
        
        // Play ping sound
        if (enablePingSound && audioSource != null)
        {
            PlayPingTone();
        }
    }
    
    void PlayPingTone()
    {
        // Generate a simple sonar ping sound procedurally
        int sampleRate = 44100;
        int samples = (int)(sampleRate * 0.15f);
        AudioClip ping = AudioClip.Create("SonarPing", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 20f); // Quick decay
            // 1500Hz ping (typical sonar frequency feel)
            data[i] = Mathf.Sin(2f * Mathf.PI * 1500f * t) * envelope * 0.3f;
        }
        
        ping.SetData(data, 0);
        audioSource.clip = ping;
        audioSource.volume = pingVolume;
        audioSource.Play();
    }
    
    void OnGUI()
    {
        if (!sonarEnabled) return;
        
        DrawSonarDisplay();
    }
    
    void DrawSonarDisplay()
    {
        // Anchor to bottom-left
        float cx = displayMarginX + displaySize / 2f;
        float cy = Screen.height - displayMarginBottom - displaySize / 2f;
        float radius = displaySize / 2f;
        
        // ── Background circle approximation ──
        GUI.DrawTexture(new Rect(cx - radius, cy - radius, displaySize, displaySize), sonarBgTex);
        
        // ── Range rings ──
        DrawCircle(cx, cy, radius * 0.25f, ringTex);
        DrawCircle(cx, cy, radius * 0.5f, ringTex);
        DrawCircle(cx, cy, radius * 0.75f, ringTex);
        DrawCircle(cx, cy, radius * 0.98f, ringTex);
        
        // ── Cross-hairs (N-S, E-W lines) ──
        GUI.color = new Color(0.05f, 0.2f, 0.08f, 0.4f);
        GUI.DrawTexture(new Rect(cx - radius, cy - 0.5f, displaySize, 1), ringTex);
        GUI.DrawTexture(new Rect(cx - 0.5f, cy - radius, 1, displaySize), ringTex);
        GUI.color = Color.white;
        
        // ── Sweep line ──
        float sweepRad = sweepAngle * Mathf.Deg2Rad;
        float sweepEndX = cx + Mathf.Sin(sweepRad) * radius;
        float sweepEndY = cy - Mathf.Cos(sweepRad) * radius;
        DrawLine(cx, cy, sweepEndX, sweepEndY, sweepTex, 2f);
        
        // ── Sweep trail (fade behind sweep) ──
        for (int i = 1; i <= 15; i++)
        {
            float trailAngle = (sweepAngle - i * 2f) * Mathf.Deg2Rad;
            float trailAlpha = 0.3f * (1f - (float)i / 15f);
            float tx = cx + Mathf.Sin(trailAngle) * radius;
            float ty = cy - Mathf.Cos(trailAngle) * radius;
            
            GUI.color = new Color(0.1f, 0.7f, 0.2f, trailAlpha);
            DrawLine(cx, cy, tx, ty, sweepTex, 1f);
        }
        GUI.color = Color.white;
        
        // ── Contact blips ──
        float maxAge = (360f / sweepSpeed) * 2f;
        
        foreach (var contact in contacts)
        {
            float age = Time.time - contact.detectedTime;
            float alpha = Mathf.Clamp01(1f - (age / maxAge));
            
            // Map bearing to screen position
            float contactAngle = contact.bearing * Mathf.Deg2Rad;
            float contactDist = (contact.distance / sonarRange) * radius;
            
            float bx = cx + Mathf.Sin(contactAngle) * contactDist;
            float by = cy - Mathf.Cos(contactAngle) * contactDist;
            
            // Blip color based on type
            Color blipColor;
            switch (contact.type)
            {
                case ContactType.Terrain:
                    blipColor = new Color(0.6f, 0.4f, 0.1f, alpha); // brown
                    break;
                case ContactType.Living:
                    blipColor = new Color(0.2f, 0.5f, 1f, alpha); // blue
                    break;
                default:
                    blipColor = new Color(0.2f, 1f, 0.3f, alpha); // green
                    break;
            }
            
            // Draw blip (size based on type)
            float blipSize = contact.type == ContactType.Terrain ? 6f : 4f;
            GUI.color = blipColor;
            GUI.DrawTexture(new Rect(bx - blipSize / 2, by - blipSize / 2, blipSize, blipSize), blipTex);
            GUI.color = Color.white;
        }
        
        // ── Center dot (ROV position) ──
        GUI.color = new Color(1f, 1f, 1f, 0.9f);
        GUI.DrawTexture(new Rect(cx - 2, cy - 2, 4, 4), blipTex);
        GUI.color = Color.white;
        
        // ── Labels ──
        // Title
        GUI.Label(new Rect(cx - 40, cy - radius - 18, 80, 16), "SONAR", titleStyle);
        
        // Range labels at rings
        GUI.Label(new Rect(cx + 2, cy - radius * 0.5f - 6, 40, 12), 
            $"{sonarRange * 0.5f:F0}m", dimStyle);
        GUI.Label(new Rect(cx + 2, cy - radius * 0.98f - 6, 40, 12), 
            $"{sonarRange:F0}m", dimStyle);
        
        // Cardinal directions
        GUI.Label(new Rect(cx - 5, cy - radius + 2, 12, 12), "N", labelStyle);
        GUI.Label(new Rect(cx - 5, cy + radius - 14, 12, 12), "S", labelStyle);
        GUI.Label(new Rect(cx + radius - 12, cy - 6, 12, 12), "E", dimStyle);
        GUI.Label(new Rect(cx - radius + 2, cy - 6, 12, 12), "W", dimStyle);
        
        // Stats below display
        float infoY = cy + radius + 4;
        GUI.Label(new Rect(cx - radius, infoY, displaySize, 14), 
            $"CONTACTS: {contacts.Count}  |  RNG: {sonarRange:F0}m", labelStyle);
        
        // Closest contact info
        if (contacts.Count > 0)
        {
            SonarContact closest = contacts[0];
            foreach (var c in contacts)
                if (c.distance < closest.distance) closest = c;
            
            GUI.Label(new Rect(cx - radius, infoY + 16, displaySize, 14),
                $"NEAREST: {closest.label} @ {closest.distance:F1}m  BRG:{closest.bearing:F0}°", dimStyle);
        }
        
        // Toggle hint
        GUI.Label(new Rect(cx - radius, infoY + 32, displaySize, 14),
            $"[{toggleKey}] Toggle", dimStyle);
    }
    
    // ── Drawing helpers ──
    
    void DrawCircle(float cx, float cy, float r, Texture2D tex)
    {
        int segments = 48;
        for (int i = 0; i < segments; i++)
        {
            float a1 = (float)i / segments * Mathf.PI * 2f;
            float a2 = (float)(i + 1) / segments * Mathf.PI * 2f;
            
            float x1 = cx + Mathf.Cos(a1) * r;
            float y1 = cy + Mathf.Sin(a1) * r;
            float x2 = cx + Mathf.Cos(a2) * r;
            float y2 = cy + Mathf.Sin(a2) * r;
            
            DrawLine(x1, y1, x2, y2, tex, 1f);
        }
    }
    
    void DrawLine(float x1, float y1, float x2, float y2, Texture2D tex, float width)
    {
        Vector2 pointA = new Vector2(x1, y1);
        Vector2 pointB = new Vector2(x2, y2);
        
        float length = Vector2.Distance(pointA, pointB);
        if (length < 0.5f) return;
        
        float angle = Mathf.Atan2(pointB.y - pointA.y, pointB.x - pointA.x) * Mathf.Rad2Deg;
        
        Matrix4x4 savedMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, pointA);
        GUI.DrawTexture(new Rect(pointA.x, pointA.y - width / 2f, length, width), tex);
        GUI.matrix = savedMatrix;
    }
    
    void OnDestroy()
    {
        if (sonarBgTex != null) Destroy(sonarBgTex);
        if (blipTex != null) Destroy(blipTex);
        if (sweepTex != null) Destroy(sweepTex);
        if (ringTex != null) Destroy(ringTex);
    }
}

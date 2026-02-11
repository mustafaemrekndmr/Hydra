using UnityEngine;

/// <summary>
/// Controls all ROV lights (front spots, work light, ambient).
/// Keyboard: L = toggle all, F = toggle work light only.
/// Finds lights by name convention under Hull.
/// </summary>
public class ROVLightController : MonoBehaviour
{
    [Header("Light References (auto-detected)")]
    public Light[] frontSpots;
    public Light workLight;
    public Light ambientLight;
    
    [Header("Controls")]
    public KeyCode toggleAllKey = KeyCode.L;
    public KeyCode toggleWorkKey = KeyCode.F;
    public bool lightsOn = true;
    public bool workLightOn = true;
    
    [Header("Settings")]
    public float spotIntensity = 6f;
    public float workIntensity = 4f;
    public float ambientIntensity = 1.5f;
    public float fadeSpeed = 8f;
    
    [Header("Audio")]
    public AudioClip toggleSound;
    private AudioSource audioSource;
    
    // Targets
    private float spotTarget;
    private float workTarget;
    private float ambientTarget;
    
    /// <summary>
    /// Property for other scripts to query
    /// </summary>
    public bool IsLightsOn => lightsOn;
    
    void Start()
    {
        FindLights();
        
        spotTarget = lightsOn ? spotIntensity : 0f;
        workTarget = workLightOn ? workIntensity : 0f;
        ambientTarget = lightsOn ? ambientIntensity : 0f;
        
        // Instant apply
        ApplyInstant();
        
        audioSource = GetComponent<AudioSource>();
    }
    
    void Update()
    {
        // Check battery — no power = no lights
        ROVController controller = GetComponent<ROVController>();
        bool powerDead = controller != null && controller.IsPowerDead;
        
        if (!powerDead)
        {
            if (Input.GetKeyDown(toggleAllKey))
                ToggleAll();
            
            if (Input.GetKeyDown(toggleWorkKey))
                ToggleWorkLight();
        }
        else
        {
            // Force all targets to 0
            spotTarget = 0f;
            workTarget = 0f;
            ambientTarget = 0f;
        }
        
        // Smooth fade
        SmoothFade();
    }
    
    public void ToggleAll()
    {
        lightsOn = !lightsOn;
        spotTarget = lightsOn ? spotIntensity : 0f;
        ambientTarget = lightsOn ? ambientIntensity : 0f;
        
        if (lightsOn)
        {
            workLightOn = true;
            workTarget = workIntensity;
        }
        else
        {
            workLightOn = false;
            workTarget = 0f;
        }
        
        PlayToggleSound();
    }
    
    public void ToggleWorkLight()
    {
        workLightOn = !workLightOn;
        workTarget = workLightOn ? workIntensity : 0f;
        PlayToggleSound();
    }
    
    public void SetLights(bool state)
    {
        lightsOn = state;
        spotTarget = lightsOn ? spotIntensity : 0f;
        ambientTarget = lightsOn ? ambientIntensity : 0f;
        if (!lightsOn) { workLightOn = false; workTarget = 0f; }
    }
    
    void SmoothFade()
    {
        float dt = Time.deltaTime * fadeSpeed;
        
        if (frontSpots != null)
        {
            foreach (Light spot in frontSpots)
            {
                if (spot != null)
                    spot.intensity = Mathf.Lerp(spot.intensity, spotTarget, dt);
            }
        }
        
        if (workLight != null)
            workLight.intensity = Mathf.Lerp(workLight.intensity, workTarget, dt);
        
        if (ambientLight != null)
            ambientLight.intensity = Mathf.Lerp(ambientLight.intensity, ambientTarget, dt);
    }
    
    void ApplyInstant()
    {
        if (frontSpots != null)
        {
            foreach (Light spot in frontSpots)
                if (spot != null) spot.intensity = spotTarget;
        }
        if (workLight != null) workLight.intensity = workTarget;
        if (ambientLight != null) ambientLight.intensity = ambientTarget;
    }
    
    void PlayToggleSound()
    {
        if (audioSource != null && toggleSound != null)
            audioSource.PlayOneShot(toggleSound);
    }
    
    void FindLights()
    {
        Transform hull = transform.Find("Hull");
        if (hull == null) hull = transform;
        
        // Find front spots
        var spots = new System.Collections.Generic.List<Light>();
        foreach (Transform child in hull)
        {
            if (child.name.Contains("SpotLight_Front"))
            {
                Light l = child.GetComponent<Light>();
                if (l != null) spots.Add(l);
            }
            else if (child.name.Contains("WorkLight"))
            {
                workLight = child.GetComponent<Light>();
            }
            else if (child.name.Contains("AmbientLight"))
            {
                ambientLight = child.GetComponent<Light>();
            }
        }
        
        frontSpots = spots.ToArray();
        
        // Log what we found
        if (frontSpots.Length == 0 && workLight == null)
            Debug.LogWarning("ROVLightController: No lights found on ROV.");
    }
}

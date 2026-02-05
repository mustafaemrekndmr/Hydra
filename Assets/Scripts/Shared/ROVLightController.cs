using UnityEngine;

/// <summary>
/// Controls ROV spotlights with keyboard input
/// </summary>
public class ROVLightController : MonoBehaviour
{
    [Header("Light References")]
    public Light leftLight;
    public Light rightLight;
    
    [Header("Control Settings")]
    public KeyCode toggleKey = KeyCode.L; // L tuşu ile ışıkları aç/kapa
    public bool lightsOn = true;
    
    [Header("Light Settings")]
    public float onIntensity = 5f; // Increased for more powerful lights
    public float offIntensity = 0f;
    public float fadeSpeed = 5f; // Yumuşak geçiş
    
    [Header("Audio (Optional)")]
    public AudioClip lightToggleSound;
    private AudioSource audioSource;
    
    private float targetIntensity;
    
    void Start()
    {
        // Find lights if not assigned
        if (leftLight == null || rightLight == null)
        {
            FindROVLights();
        }
        
        // Set initial state
        targetIntensity = lightsOn ? onIntensity : offIntensity;
        if (leftLight != null) leftLight.intensity = targetIntensity;
        if (rightLight != null) rightLight.intensity = targetIntensity;
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && lightToggleSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        Debug.Log($"ROV Light Controller initialized. Press '{toggleKey}' to toggle lights.");
    }
    
    void Update()
    {
        // Toggle lights on key press
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleLights();
        }
        
        // Smooth fade
        if (leftLight != null)
        {
            leftLight.intensity = Mathf.Lerp(leftLight.intensity, targetIntensity, Time.deltaTime * fadeSpeed);
        }
        if (rightLight != null)
        {
            rightLight.intensity = Mathf.Lerp(rightLight.intensity, targetIntensity, Time.deltaTime * fadeSpeed);
        }
    }
    
    public void ToggleLights()
    {
        lightsOn = !lightsOn;
        targetIntensity = lightsOn ? onIntensity : offIntensity;
        
        // Play sound
        if (audioSource != null && lightToggleSound != null)
        {
            audioSource.PlayOneShot(lightToggleSound);
        }
        
        Debug.Log($"ROV Lights: {(lightsOn ? "ON" : "OFF")}");
    }
    
    public void SetLights(bool state)
    {
        lightsOn = state;
        targetIntensity = lightsOn ? onIntensity : offIntensity;
    }
    
    void FindROVLights()
    {
        // Try to find lights in CameraMount flashlights
        Transform cameraMount = transform.Find("Hull/CameraMount");
        if (cameraMount != null)
        {
            Transform leftFlashlight = cameraMount.Find("Flashlight_Left");
            Transform rightFlashlight = cameraMount.Find("Flashlight_Right");
            
            if (leftFlashlight != null)
            {
                Transform leftLightObj = leftFlashlight.Find("Light");
                if (leftLightObj != null)
                    leftLight = leftLightObj.GetComponent<Light>();
            }
            
            if (rightFlashlight != null)
            {
                Transform rightLightObj = rightFlashlight.Find("Light");
                if (rightLightObj != null)
                    rightLight = rightLightObj.GetComponent<Light>();
            }
        }
        
        // Fallback: search in all children
        if (leftLight == null || rightLight == null)
        {
            Light[] lights = GetComponentsInChildren<Light>();
            
            foreach (Light light in lights)
            {
                if (light.type == LightType.Spot)
                {
                    if (light.gameObject.name.Contains("Left") || light.transform.parent.name.Contains("Left"))
                        leftLight = light;
                    else if (light.gameObject.name.Contains("Right") || light.transform.parent.name.Contains("Right"))
                        rightLight = light;
                }
            }
        }
        
        if (leftLight != null && rightLight != null)
        {
            Debug.Log("ROV flashlights found automatically!");
        }
        else
        {
            Debug.LogWarning("Could not find ROV flashlights. Please assign manually.");
        }
    }
    
    // Visual feedback in editor
    void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        // Show light status in top-right corner
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 16;
        style.normal.textColor = lightsOn ? Color.yellow : Color.gray;
        style.alignment = TextAnchor.UpperRight;
        
        string status = lightsOn ? "💡 LIGHTS ON" : "🌑 LIGHTS OFF";
        GUI.Label(new Rect(Screen.width - 200, 10, 190, 30), status, style);
        
        // Show controls hint
        style.fontSize = 12;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(Screen.width - 200, 40, 190, 20), $"Press '{toggleKey}' to toggle", style);
    }
}

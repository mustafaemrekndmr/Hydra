using UnityEngine;

/// <summary>
/// Helper script to setup ROV flashlights with proper lighting
/// </summary>
public class FlashlightSetup : MonoBehaviour
{
    [ContextMenu("Setup Flashlights")]
    public void SetupFlashlights()
    {
        // Find flashlight containers
        Transform cameraMount = GameObject.Find("CameraMount")?.transform;
        if (cameraMount == null)
        {
            Debug.LogError("CameraMount not found!");
            return;
        }
        
        // Setup left flashlight
        SetupFlashlight(cameraMount, "Flashlight_Left");
        
        // Setup right flashlight
        SetupFlashlight(cameraMount, "Flashlight_Right");
        
        Debug.Log("Flashlights setup complete!");
    }
    
    void SetupFlashlight(Transform parent, string flashlightName)
    {
        Transform flashlight = parent.Find(flashlightName);
        if (flashlight == null)
        {
            Debug.LogWarning($"{flashlightName} not found!");
            return;
        }
        
        // Find or create light GameObject
        Transform lightTransform = flashlight.Find("Light");
        if (lightTransform == null)
        {
            Debug.LogWarning($"Light not found in {flashlightName}!");
            return;
        }
        
        // Add Light component if not exists
        Light light = lightTransform.GetComponent<Light>();
        if (light == null)
        {
            light = lightTransform.gameObject.AddComponent<Light>();
        }
        
        // Configure light
        light.type = LightType.Spot;
        light.color = new Color(1f, 0.95f, 0.85f); // Warm white
        light.intensity = 5f; // Increased intensity
        light.range = 30f; // Longer range
        light.spotAngle = 50f;
        light.innerSpotAngle = 25f;
        light.shadows = LightShadows.None; // Performance
        
        // Setup flashlight body material
        Transform body = flashlight.Find("FlashlightBody");
        if (body != null)
        {
            Renderer renderer = body.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.2f, 0.2f, 0.2f); // Dark gray/black
                mat.SetFloat("_Metallic", 0.8f);
                mat.SetFloat("_Glossiness", 0.6f);
                renderer.material = mat;
            }
        }
        
        Debug.Log($"Setup {flashlightName} - Intensity: {light.intensity}, Range: {light.range}");
    }
}

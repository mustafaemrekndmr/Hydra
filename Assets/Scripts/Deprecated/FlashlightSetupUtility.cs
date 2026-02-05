using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor utility to setup flashlights at runtime or in editor
/// </summary>
public class FlashlightSetupUtility : MonoBehaviour
{
    void Start()
    {
        SetupFlashlightsRuntime();
    }
    
    public void SetupFlashlightsRuntime()
    {
        // Find CameraMount
        Transform cameraMount = GameObject.Find("CameraMount")?.transform;
        if (cameraMount == null)
        {
            Debug.LogError("CameraMount not found!");
            return;
        }
        
        // Setup left flashlight
        Transform leftFlashlight = cameraMount.Find("Flashlight_Left");
        if (leftFlashlight != null)
        {
            Transform leftLightObj = leftFlashlight.Find("Light");
            if (leftLightObj != null)
            {
                Light light = leftLightObj.GetComponent<Light>();
                if (light == null)
                {
                    light = leftLightObj.gameObject.AddComponent<Light>();
                }
                ConfigureLight(light);
                Debug.Log("Left flashlight configured");
            }
            
            // Setup body material
            Transform body = leftFlashlight.Find("FlashlightBody");
            if (body != null)
            {
                SetupFlashlightBody(body);
            }
        }
        
        // Setup right flashlight
        Transform rightFlashlight = cameraMount.Find("Flashlight_Right");
        if (rightFlashlight != null)
        {
            Transform rightLightObj = rightFlashlight.Find("Light");
            if (rightLightObj != null)
            {
                Light light = rightLightObj.GetComponent<Light>();
                if (light == null)
                {
                    light = rightLightObj.gameObject.AddComponent<Light>();
                }
                ConfigureLight(light);
                Debug.Log("Right flashlight configured");
            }
            
            // Setup body material
            Transform body = rightFlashlight.Find("FlashlightBody");
            if (body != null)
            {
                SetupFlashlightBody(body);
            }
        }
        
        Debug.Log("Flashlights setup complete!");
    }
    
    void ConfigureLight(Light light)
    {
        light.type = LightType.Spot;
        light.color = new Color(1f, 0.95f, 0.85f); // Warm white
        light.intensity = 5f;
        light.range = 30f;
        light.spotAngle = 50f;
        light.innerSpotAngle = 25f;
        light.shadows = LightShadows.None;
    }
    
    void SetupFlashlightBody(Transform body)
    {
        Renderer renderer = body.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.15f, 0.15f, 0.15f); // Dark gray
            mat.SetFloat("_Metallic", 0.9f);
            mat.SetFloat("_Glossiness", 0.7f);
            renderer.material = mat;
        }
    }
}

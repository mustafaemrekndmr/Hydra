using UnityEngine;

/// <summary>
/// Adds subtle camera shake to simulate underwater currents and ROV vibration
/// </summary>
public class UnderwaterCameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public bool enableShake = true;
    public float shakeIntensity = 0.02f;
    public float shakeSpeed = 1.5f;
    
    [Header("Movement-Based Shake")]
    public bool shakeOnMovement = true;
    public float movementShakeMultiplier = 0.5f;
    public Rigidbody rovRigidbody;
    
    private Vector3 originalLocalPosition;
    private float shakeTime;
    
    void Start()
    {
        originalLocalPosition = transform.localPosition;
        
        if (rovRigidbody == null)
        {
            Transform rov = GameObject.Find("ROV")?.transform;
            if (rov != null)
                rovRigidbody = rov.GetComponent<Rigidbody>();
        }
    }
    
    void Update()
    {
        if (!enableShake) return;
        
        shakeTime += Time.deltaTime * shakeSpeed;
        
        // Base shake (ambient water movement)
        float baseShake = shakeIntensity;
        
        // Additional shake based on ROV movement
        if (shakeOnMovement && rovRigidbody != null)
        {
            float speed = rovRigidbody.velocity.magnitude;
            baseShake += speed * movementShakeMultiplier * shakeIntensity;
        }
        
        // Perlin noise for smooth, natural shake
        float shakeX = (Mathf.PerlinNoise(shakeTime, 0f) - 0.5f) * baseShake;
        float shakeY = (Mathf.PerlinNoise(0f, shakeTime) - 0.5f) * baseShake;
        float shakeZ = (Mathf.PerlinNoise(shakeTime, shakeTime) - 0.5f) * baseShake * 0.5f;
        
        transform.localPosition = originalLocalPosition + new Vector3(shakeX, shakeY, shakeZ);
    }
    
    public void SetShakeIntensity(float intensity)
    {
        shakeIntensity = intensity;
    }
}

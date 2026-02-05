using UnityEngine;

/// <summary>
/// Creates floating particles in water (sediment, plankton, debris)
/// </summary>
public class UnderwaterParticles : MonoBehaviour
{
    [Header("Particle Settings")]
    public int particleCount = 200;
    public float spawnRadius = 20f;
    public float particleSize = 0.05f;
    public Color particleColor = new Color(0.8f, 0.9f, 1f, 0.3f);
    
    [Header("Movement")]
    public float driftSpeed = 0.2f;
    public float turbulenceStrength = 0.5f;
    public Vector3 currentDirection = new Vector3(0.1f, 0.05f, 0);
    
    private ParticleSystem particleSystem;
    
    void Start()
    {
        CreateParticleSystem();
    }
    
    void CreateParticleSystem()
    {
        GameObject particleObj = new GameObject("UnderwaterParticles");
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = Vector3.zero;
        
        particleSystem = particleObj.AddComponent<ParticleSystem>();
        
        var main = particleSystem.main;
        main.loop = true;
        main.startLifetime = 10f;
        main.startSpeed = 0.1f;
        main.startSize = new ParticleSystem.MinMaxCurve(particleSize * 0.5f, particleSize * 2f);
        main.startColor = particleColor;
        main.maxParticles = particleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // Emission
        var emission = particleSystem.emission;
        emission.rateOverTime = particleCount / 5f;
        
        // Shape
        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = spawnRadius;
        
        // Velocity over lifetime (drift)
        var velocity = particleSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(currentDirection.x - turbulenceStrength, currentDirection.x + turbulenceStrength);
        velocity.y = new ParticleSystem.MinMaxCurve(currentDirection.y - turbulenceStrength * 0.5f, currentDirection.y + turbulenceStrength * 0.5f);
        velocity.z = new ParticleSystem.MinMaxCurve(currentDirection.z - turbulenceStrength, currentDirection.z + turbulenceStrength);
        
        // Noise (turbulence)
        var noise = particleSystem.noise;
        noise.enabled = true;
        noise.strength = turbulenceStrength;
        noise.frequency = 0.5f;
        noise.scrollSpeed = 0.2f;
        
        // Renderer
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.SetColor("_Color", particleColor);
        
        Debug.Log($"Underwater particles created: {particleCount} particles");
    }
    
    void Update()
    {
        // Keep particles around camera
        if (particleSystem != null)
        {
            var shape = particleSystem.shape;
            shape.position = Vector3.zero; // Relative to camera
        }
    }
}

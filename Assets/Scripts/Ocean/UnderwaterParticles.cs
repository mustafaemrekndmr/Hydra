using UnityEngine;

/// <summary>
/// Creates floating particles in water (sediment, plankton, debris).
/// Optimized: fixed deprecated field name, proper cleanup.
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
    
    private new ParticleSystem particleSystem; // 'new' to avoid hiding inherited member
    
    void Start()
    {
        CreateParticleSystem();
    }
    
    void CreateParticleSystem()
    {
        // Create as child of this transform (follows camera)
        particleSystem = gameObject.AddComponent<ParticleSystem>();
        
        var main = particleSystem.main;
        main.loop = true;
        main.startLifetime = 10f;
        main.startSpeed = 0.1f;
        main.startSize = new ParticleSystem.MinMaxCurve(particleSize * 0.5f, particleSize * 2f);
        main.startColor = particleColor;
        main.maxParticles = particleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        
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
        velocity.x = new ParticleSystem.MinMaxCurve(
            currentDirection.x - turbulenceStrength,
            currentDirection.x + turbulenceStrength);
        velocity.y = new ParticleSystem.MinMaxCurve(
            currentDirection.y - turbulenceStrength * 0.5f,
            currentDirection.y + turbulenceStrength * 0.5f);
        velocity.z = new ParticleSystem.MinMaxCurve(
            currentDirection.z - turbulenceStrength,
            currentDirection.z + turbulenceStrength);
        
        // Noise (turbulence)
        var noise = particleSystem.noise;
        noise.enabled = true;
        noise.strength = turbulenceStrength;
        noise.frequency = 0.5f;
        noise.scrollSpeed = 0.2f;
        
        // Size over lifetime (fade in/out)
        var sol = particleSystem.sizeOverLifetime;
        sol.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0f);
        sizeCurve.AddKey(0.1f, 1f);
        sizeCurve.AddKey(0.9f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sol.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Renderer
        var psRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        Material pMat = new Material(Shader.Find("Particles/Standard Unlit"));
        pMat.SetColor("_Color", particleColor);
        psRenderer.material = pMat;
    }
    
    void OnDestroy()
    {
        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
    }
}

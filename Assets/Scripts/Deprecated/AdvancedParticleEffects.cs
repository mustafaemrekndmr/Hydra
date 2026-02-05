using UnityEngine;

/// <summary>
/// Advanced particle system manager for underwater and atmospheric effects
/// Handles bubbles, splash particles, and dust motes
/// </summary>
public class AdvancedParticleEffects : MonoBehaviour
{
    [Header("Bubble System")]
    public bool enableBubbles = true;
    public ParticleSystem bubbleParticleSystem;
    [Range(0, 100)] public int maxBubbles = 50;
    [Range(0.01f, 0.5f)] public float bubbleSize = 0.1f;
    [Range(0.1f, 5f)] public float bubbleRiseSpeed = 1f;
    public Color bubbleColor = new Color(1f, 1f, 1f, 0.3f);
    
    [Header("Splash System")]
    public bool enableSplash = true;
    public ParticleSystem splashParticleSystem;
    [Range(10, 200)] public int splashParticleCount = 50;
    [Range(0.5f, 10f)] public float splashForce = 5f;
    
    [Header("Dust Motes (Air)")]
    public bool enableDustMotes = true;
    public ParticleSystem dustMotesSystem;
    [Range(10, 500)] public int dustMoteCount = 100;
    [Range(0.001f, 0.05f)] public float dustMoteSize = 0.01f;
    [Range(0.1f, 2f)] public float dustMoteDriftSpeed = 0.5f;
    
    [Header("Underwater Particles")]
    public bool enableUnderwaterParticles = true;
    [Range(0, 200)] public int underwaterParticleCount = 50;
    public Color underwaterParticleColor = new Color(0.8f, 0.9f, 1f, 0.2f);
    
    [Header("Caustics Particles")]
    public bool enableCausticsParticles = true;
    public Material causticParticleMaterial;
    [Range(0.1f, 2f)] public float causticsParticleIntensity = 0.5f;
    
    void Start()
    {
        SetupBubbleSystem();
        SetupSplashSystem();
        SetupDustMotesSystem();
        SetupUnderwaterParticles();
    }
    
    void SetupBubbleSystem()
    {
        if (!enableBubbles) return;
        
        if (bubbleParticleSystem == null)
        {
            GameObject bubbleObj = new GameObject("BubbleSystem");
            bubbleObj.transform.parent = transform;
            bubbleParticleSystem = bubbleObj.AddComponent<ParticleSystem>();
        }
        
        var main = bubbleParticleSystem.main;
        main.startLifetime = 3f;
        main.startSpeed = bubbleRiseSpeed;
        main.startSize = bubbleSize;
        main.startColor = bubbleColor;
        main.maxParticles = maxBubbles;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.5f; // Bubbles rise
        
        var emission = bubbleParticleSystem.emission;
        emission.rateOverTime = 0; // Controlled manually
        
        var shape = bubbleParticleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        // Velocity over lifetime (bubbles wobble as they rise)
        var velocityOverLifetime = bubbleParticleSystem.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        
        AnimationCurve wobbleCurve = new AnimationCurve();
        wobbleCurve.AddKey(0f, 0f);
        wobbleCurve.AddKey(0.5f, 1f);
        wobbleCurve.AddKey(1f, 0f);
        
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(0.5f, wobbleCurve);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(0.5f, wobbleCurve);
        
        // Size over lifetime (bubbles expand as they rise)
        var sizeOverLifetime = bubbleParticleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        
        AnimationCurve sizeCurve = AnimationCurve.Linear(0f, 0.5f, 1f, 1.5f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Color over lifetime (fade out)
        var colorOverLifetime = bubbleParticleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.3f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
        
        // Renderer
        var renderer = bubbleParticleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateBubbleMaterial();
    }
    
    void SetupSplashSystem()
    {
        if (!enableSplash) return;
        
        if (splashParticleSystem == null)
        {
            GameObject splashObj = new GameObject("SplashSystem");
            splashObj.transform.parent = transform;
            splashParticleSystem = splashObj.AddComponent<ParticleSystem>();
        }
        
        var main = splashParticleSystem.main;
        main.startLifetime = 1f;
        main.startSpeed = splashForce;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
        main.startColor = new Color(0.8f, 0.9f, 1f, 0.7f);
        main.maxParticles = splashParticleCount;
        main.gravityModifier = 1f;
        
        var emission = splashParticleSystem.emission;
        emission.rateOverTime = 0; // Burst only
        
        var shape = splashParticleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 45f;
        shape.radius = 0.3f;
        
        // Collision with water surface
        var collision = splashParticleSystem.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        collision.dampen = 0.5f;
        collision.bounce = 0.3f;
    }
    
    void SetupDustMotesSystem()
    {
        if (!enableDustMotes) return;
        
        if (dustMotesSystem == null)
        {
            GameObject dustObj = new GameObject("DustMotesSystem");
            dustObj.transform.parent = transform;
            dustMotesSystem = dustObj.AddComponent<ParticleSystem>();
        }
        
        var main = dustMotesSystem.main;
        main.startLifetime = 10f;
        main.startSpeed = 0f;
        main.startSize = dustMoteSize;
        main.startColor = new Color(1f, 1f, 1f, 0.1f);
        main.maxParticles = dustMoteCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = dustMotesSystem.emission;
        emission.rateOverTime = dustMoteCount / 10f;
        
        var shape = dustMotesSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(30f, 10f, 30f);
        
        // Noise for drift
        var noise = dustMotesSystem.noise;
        noise.enabled = true;
        noise.strength = dustMoteDriftSpeed;
        noise.frequency = 0.5f;
        noise.scrollSpeed = 0.1f;
        noise.damping = true;
        
        // Renderer
        var renderer = dustMotesSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateDustMoteMaterial();
    }
    
    void SetupUnderwaterParticles()
    {
        if (!enableUnderwaterParticles) return;
        
        GameObject underwaterObj = new GameObject("UnderwaterParticles");
        underwaterObj.transform.parent = transform;
        ParticleSystem underwaterPS = underwaterObj.AddComponent<ParticleSystem>();
        
        var main = underwaterPS.main;
        main.startLifetime = 5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.1f);
        main.startColor = underwaterParticleColor;
        main.maxParticles = underwaterParticleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = underwaterPS.emission;
        emission.rateOverTime = underwaterParticleCount / 5f;
        
        var shape = underwaterPS.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(50f, 3f, 25f);
        
        // Slow drift
        var velocityOverLifetime = underwaterPS.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
        
        var renderer = underwaterPS.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateUnderwaterParticleMaterial();
    }
    
    Material CreateBubbleMaterial()
    {
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.SetColor("_Color", new Color(1f, 1f, 1f, 0.3f));
        mat.SetFloat("_Mode", 3); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        return mat;
    }
    
    Material CreateDustMoteMaterial()
    {
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.SetColor("_Color", new Color(1f, 1f, 1f, 0.05f));
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        return mat;
    }
    
    Material CreateUnderwaterParticleMaterial()
    {
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.SetColor("_Color", underwaterParticleColor);
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        return mat;
    }
    
    /// <summary>
    /// Emit bubbles at a specific position
    /// </summary>
    public void EmitBubbles(Vector3 position, int count = 10)
    {
        if (bubbleParticleSystem == null || !enableBubbles) return;
        
        bubbleParticleSystem.transform.position = position;
        bubbleParticleSystem.Emit(count);
    }
    
    /// <summary>
    /// Create splash effect at position
    /// </summary>
    public void CreateSplash(Vector3 position, float intensity = 1f)
    {
        if (splashParticleSystem == null || !enableSplash) return;
        
        splashParticleSystem.transform.position = position;
        
        var emission = splashParticleSystem.emission;
        ParticleSystem.Burst burst = new ParticleSystem.Burst(0f, (short)(splashParticleCount * intensity));
        emission.SetBurst(0, burst);
        
        splashParticleSystem.Play();
    }
}

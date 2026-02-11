using UnityEngine;

/// <summary>
/// Runtime charging logic for the underwater charging station.
/// Visual parts are created by the Editor tool (OceanSceneBuilder).
/// This script handles: beacon pulse, docking detection, battery charging.
/// ROV must LAND ON TOP of the station platform to charge.
/// </summary>
public class ChargingStation : MonoBehaviour
{
    [Header("Charging")]
    public float dockingHeight = 3.5f;     // how high above platform counts as docked
    public float dockingRadius = 3f;       // horizontal radius for docking
    public float chargeRate = 40f;         // battery units per second
    
    [Header("Beacon Light")]
    public float beaconIntensity = 4f;
    public Color beaconColor = new Color(1f, 0.15f, 0.1f);
    public float pulseSpeed = 1.5f;
    public float pulseMin = 0.3f;
    
    private Light beaconLight;
    private Transform rovTransform;
    private ROVHUD rovHUD;
    private bool isCharging = false;
    
    // Materials found from children
    private Material beaconMaterial;
    private Material padMaterial;
    private Material baseMaterial;
    
    void Start()
    {
        // Find ROV
        GameObject rov = GameObject.Find("ROV");
        if (rov != null)
        {
            rovTransform = rov.transform;
            rovHUD = rov.GetComponentInChildren<ROVHUD>();
            if (rovHUD == null)
                rovHUD = FindAnyObjectByType<ROVHUD>();
        }
        
        // Find beacon light from children
        Transform lightTr = transform.Find("BeaconLight");
        if (lightTr != null)
            beaconLight = lightTr.GetComponent<Light>();
        
        // Find materials from children renderers
        Transform beacon = transform.Find("Beacon");
        if (beacon != null)
            beaconMaterial = beacon.GetComponent<Renderer>()?.material;
        
        Transform pad = transform.Find("LandingPad");
        if (pad != null)
            padMaterial = pad.GetComponent<Renderer>()?.material;
        
        Transform baseTr = transform.Find("Base");
        if (baseTr != null)
            baseMaterial = baseTr.GetComponent<Renderer>()?.material;
        
        // Ensure we have the right name for sonar detection
        gameObject.name = "ChargingStation";
    }
    
    void Update()
    {
        // ── Pulse beacon ──
        float pulse = Mathf.Lerp(pulseMin, 1f, (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) / 2f);
        
        if (beaconLight != null)
            beaconLight.intensity = beaconIntensity * pulse;
        
        if (beaconMaterial != null)
            beaconMaterial.SetColor("_EmissionColor", beaconColor * (3f * pulse));
        
        // ── Check docking (ROV must be ABOVE the station within radius) ──
        isCharging = false;
        
        if (rovTransform != null)
        {
            Vector3 stationTop = transform.position + Vector3.up * 0.4f;
            Vector3 rovPos = rovTransform.position;
            
            float horizontalDist = Vector2.Distance(
                new Vector2(rovPos.x, rovPos.z),
                new Vector2(stationTop.x, stationTop.z)
            );
            
            float verticalOffset = rovPos.y - stationTop.y;
            
            bool inRadius = horizontalDist <= dockingRadius;
            bool abovePlatform = verticalOffset >= -0.5f && verticalOffset <= dockingHeight;
            
            if (inRadius && abovePlatform && rovHUD != null)
            {
                isCharging = true;
                rovHUD.AddBattery(chargeRate * Time.deltaTime);
                
                if (beaconLight != null)
                    beaconLight.intensity = beaconIntensity * 2f;
                
                if (padMaterial != null)
                {
                    Color chargeGlow = Color.Lerp(beaconColor * 0.5f, new Color(0, 1f, 0.3f) * 1.5f, pulse);
                    padMaterial.SetColor("_EmissionColor", chargeGlow);
                }
                
                if (baseMaterial != null)
                {
                    baseMaterial.color = Color.Lerp(
                        new Color(0.15f, 0.15f, 0.2f),
                        new Color(0.1f, 0.4f, 0.15f),
                        pulse * 0.5f
                    );
                }
            }
            else
            {
                if (padMaterial != null)
                    padMaterial.SetColor("_EmissionColor", beaconColor * 0.5f * pulse);
                
                if (baseMaterial != null)
                    baseMaterial.color = new Color(0.15f, 0.15f, 0.2f);
            }
        }
    }
    
    /// <summary>True if ROV is currently docked and charging</summary>
    public bool IsCharging => isCharging;
}

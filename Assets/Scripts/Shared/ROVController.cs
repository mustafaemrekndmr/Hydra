using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ROVController : MonoBehaviour
{
    [Header("Thruster Forces")]
    public float horizontalThrustForce = 15f;
    public float verticalThrustForce = 12f;
    public float rotationThrustForce = 8f;
    
    [Header("Stabilization")]
    public bool autoStabilize = true;
    public float stabilizationStrength = 10f;
    public float depthHoldStrength = 8f;
    public bool lockRoll = true;
    
    [Header("Limits")]
    public float maxSpeed = 3f;
    public float maxAngularSpeed = 1f;
    
    [Header("Camera")]
    public Transform cameraTransform;
    public float cameraTiltSpeed = 30f;
    public float minCameraTilt = -45f;
    public float maxCameraTilt = 45f;
    
    [Header("Debug")]
    public bool enableDebugLogs = false;
    
    private Rigidbody rb;
    private float targetDepth;
    private float currentCameraTilt = 0f;
    private bool depthHoldActive = false;
    private float waterSurfaceY = 10f;
    private ROVHUD rovHUD;
    
    /// <summary>True when battery is dead and thrusters are offline</summary>
    public bool IsPowerDead => rovHUD != null && rovHUD.IsBatteryDead;

    // Cached input values for FixedUpdate
    private float inputForward;
    private float inputStrafe;
    private float inputVertical;
    private float inputRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            // Ensure proper physics settings
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            if (enableDebugLogs)
                Debug.Log($"ROVController started: Mass={rb.mass}, Drag={rb.drag}");
        }
        else
        {
            Debug.LogError("ROV Rigidbody not found!");
        }
        
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cameraTransform = cam.transform;
        }
        
        // Auto-detect water surface level
        GameObject waterSurface = GameObject.Find("WaterSurface");
        if (waterSurface != null)
        {
            waterSurfaceY = waterSurface.transform.position.y;
            // Ensure WaterSurface collider is trigger so ROV passes through
            Collider wsCollider = waterSurface.GetComponent<Collider>();
            if (wsCollider != null)
                wsCollider.isTrigger = true;
        }
        
        targetDepth = transform.position.y;
        
        // Find HUD for battery check
        rovHUD = GetComponent<ROVHUD>();
        if (rovHUD == null)
            rovHUD = FindAnyObjectByType<ROVHUD>();
    }

    void Update()
    {
        HandleInput();
        HandleCameraControl();
    }
    
    void FixedUpdate()
    {
        if (rb == null) return;
        
        // Battery dead = no thruster power, but surface force still works
        if (IsPowerDead)
        {
            ApplyStabilization(); // Keep upright
            ApplySurfaceForce();  // Don't float to space
            LimitVelocity();
            return;
        }
        
        ApplyThrusters(inputForward, inputStrafe, inputVertical, inputRotation);
        ApplyStabilization();
        ApplySurfaceForce();
        LimitVelocity();
    }
    
    /// <summary>
    /// When ROV is above or at water surface, apply downward force to pull it back under.
    /// This prevents the ROV from getting stuck on the surface.
    /// </summary>
    void ApplySurfaceForce()
    {
        if (transform.position.y > waterSurfaceY - 0.5f)
        {
            // Above water: strong downward pull
            float overshoot = transform.position.y - (waterSurfaceY - 0.5f);
            float pullForce = overshoot * 15f + 5f;
            rb.AddForce(Vector3.down * pullForce);
            
            // Also dampen upward velocity
            if (rb.velocity.y > 0)
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.9f, rb.velocity.z);
        }
    }
    
    void HandleInput()
    {
        // Forward/Backward (W/S)
        inputForward = 0f;
        if (Input.GetKey(KeyCode.W)) inputForward = 1f;
        if (Input.GetKey(KeyCode.S)) inputForward = -1f;
        
        // Strafe Left/Right (A/D)
        inputStrafe = 0f;
        if (Input.GetKey(KeyCode.D)) inputStrafe = 1f;
        if (Input.GetKey(KeyCode.A)) inputStrafe = -1f;
        
        // Vertical Up/Down (Q/E)
        inputVertical = 0f;
        if (Input.GetKey(KeyCode.Q)) inputVertical = 1f;
        if (Input.GetKey(KeyCode.E)) inputVertical = -1f;
        
        // Rotation Left/Right (C/V)
        inputRotation = 0f;
        if (Input.GetKey(KeyCode.V)) inputRotation = 1f;
        if (Input.GetKey(KeyCode.C)) inputRotation = -1f;
        
        // Depth hold toggle (Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            depthHoldActive = !depthHoldActive;
            if (depthHoldActive)
                targetDepth = transform.position.y;
        }
    }
    
    void ApplyThrusters(float forward, float strafe, float vertical, float rotation)
    {
        // Horizontal thrusters
        Vector3 forwardForce = transform.forward * forward * horizontalThrustForce;
        Vector3 strafeForce = transform.right * strafe * horizontalThrustForce;
        Vector3 totalHorizontalForce = forwardForce + strafeForce;
        
        if (totalHorizontalForce.sqrMagnitude > 0.01f)
        {
            rb.AddForce(totalHorizontalForce);
        }
        
        // Vertical thrusters
        if (!depthHoldActive || Mathf.Abs(vertical) > 0.1f)
        {
            if (Mathf.Abs(vertical) > 0.1f)
            {
                rb.AddForce(Vector3.up * vertical * verticalThrustForce);
                depthHoldActive = false;
            }
        }
        
        // Rotation thrusters (yaw)
        if (Mathf.Abs(rotation) > 0.1f)
        {
            rb.AddTorque(Vector3.up * rotation * rotationThrustForce);
        }
    }
    
    void ApplyStabilization()
    {
        if (!autoStabilize) return;
        
        if (lockRoll)
        {
            Vector3 currentEuler = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, currentEuler.y, 0f);
        }
        else
        {
            Vector3 currentRotation = transform.rotation.eulerAngles;
            float pitch = NormalizeAngle(currentRotation.x);
            float roll = NormalizeAngle(currentRotation.z);
            rb.AddTorque(new Vector3(-pitch, 0f, -roll) * stabilizationStrength);
        }
        
        // Depth hold
        if (depthHoldActive)
        {
            float depthError = targetDepth - transform.position.y;
            rb.AddForce(Vector3.up * depthError * depthHoldStrength);
        }
    }
    
    void LimitVelocity()
    {
        if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        
        if (rb.angularVelocity.sqrMagnitude > maxAngularSpeed * maxAngularSpeed)
        {
            rb.angularVelocity = rb.angularVelocity.normalized * maxAngularSpeed;
        }
    }
    
    void HandleCameraControl()
    {
        if (cameraTransform == null) return;
        
        float tiltInput = 0f;
        if (Input.GetKey(KeyCode.UpArrow)) tiltInput = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) tiltInput = -1f;
        
        if (Mathf.Abs(tiltInput) > 0.01f)
        {
            currentCameraTilt += tiltInput * cameraTiltSpeed * Time.deltaTime;
            currentCameraTilt = Mathf.Clamp(currentCameraTilt, minCameraTilt, maxCameraTilt);
            cameraTransform.localRotation = Quaternion.Euler(currentCameraTilt, 0f, 0f);
        }
    }
    
    float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ROVController : MonoBehaviour
{
    [Header("Thruster Forces")]
    public float horizontalThrustForce = 15f;  // Daha yavaş hareket için azaltıldı
    public float verticalThrustForce = 12f;    // Daha yavaş dikey hareket
    public float rotationThrustForce = 8f;     // Daha yavaş dönüş
    
    [Header("Stabilization")]
    public bool autoStabilize = true;
    public float stabilizationStrength = 10f;  // Daha güçlü stabilizasyon
    public float depthHoldStrength = 8f;
    public bool lockRoll = true;               // Roll eksenini kilitle
    
    [Header("Limits")]
    public float maxSpeed = 3f;                // Daha gerçekçi maksimum hız
    public float maxAngularSpeed = 1f;         // Daha yavaş dönüş hızı
    
    [Header("Camera")]
    public Transform cameraTransform;
    public float cameraTiltSpeed = 30f;
    public float minCameraTilt = -45f;
    public float maxCameraTilt = 45f;
    
    private Rigidbody rb;
    private float targetDepth;
    private float currentCameraTilt = 0f;
    private bool depthHoldActive = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Rigidbody ayarları zaten BuildAdvancedIndoorPool'da yapılıyor
        // Burada sadece kontrol edelim
        if (rb != null)
        {
            Debug.Log($"ROVController baslatildi: UseGravity={rb.useGravity}, Drag={rb.drag}, Mass={rb.mass}");
        }
        else
        {
            Debug.LogError("ROV'da Rigidbody bulunamadi!");
        }
        
        if (cameraTransform == null)
        {
            // Try to find main camera as child
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cameraTransform = cam.transform;
        }
        
        targetDepth = transform.position.y;
    }

    void Update()
    {
        HandleInput();
        HandleCameraControl();
    }
    
    void FixedUpdate()
    {
        if (rb == null) return;
        
        ApplyStabilization();
        LimitVelocity();
    }
    
    void HandleInput()
    {
        // Forward/Backward (W/S)
        float forward = 0f;
        if (Input.GetKey(KeyCode.W)) forward = 1f;
        if (Input.GetKey(KeyCode.S)) forward = -1f;
        
        // Strafe Left/Right (A/D)
        float strafe = 0f;
        if (Input.GetKey(KeyCode.D)) strafe = 1f;
        if (Input.GetKey(KeyCode.A)) strafe = -1f;
        
        // Vertical Up/Down (Q/E)
        float vertical = 0f;
        if (Input.GetKey(KeyCode.Q)) vertical = 1f;
        if (Input.GetKey(KeyCode.E)) vertical = -1f;
        
        // Rotation Left/Right (C/V)
        float rotation = 0f;
        if (Input.GetKey(KeyCode.V)) rotation = 1f;
        if (Input.GetKey(KeyCode.C)) rotation = -1f;
        
        // Debug input
        if (forward != 0f || strafe != 0f || vertical != 0f || rotation != 0f)
        {
            Debug.Log($"ROV Input - Forward: {forward}, Strafe: {strafe}, Vertical: {vertical}, Rotation: {rotation}");
        }
        
        // Depth hold toggle (Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            depthHoldActive = !depthHoldActive;
            if (depthHoldActive)
                targetDepth = transform.position.y;
            Debug.Log($"Depth Hold: {depthHoldActive}");
        }
        
        // Apply thruster forces
        ApplyThrusters(forward, strafe, vertical, rotation);
    }
    
    void ApplyThrusters(float forward, float strafe, float vertical, float rotation)
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody is null!");
            return;
        }
        
        // Horizontal thrusters (forward/backward and strafe)
        Vector3 forwardForce = transform.forward * forward * horizontalThrustForce;
        Vector3 strafeForce = transform.right * strafe * horizontalThrustForce;
        Vector3 totalHorizontalForce = forwardForce + strafeForce;
        
        if (totalHorizontalForce.magnitude > 0.1f)
        {
            rb.AddForce(totalHorizontalForce);
            Debug.Log($"Applying horizontal force: {totalHorizontalForce}, Current velocity: {rb.velocity}");
        }
        
        // Vertical thrusters (up/down)
        if (!depthHoldActive || Mathf.Abs(vertical) > 0.1f)
        {
            Vector3 verticalForce = Vector3.up * vertical * verticalThrustForce;
            if (Mathf.Abs(vertical) > 0.1f)
            {
                rb.AddForce(verticalForce);
                Debug.Log($"Applying vertical force: {verticalForce}");
                depthHoldActive = false;
            }
        }
        
        // Rotation thrusters (yaw)
        if (Mathf.Abs(rotation) > 0.1f)
        {
            Vector3 torque = Vector3.up * rotation * rotationThrustForce;
            rb.AddTorque(torque);
            Debug.Log($"Applying torque: {torque}");
        }
    }
    
    void ApplyStabilization()
    {
        if (!autoStabilize) return;
        
        // Roll ve pitch'i sıfırla (sadece yaw serbest)
        if (lockRoll)
        {
            Vector3 currentEuler = transform.rotation.eulerAngles;
            float yaw = currentEuler.y;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
        else
        {
            // Auto-level the ROV (reduce roll and pitch)
            Vector3 currentRotation = transform.rotation.eulerAngles;
            float pitch = NormalizeAngle(currentRotation.x);
            float roll = NormalizeAngle(currentRotation.z);
            
            // Apply counter-torque to level out
            Vector3 stabilizationTorque = new Vector3(-pitch, 0f, -roll) * stabilizationStrength;
            rb.AddTorque(stabilizationTorque);
        }
        
        // Depth hold
        if (depthHoldActive)
        {
            float depthError = targetDepth - transform.position.y;
            float depthForce = depthError * depthHoldStrength;
            rb.AddForce(Vector3.up * depthForce);
        }
    }
    
    void LimitVelocity()
    {
        // Limit linear velocity
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        
        // Limit angular velocity
        if (rb.angularVelocity.magnitude > maxAngularSpeed)
        {
            rb.angularVelocity = rb.angularVelocity.normalized * maxAngularSpeed;
        }
    }
    
    void HandleCameraControl()
    {
        if (cameraTransform == null) return;
        
        // Camera tilt (Up/Down arrows)
        float tiltInput = 0f;
        if (Input.GetKey(KeyCode.UpArrow)) tiltInput = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) tiltInput = -1f;
        
        currentCameraTilt += tiltInput * cameraTiltSpeed * Time.deltaTime;
        currentCameraTilt = Mathf.Clamp(currentCameraTilt, minCameraTilt, maxCameraTilt);
        
        cameraTransform.localRotation = Quaternion.Euler(currentCameraTilt, 0f, 0f);
    }
    
    float NormalizeAngle(float angle)
    {
        // Convert angle to range -180 to 180
        if (angle > 180f)
            angle -= 360f;
        return angle;
    }
    
    void OnDrawGizmos()
    {
        // Visualize thruster directions
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * 2f); // Forward
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right * 2f); // Right
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.up * 2f); // Up
    }
}

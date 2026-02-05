using UnityEngine;

public class ROVDebugger : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 lastPosition;
    private float checkInterval = 1f;
    private float nextCheckTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;
        
        if (rb == null)
        {
            Debug.LogError("ROVDebugger: Rigidbody not found!");
        }
        else
        {
            Debug.Log($"ROVDebugger Started - Position: {transform.position}");
            Debug.Log($"Rigidbody Settings - UseGravity: {rb.useGravity}, IsKinematic: {rb.isKinematic}, Mass: {rb.mass}, Drag: {rb.drag}");
            Debug.Log($"Constraints: {rb.constraints}");
        }
    }

    void Update()
    {
        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;
            
            Vector3 movement = transform.position - lastPosition;
            float distance = movement.magnitude;
            
            if (rb != null)
            {
                Debug.Log($"=== ROV Status ===");
                Debug.Log($"Position: {transform.position}");
                Debug.Log($"Movement (last {checkInterval}s): {distance:F3}m");
                Debug.Log($"Velocity: {rb.velocity} (magnitude: {rb.velocity.magnitude:F3})");
                Debug.Log($"Angular Velocity: {rb.angularVelocity}");
                Debug.Log($"Time.timeScale: {Time.timeScale}");
                Debug.Log($"==================");
            }
            
            lastPosition = transform.position;
        }
        
        // Test tuşları
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("TEST: Applying upward force!");
            if (rb != null)
                rb.AddForce(Vector3.up * 100f);
        }
        
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("TEST: Applying forward force!");
            if (rb != null)
                rb.AddForce(transform.forward * 100f);
        }
    }
}

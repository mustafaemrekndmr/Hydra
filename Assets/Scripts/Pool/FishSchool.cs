using UnityEngine;
using System.Collections.Generic;

public class FishSchool : MonoBehaviour
{
    [Header("School Settings")]
    public int fishCount = 20;
    public float spawnRadius = 5f;
    
    [Header("Fish Appearance")]
    public float fishLength = 0.3f;
    public float fishWidth = 0.1f;
    public Color fishColor = new Color(0.7f, 0.7f, 0.9f);
    
    [Header("Boid Behavior")]
    public float swimSpeed = 2f;
    public float rotationSpeed = 3f;
    public float neighborRadius = 2f;
    public float separationRadius = 1f;
    
    [Header("Boid Weights")]
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1f;
    public float cohesionWeight = 1f;
    public float avoidanceWeight = 2f;
    
    [Header("Boundaries")]
    public Vector3 boundaryCenter = Vector3.zero;
    public Vector3 boundarySize = new Vector3(20f, 10f, 20f);
    
    private List<Fish> fishes = new List<Fish>();
    
    class Fish
    {
        public GameObject gameObject;
        public Vector3 velocity;
        public Transform transform;
    }
    
    void Start()
    {
        GenerateSchool();
    }
    
    void GenerateSchool()
    {
        for (int i = 0; i < fishCount; i++)
        {
            GameObject fishObj = CreateFish("Fish_" + i);
            
            // Random position within spawn radius
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            fishObj.transform.position = randomPos;
            
            // Random initial velocity
            Vector3 randomVelocity = Random.onUnitSphere * swimSpeed;
            
            Fish fish = new Fish
            {
                gameObject = fishObj,
                transform = fishObj.transform,
                velocity = randomVelocity
            };
            
            fishes.Add(fish);
        }
    }
    
    GameObject CreateFish(string name)
    {
        GameObject fishObj = new GameObject(name);
        fishObj.transform.parent = transform;
        
        // Body (main capsule)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.transform.parent = fishObj.transform;
        body.transform.localPosition = Vector3.zero;
        body.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        body.transform.localScale = new Vector3(fishWidth, fishLength * 0.5f, fishWidth);
        
        // Tail (small pyramid-like shape using cube)
        GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tail.transform.parent = fishObj.transform;
        tail.transform.localPosition = new Vector3(-fishLength * 0.7f, 0f, 0f);
        tail.transform.localScale = new Vector3(fishLength * 0.3f, fishWidth * 0.5f, fishWidth * 1.5f);
        
        // Apply color
        body.GetComponent<Renderer>().material.color = fishColor;
        tail.GetComponent<Renderer>().material.color = fishColor;
        
        // Remove colliders for performance
        Destroy(body.GetComponent<Collider>());
        Destroy(tail.GetComponent<Collider>());
        
        return fishObj;
    }
    
    void Update()
    {
        foreach (Fish fish in fishes)
        {
            if (fish == null || fish.gameObject == null) continue;
            
            Vector3 separation = CalculateSeparation(fish);
            Vector3 alignment = CalculateAlignment(fish);
            Vector3 cohesion = CalculateCohesion(fish);
            Vector3 avoidance = CalculateBoundaryAvoidance(fish);
            
            // Combine all forces
            Vector3 acceleration = separation * separationWeight +
                                   alignment * alignmentWeight +
                                   cohesion * cohesionWeight +
                                   avoidance * avoidanceWeight;
            
            fish.velocity += acceleration * Time.deltaTime;
            fish.velocity = fish.velocity.normalized * swimSpeed;
            
            // Update position
            fish.transform.position += fish.velocity * Time.deltaTime;
            
            // Update rotation to face velocity direction
            if (fish.velocity.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(fish.velocity);
                fish.transform.rotation = Quaternion.Slerp(fish.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    Vector3 CalculateSeparation(Fish fish)
    {
        Vector3 separationForce = Vector3.zero;
        int count = 0;
        
        foreach (Fish other in fishes)
        {
            if (other == fish || other.gameObject == null) continue;
            
            float distance = Vector3.Distance(fish.transform.position, other.transform.position);
            
            if (distance < separationRadius && distance > 0)
            {
                Vector3 diff = fish.transform.position - other.transform.position;
                separationForce += diff.normalized / distance; // Closer = stronger
                count++;
            }
        }
        
        if (count > 0)
            separationForce /= count;
        
        return separationForce;
    }
    
    Vector3 CalculateAlignment(Fish fish)
    {
        Vector3 averageVelocity = Vector3.zero;
        int count = 0;
        
        foreach (Fish other in fishes)
        {
            if (other == fish || other.gameObject == null) continue;
            
            float distance = Vector3.Distance(fish.transform.position, other.transform.position);
            
            if (distance < neighborRadius)
            {
                averageVelocity += other.velocity;
                count++;
            }
        }
        
        if (count > 0)
        {
            averageVelocity /= count;
            return (averageVelocity - fish.velocity).normalized;
        }
        
        return Vector3.zero;
    }
    
    Vector3 CalculateCohesion(Fish fish)
    {
        Vector3 centerOfMass = Vector3.zero;
        int count = 0;
        
        foreach (Fish other in fishes)
        {
            if (other == fish || other.gameObject == null) continue;
            
            float distance = Vector3.Distance(fish.transform.position, other.transform.position);
            
            if (distance < neighborRadius)
            {
                centerOfMass += other.transform.position;
                count++;
            }
        }
        
        if (count > 0)
        {
            centerOfMass /= count;
            return (centerOfMass - fish.transform.position).normalized;
        }
        
        return Vector3.zero;
    }
    
    Vector3 CalculateBoundaryAvoidance(Fish fish)
    {
        Vector3 avoidanceForce = Vector3.zero;
        Vector3 relativePos = fish.transform.position - boundaryCenter;
        
        // Check each axis
        if (Mathf.Abs(relativePos.x) > boundarySize.x * 0.5f)
            avoidanceForce.x = -Mathf.Sign(relativePos.x);
        
        if (Mathf.Abs(relativePos.y) > boundarySize.y * 0.5f)
            avoidanceForce.y = -Mathf.Sign(relativePos.y);
        
        if (Mathf.Abs(relativePos.z) > boundarySize.z * 0.5f)
            avoidanceForce.z = -Mathf.Sign(relativePos.z);
        
        return avoidanceForce;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw boundary
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boundaryCenter, boundarySize);
        
        // Draw spawn radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}

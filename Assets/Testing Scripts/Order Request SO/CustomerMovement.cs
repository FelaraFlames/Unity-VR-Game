using UnityEngine;
using UnityEngine.AI;

public class CustomerMovement : MonoBehaviour
{
    // The NavMeshAgent component for pathfinding.
    [SerializeField] private NavMeshAgent agent;
    
    // The counter location where the customer will place their order.
    [SerializeField] private Transform counterLocation;
    
    // Optional separate look target (e.g., the Store direction). If not set, uses counterLocation.
    [SerializeField] private Transform storeLookTarget;
    
    // Reference to the Customer component on this GameObject.
    [SerializeField] private Customer customer;
    
    // The distance threshold to consider the counter "reached".
    [SerializeField] private float arrivalThreshold = 0.5f;

    [SerializeField] private float rotationSpeed = 5f;

    private bool hasArrivedAtCounter = false;
    private bool isGoingToDespawn = false;
    private bool hasArrivedAtDespawn = false;
    private bool shouldFaceStore = false;

    private void Start()
    {
        // Validate all references
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                Debug.LogError("CustomerMovement: NavMeshAgent not found!", gameObject);
                return;
            }
        }

        // If counterLocation wasn't assigned directly, try to use the one from the Customer component.
        if (counterLocation == null && customer != null)
        {
            counterLocation = customer.counterLocation;
        }

        if (counterLocation == null)
        {
            Debug.LogError("CustomerMovement: counterLocation is not assigned!", gameObject);
            return;
        }

        if (customer == null)
        {
            customer = GetComponent<Customer>();
            if (customer == null)
            {
                Debug.LogError("CustomerMovement: Customer component not found!", gameObject);
                return;
            }
        }

        // Begin navigation to the counter
        agent.SetDestination(counterLocation.position);
        Debug.Log("Customer starting navigation to counter.");
        if (customer != null && customer.animator != null)
        {
            customer.animator.SetBool("IsWalking", true);
        }
    }

    private void Update()
    {
        // Check if customer has reached the counter
        if (!hasArrivedAtCounter && agent != null && !agent.pathPending)
        {
            // Only treat as arrived when close enough AND the agent has effectively stopped.
            if (agent.remainingDistance <= arrivalThreshold &&
                (!agent.hasPath || agent.velocity.sqrMagnitude <= 0.01f))
            {
                hasArrivedAtCounter = true;
                Debug.Log("Customer has arrived at the counter!");
                
                // Notify the customer to display their order
                if (customer != null)
                {
                    customer.ArriveAtCounter();
                    agent.updateRotation = false; // Disable automatic rotation
                    shouldFaceStore = true;       // Start smoothly rotating towards the store

                    if (customer.animator != null)
                    {
                        customer.animator.SetBool("IsWalking", false);
                    }
                }
                
                // Notify the serving zone that a customer is now available
                RegisterWithServingZone();
            }
        }

        // Check if customer has reached the despawn point when walking away
        if (isGoingToDespawn && !hasArrivedAtDespawn && agent != null && !agent.pathPending)
        {
            if (agent.remainingDistance <= arrivalThreshold && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f))
            {
                hasArrivedAtDespawn = true;
                Debug.Log("Customer has reached the despawn point.");

                if (customer != null)
                {
                    customer.OnReachedDespawn();
                }

                // Teleport back to spawn and start the cycle again
                if (customer != null && customer.spawnPoint != null)
                {
                    agent.Warp(customer.spawnPoint.position);
                }

                // Reset state and walk back to the counter
                hasArrivedAtCounter = false;
                isGoingToDespawn = false;

                agent.updateRotation = true;
                if (counterLocation != null)
                {
                    agent.SetDestination(counterLocation.position);
                }

                if (customer != null && customer.animator != null)
                {
                    customer.animator.SetBool("IsWalking", true);
                }
            }
        }

        // Handle smooth rotation towards the store direction while at the counter.
        if (shouldFaceStore)
        {
            Transform target = storeLookTarget != null ? storeLookTarget : counterLocation;
            if (target != null)
            {
                FaceTarget(target.position);

                // Stop rotating once we're roughly facing the target (within a few degrees).
                Vector3 toTarget = (target.position - transform.position);
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.001f)
                {
                    float angle = Vector3.Angle(transform.forward, toTarget.normalized);
                    if (angle <= 2f)
                    {
                        shouldFaceStore = false;
                    }
                }
                else
                {
                    shouldFaceStore = false;
                }
            }
            else
            {
                shouldFaceStore = false;
            }
        }
    }

    // Finds and registers this customer with the serving zone.
    private void RegisterWithServingZone()
    {
        ServingZone servingZone = FindObjectOfType<ServingZone>();
        
        if (servingZone == null)
        {
            Debug.LogWarning("ServingZone not found in the scene!");
            return;
        }

        servingZone.SetCurrentCustomer(customer);
        Debug.Log("Customer registered with serving zone.");
    }

    // Called by Customer when it starts walking to the despawn point.
    public void OnStartGoingToDespawn()
    {
        isGoingToDespawn = true;
        hasArrivedAtDespawn = false;
    }

    void FaceTarget(Vector3 destination)
    {
        Vector3 direction = (destination - transform.position).normalized;
        // Keep the rotation in the XZ plane (ignore Y component for a flat rotation)
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        
        // Smoothly rotate the agent towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
}
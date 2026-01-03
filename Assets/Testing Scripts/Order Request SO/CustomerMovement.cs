using UnityEngine;
using UnityEngine.AI;

public class CustomerMovement : MonoBehaviour
{
    // The NavMeshAgent component for pathfinding.
    [SerializeField] private NavMeshAgent agent;
    
    // The counter location where the customer will place their order.
    [SerializeField] private Transform counterLocation;
    
    // Reference to the Customer component on this GameObject.
    [SerializeField] private Customer customer;
    
    // The distance threshold to consider the counter "reached".
    [SerializeField] private float counterArrivalThreshold = 0.5f;
    [SerializeField] private float arrivalThreshold = 0.5f;

    [SerializeField] private float rotationSpeed = 5f;

    private bool hasArrivedAtCounter = false;

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
    }

    private void Update()
    {
        // Check if customer has reached the counter
        if (!hasArrivedAtCounter && agent != null && !agent.pathPending)
        {
            if (agent.remainingDistance <= arrivalThreshold && !agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                hasArrivedAtCounter = true;
                Debug.Log("Customer has arrived at the counter!");
                
                // Notify the customer to display their order
                if (customer != null)
                {
                    customer.ArriveAtCounter();
                    agent.updateRotation = false; // Disable automatic rotation
                    FaceTarget(counterLocation.position);
                }
                
                // Notify the serving zone that a customer is now available
                RegisterWithServingZone();
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

    void FaceTarget(Vector3 destination)
    {
        Vector3 direction = (destination - transform.position).normalized;
        // Keep the rotation in the XZ plane (ignore Y component for a flat rotation)
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        
        // Smoothly rotate the agent towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
}
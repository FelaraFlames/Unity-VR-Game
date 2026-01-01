using UnityEngine;
using UnityEngine.AI;

public class CustomerMovement : MonoBehaviour
{
    /// <summary>
    /// The NavMeshAgent component for pathfinding.
    /// </summary>
    [SerializeField] private NavMeshAgent agent;
    
    /// <summary>
    /// The counter location where the customer will place their order.
    /// </summary>
    [SerializeField] private Transform counterLocation;
    
    /// <summary>
    /// Reference to the Customer component on this GameObject.
    /// </summary>
    [SerializeField] private Customer customer;
    
    /// <summary>
    /// The distance threshold to consider the counter "reached".
    /// </summary>
    [SerializeField] private float arrivalThreshold = 0.5f;

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
                }
                
                // Notify the serving zone that a customer is now available
                RegisterWithServingZone();
            }
        }
    }

    /// <summary>
    /// Finds and registers this customer with the serving zone.
    /// </summary>
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
}
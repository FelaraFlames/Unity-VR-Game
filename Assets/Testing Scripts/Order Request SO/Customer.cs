using UnityEngine;
using TMPro;
using UnityEngine.AI;

public class Customer : MonoBehaviour
{
    [Header("Order Data")]
    public int skewersNeeded;
    public int skewersGiven;
    public int teaNeeded;
    public int teaGiven;

    [Header("UI Reference")]
    /// <summary>
    /// The world space UI panel containing the order display.
    /// Will be enabled at counter and disabled when order is complete.
    /// </summary>
    public GameObject orderUIPanel;
    public TextMeshProUGUI orderText;

    [Header("Navigation Points")]
    /// <summary>
    /// The spawn point where the customer prefab is instantiated.
    /// </summary>
    public Transform spawnPoint;
    
    /// <summary>
    /// The counter location where orders are displayed and food is delivered.
    /// </summary>
    public Transform counterLocation;
    
    /// <summary>
    /// The despawn point where the customer walks after order completion.
    /// </summary>
    public Transform despawnPoint;

    private NavMeshAgent agent;
    private bool orderComplete = false;

    private void Start()
    {
        // Validate references
        if (orderUIPanel == null)
        {
            Debug.LogError("Customer: orderUIPanel is not assigned!", gameObject);
        }
        if (orderText == null)
        {
            Debug.LogError("Customer: orderText is not assigned!", gameObject);
        }
        if (counterLocation == null)
        {
            Debug.LogError("Customer: counterLocation is not assigned!", gameObject);
        }
        if (despawnPoint == null)
        {
            Debug.LogError("Customer: despawnPoint is not assigned!", gameObject);
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("Customer: NavMeshAgent not found on this GameObject!", gameObject);
        }

        // Ensure UI is hidden at start
        if (orderUIPanel != null)
        {
            orderUIPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Called when the customer arrives at the counter.
    /// Generates a random order and enables the UI.
    /// </summary>
    public void ArriveAtCounter()
    {
        if (orderComplete)
        {
            Debug.LogWarning("Customer has already completed their order!");
            return;
        }

        GenerateRandomOrder();

        // Enable the order UI panel
        if (orderUIPanel != null)
        {
            orderUIPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Generates a randomized order for the customer.
    /// </summary>
    private void GenerateRandomOrder()
    {
        skewersNeeded = Random.Range(1, 4); // Random 1 to 3
        teaNeeded = Random.Range(1, 3);     // Random 1 to 2
        
        skewersGiven = 0;
        teaGiven = 0;

        UpdateOrderUI();
        Debug.Log($"Customer order generated: {skewersNeeded} skewers, {teaNeeded} tea");
    }

    /// <summary>
    /// Called by ServingZone when food is delivered to this customer.
    /// Only accepts CookedSkewer and Tea items.
    /// </summary>
    public void ReceiveFood(Order_ItemType itemType)
    {
        if (orderComplete)
        {
            Debug.LogWarning("Order already complete! Ignoring delivered item.");
            return;
        }

        // Only accept cooked skewers, reject uncooked ones
        if (itemType == Order_ItemType.CookedSkwewer)
        {
            skewersGiven++;
            Debug.Log($"Received CookedSkewer. Count: {skewersGiven}/{skewersNeeded}");
        }
        else if (itemType == Order_ItemType.Tea)
        {
            teaGiven++;
            Debug.Log($"Received Tea. Count: {teaGiven}/{teaNeeded}");
        }
        else if (itemType == Order_ItemType.UncookedSkwewer)
        {
            // Reject uncooked skewers
            Debug.LogWarning("Uncooked skewer rejected! Customer needs it to be cooked.");
            return;
        }
        
        UpdateOrderUI();
        CheckIfSatisfied();
    }

    /// <summary>
    /// Updates the UI text to show current order progress.
    /// </summary>
    private void UpdateOrderUI()
    {
        if (orderText != null)
        {
            orderText.text = $"Skewers: {skewersGiven}/{skewersNeeded}\nTea: {teaGiven}/{teaNeeded}";
        }
    }

    /// <summary>
    /// Checks if the customer's order is satisfied and triggers departure.
    /// </summary>
    private void CheckIfSatisfied()
    {
        if (skewersGiven >= skewersNeeded && teaGiven >= teaNeeded)
        {
            orderComplete = true;
            Debug.Log("Order Complete! Customer walking to despawn point.");
            LeaveCounter();
        }
    }

    /// <summary>
    /// Initiates the customer's departure: hides UI and walks to despawn point.
    /// </summary>
    private void LeaveCounter()
    {
        // Disable the order UI panel
        if (orderUIPanel != null)
        {
            orderUIPanel.SetActive(false);
        }

        // Move to despawn point if NavMeshAgent is available
        if (agent != null && despawnPoint != null)
        {
            agent.SetDestination(despawnPoint.position);
            Debug.Log("Customer navigating to despawn point.");
        }
    }
}
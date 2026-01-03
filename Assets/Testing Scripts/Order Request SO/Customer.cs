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
    // The world space UI panel containing the order display.
    // Will be enabled at counter and disabled when order is complete.
    public GameObject orderUIPanel;
    public TextMeshProUGUI orderText;

    [Header("Navigation Points")]
    // The spawn point where the customer prefab is instantiated.
    public Transform spawnPoint;
    
    // The counter location where orders are displayed and food is delivered.
    public Transform counterLocation;
    
    // The despawn point where the customer walks after order completion.
    public Transform despawnPoint;

    [Header("Animation & Movement")]
    public Animator animator;

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

        animator.SetBool("IsWalking", true);
    }

    // Called when the customer arrives at the counter.
    // Generates a random order and enables the UI.
    public void ArriveAtCounter()
    {
        animator.SetBool("IsWalking", false);
        
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

    #region Order Management
    // Generates a randomized order for the customer.
    private void GenerateRandomOrder()
    {
        skewersNeeded = Random.Range(2, 4); // Random 2 to 3
        teaNeeded = Random.Range(1, 3);     // Random 1 to 2
        
        skewersGiven = 0;
        teaGiven = 0;

        UpdateOrderUI();
        Debug.Log($"Customer order generated: {skewersNeeded} skewers, {teaNeeded} tea");
    }
    #endregion

    // Called by ServingZone when food is delivered to this customer.
    // Only accepts CookedSkewer and Tea items.
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

    // Updates the UI text to show current order progress.
    private void UpdateOrderUI()
    {
        if (orderText != null)
        {
            orderText.text = $"Skewers: {skewersGiven}/{skewersNeeded}\nTea: {teaGiven}/{teaNeeded}";
        }
    }

    // Checks if the customer's order is satisfied and triggers departure.
    private void CheckIfSatisfied()
    {
        if (skewersGiven >= skewersNeeded && teaGiven >= teaNeeded)
        {
            orderComplete = true;
            Debug.Log("Order Complete! Customer walking to despawn point.");
            LeaveCounter();
        }
    }

    // Initiates the customer's departure: hides UI and walks to despawn point.
    private void LeaveCounter()
    {
        agent.updateRotation = true;
        animator.SetBool("IsWalking", true);
        
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
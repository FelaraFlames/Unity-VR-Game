using UnityEngine;

public class ServingZone : MonoBehaviour
{
    /// <summary>
    /// The current customer at the counter waiting for their order.
    /// </summary>
    private Customer currentCustomer;

    /// <summary>
    /// Sets the current customer at this serving zone.
    /// Called by CustomerMovement when a customer arrives at the counter.
    /// </summary>
    public void SetCurrentCustomer(Customer customer)
    {
        if (customer == null)
        {
            Debug.LogWarning("ServingZone: Attempted to set null customer!");
            return;
        }

        currentCustomer = customer;
        Debug.Log($"ServingZone: Customer {customer.gameObject.name} is now being served.");
    }

    /// <summary>
    /// Clears the current customer (called when they leave).
    /// </summary>
    public void ClearCurrentCustomer()
    {
        if (currentCustomer != null)
        {
            Debug.Log($"ServingZone: Customer {currentCustomer.gameObject.name} has left.");
        }
        currentCustomer = null;
    }

    /// <summary>
    /// Called when a food item enters the serving zone trigger.
    /// Validates the item and delivers it to the current customer.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 1. Check if we have a customer to serve
        if (currentCustomer == null)
        {
            Debug.LogWarning("ServingZone: Food entered but no customer is being served!");
            return;
        }

        // 2. Check if the object is food with an ItemIdentity component
        ItemIdentity item = other.GetComponent<ItemIdentity>();

        if (item == null)
        {
            Debug.LogWarning($"ServingZone: {other.gameObject.name} entered but has no ItemIdentity component!");
            return;
        }

        // 3. Validate the item type before accepting it
        if (item.type == Order_ItemType.UncookedSkwewer)
        {
            Debug.LogWarning($"ServingZone: Uncooked skewer attempted to be served! Rejecting.");
            return;
        }

        // 4. Deliver the food to the customer
        currentCustomer.ReceiveFood(item.type);
        Debug.Log($"ServingZone: Delivered {item.type} to customer");

        // 5. Destroy the food object to clean up the counter
        Destroy(other.gameObject);
    }
}
using UnityEngine;

public class ItemIdentity : MonoBehaviour
{
    /// <summary>
    /// Tracks the current type of the food item.
    /// Can change during cooking (e.g., UncookedSkewer -> CookedSkewer)
    /// </summary>
    public Order_ItemType type;

    /// <summary>
    /// Called by the cooking system when the item transitions from uncooked to cooked.
    /// Updates the ItemType and handles visual state changes.
    /// </summary>
    public void SetItemAsCooked()
    {
        if (type == Order_ItemType.UncookedSkwewer)
        {
            type = Order_ItemType.CookedSkwewer;
            Debug.Log("Item state updated to CookedSkewer");
        }
    }
}
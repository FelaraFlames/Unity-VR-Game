using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct OrderItem
{
    public Order_ItemType itemType;
    public int quantity;
    public Sprite icon; //Thought bubble UI
}

[CreateAssetMenu(fileName = "NewOrder", menuName = "Scriptable Objects/Order Data")]
public class OrderData_SO : ScriptableObject
{
    public string orderName;
    public List<OrderItem> itemsRequired;

    // Helper method to get the requirement for a specific type
    public int GetRequiredQuantity(Order_ItemType type) {
        foreach (var item in itemsRequired) {
            if (item.itemType == type) return item.quantity;
        }
        return 0;
    }
}
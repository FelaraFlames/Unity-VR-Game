# Cooking System Integration - Code Snippets

This file provides code examples for integrating the order system with your existing cooking/food preparation systems.

---

## 1. Update YakitoriSkewer Cooking Completion

When a skewer finishes cooking, you need to update its ItemIdentity state.

### Option A: Add to YakitoriSkewer.cs

```csharp
// In your YakitoriSkewer.cs, after the skewer becomes cooked:

private void OnCookingComplete()
{
    // Disable cooking meshes, enable cooked mesh
    if (side1Mesh != null) side1Mesh.SetActive(false);
    if (side2Mesh != null) side2Mesh.SetActive(false);
    if (cookedMesh != null) cookedMesh.SetActive(true);
    
    // ← ADD THIS: Update ItemIdentity state
    ItemIdentity itemIdentity = GetComponent<ItemIdentity>();
    if (itemIdentity != null)
    {
        itemIdentity.SetItemAsCooked();
        Debug.Log("Skewer is now ready for serving!");
    }
}
```

### Option B: Call from CheckIfSatisfied() equivalent

In your cooking coroutine, after determining the skewer is cooked:

```csharp
IEnumerator CookSideRoutine(int side)
{
    // ... existing cooking logic ...
    
    while (cookProgress < targetCookProgress && !isBurned)
    {
        // ... update progress ...
        yield return new WaitForSeconds(0.1f);
    }
    
    // When finished cooking:
    if (cookProgress >= targetCookProgress)
    {
        // Update visual state
        ShowCookedMesh();
        
        // ← ADD THIS: Update order system state
        ItemIdentity itemIdentity = GetComponent<ItemIdentity>();
        if (itemIdentity != null)
        {
            itemIdentity.SetItemAsCooked();
        }
    }
}
```

---

## 2. Food Prefab Creation Checklist

### For Uncooked Skewers

```
UncookedSkewer Prefab
├── Mesh (Visual)
├── Collider (Not trigger)
├── Rigidbody (optional, if physics needed)
├── ItemIdentity
│   └── type = Order_ItemType.UncookedSkwewer
└── YakitoriSkewer (or your cooking script)
```

**Inspector Setup:**
- ItemIdentity type: `UncookedSkwewer`
- Ensure it has a non-trigger collider for detection
- Make it grabbable with XR Interaction Toolkit

### For Cooked Skewers

Same as uncooked, but the `ItemIdentity.type` is set to `CookedSkwewer` **after cooking completes**.

### For Tea

```
Tea Prefab
├── Mesh (Visual - Cup/Glass)
├── Collider (Not trigger)
├── Rigidbody (optional)
└── ItemIdentity
    └── type = Order_ItemType.Tea
```

**Note:** Tea doesn't need cooking logic, just ensure ItemIdentity is set to `Tea` before using it.

---

## 3. Example: Complete Cooking System Integration

Here's a complete example showing how to integrate ItemIdentity updates with a cooking system:

```csharp
using UnityEngine;

public class FoodItem : MonoBehaviour
{
    [SerializeField] private GameObject uncookedMesh;
    [SerializeField] private GameObject cookedMesh;
    [SerializeField] private float cookingTimeRequired = 10f;
    
    private ItemIdentity itemIdentity;
    private bool isCooked = false;
    private float cookingProgress = 0f;
    
    private void Start()
    {
        itemIdentity = GetComponent<ItemIdentity>();
        if (itemIdentity == null)
        {
            Debug.LogError("FoodItem: ItemIdentity component not found!");
        }
    }
    
    /// <summary>
    /// Called when food enters the grill/cooking zone
    /// </summary>
    public void StartCooking()
    {
        if (isCooked)
        {
            Debug.LogWarning("Food is already cooked!");
            return;
        }
        
        StartCoroutine(CookRoutine());
    }
    
    private System.Collections.IEnumerator CookRoutine()
    {
        cookingProgress = 0f;
        
        while (cookingProgress < cookingTimeRequired && !isCooked)
        {
            cookingProgress += Time.deltaTime;
            
            // Optional: Visual feedback (material color change, etc.)
            UpdateVisuals();
            
            yield return null;
        }
        
        // Cooking complete!
        isCooked = true;
        FinishCooking();
    }
    
    private void FinishCooking()
    {
        // Update visuals
        if (uncookedMesh != null) uncookedMesh.SetActive(false);
        if (cookedMesh != null) cookedMesh.SetActive(true);
        
        // ← CRITICAL: Update the order system
        if (itemIdentity != null)
        {
            itemIdentity.SetItemAsCooked();
            Debug.Log($"Food {gameObject.name} is now cooked and ready for serving!");
        }
    }
    
    private void UpdateVisuals()
    {
        // Add visual feedback based on cooking progress
        // e.g., change material color from raw to cooked
    }
}
```

---

## 4. Integration with GrillTrigger

If you want to automatically handle cooking when food enters the grill:

```csharp
// In GrillTrigger.cs

using UnityEngine;

public class GrillTrigger : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        // Try to get a food item with cooking capability
        FoodItem foodItem = other.GetComponent<FoodItem>();
        
        if (foodItem != null)
        {
            // Start cooking if not already cooking
            foodItem.StartCooking();
        }
    }
}
```

---

## 5. Player Feedback on Cooking Status

Add feedback so players know when food is ready:

```csharp
using UnityEngine;
using TMPro;

public class FoodItem : MonoBehaviour
{
    private ItemIdentity itemIdentity;
    private TextMeshPro statusDisplay; // Optional UI
    
    private void FinishCooking()
    {
        // Update visuals
        ShowCookedMesh();
        
        // Update order system
        if (itemIdentity != null)
        {
            itemIdentity.SetItemAsCooked();
        }
        
        // Player feedback
        PlayCookingCompleteSound();
        ShowReadyParticles();
        
        if (statusDisplay != null)
        {
            statusDisplay.text = "READY TO SERVE!";
            statusDisplay.color = Color.green;
        }
    }
    
    private void PlayCookingCompleteSound()
    {
        // Play a "ding" sound to notify player
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null)
        {
            audio.PlayOneShot(/* cooking complete clip */);
        }
    }
    
    private void ShowReadyParticles()
    {
        // Spawn particle effect above food to indicate it's ready
        ParticleSystem particles = GetComponentInChildren<ParticleSystem>();
        if (particles != null)
        {
            particles.Play();
        }
    }
}
```

---

## 6. Preventing Uncooked Food Delivery

The serving zone already rejects uncooked skewers, but you can add extra validation:

```csharp
// In ServingZone.cs (already implemented, but shown for reference)

private void OnTriggerEnter(Collider other)
{
    if (currentCustomer == null) return;
    
    ItemIdentity item = other.GetComponent<ItemIdentity>();
    if (item == null) return;
    
    // Reject uncooked food
    if (item.type == Order_ItemType.UncookedSkwewer)
    {
        Debug.LogWarning("Uncooked skewer rejected! Player must cook it first.");
        
        // Optional: Add visual/audio feedback
        PlayRejectionSound();
        ShowRejectionParticles(other.transform.position);
        
        return; // Do NOT destroy the food, let player try again
    }
    
    // Accept cooked food
    currentCustomer.ReceiveFood(item.type);
    Destroy(other.gameObject);
}

private void PlayRejectionSound()
{
    // Play buzzer/rejection sound
}

private void ShowRejectionParticles(Vector3 position)
{
    // Show red X or rejection particles at the serving zone
}
```

---

## 7. Quick Integration Checklist

- [ ] Add `ItemIdentity` component to all food prefabs
- [ ] Set correct `Order_ItemType` for each food
- [ ] In your cooking system, call `itemIdentity.SetItemAsCooked()` when cooking completes
- [ ] Verify uncooked items have colliders (not triggers)
- [ ] Verify cooked items have colliders (not triggers)
- [ ] Test that cooked items are accepted at the counter
- [ ] Test that uncooked items are rejected at the counter
- [ ] Add optional feedback (sounds, particles) for better UX

---

## 8. Debugging Tips

### If uncooked food is being accepted:

```csharp
// Add to ServingZone.OnTriggerEnter() to debug
Debug.Log($"Food item type: {item.type}");
Debug.Log($"Is uncooked? {item.type == Order_ItemType.UncookedSkwewer}");
```

### If cooked food isn't being detected:

```csharp
// Check that ItemIdentity exists
ItemIdentity identity = foodObject.GetComponent<ItemIdentity>();
Debug.Log($"ItemIdentity found: {identity != null}");
Debug.Log($"Item type: {identity?.type}");

// Check that collider is not a trigger
Collider col = foodObject.GetComponent<Collider>();
Debug.Log($"Collider is trigger: {col.isTrigger}"); // Should be FALSE
```

### If cooking isn't completing:

```csharp
// Add logging in your cooking routine
Debug.Log($"Cooking progress: {cookingProgress}/{cookingTimeRequired}");

// Check if ItemIdentity.SetItemAsCooked() was called
// Add this line after calling SetItemAsCooked():
Debug.Log($"Item type after cooking: {itemIdentity.type}");
```

---

## Summary

The key integration point is:

```csharp
// When your cooking system finishes:
ItemIdentity itemIdentity = GetComponent<ItemIdentity>();
if (itemIdentity != null)
{
    itemIdentity.SetItemAsCooked(); // ← This single line enables serving
}
```

That's all you need! The rest of the system handles validation and delivery automatically.

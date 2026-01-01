# Customer Order System - Integration Diagram

## Component Relationships

```
┌─────────────────────────────────────────────────────────────┐
│                    SCENE HIERARCHY                          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  CustomerSpawnPoint (Transform)                            │
│  CounterLocation (Transform)                               │
│  CustomerDespawnPoint (Transform)                          │
│                                                             │
│  ┌─────────────────────────────────────────────────┐       │
│  │ Customer Prefab (Instantiated at runtime)       │       │
│  ├─────────────────────────────────────────────────┤       │
│  │ • NavMeshAgent                                  │       │
│  │ • Customer.cs                                   │       │
│  │ • CustomerMovement.cs                           │       │
│  │                                                 │       │
│  │ ┌──────────────────────────────────────┐       │       │
│  │ │ OrderUIPanel (Canvas - World Space)  │       │       │
│  │ │ ┌────────────────────────────────┐   │       │       │
│  │ │ │ OrderText (TextMeshPro)        │   │       │       │
│  │ │ │ "Skewers: 0/2"                 │   │       │       │
│  │ │ │ "Tea: 0/1"                     │   │       │       │
│  │ │ └────────────────────────────────┘   │       │       │
│  │ └──────────────────────────────────────┘       │       │
│  └─────────────────────────────────────────────────┘       │
│                                                             │
│  Counter (with Box Collider - Trigger)                    │
│  ├─ ServingZone.cs                                         │
│  └─ Box Collider (Set as Trigger)                          │
│                                                             │
│  Skewer Food Prefabs                                       │
│  ├─ ItemIdentity.cs (type = UncookedSkewer)                │
│  └─ Collider (Not Trigger)                                 │
│                                                             │
│  Cooked Skewer Prefabs                                     │
│  ├─ ItemIdentity.cs (type = CookedSkewer)                  │
│  └─ Collider (Not Trigger)                                 │
│                                                             │
│  Tea Prefabs                                               │
│  ├─ ItemIdentity.cs (type = Tea)                           │
│  └─ Collider (Not Trigger)                                 │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Data Flow

```
┌──────────────────────────────────────────────────────────┐
│ 1. INITIALIZATION                                        │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  Customer Instantiated → CustomerMovement.Start()       │
│         ↓                                                │
│  agent.SetDestination(counterLocation)                  │
│         ↓                                                │
│  Customer starts navigating to counter                  │
│                                                          │
└──────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────┐
│ 2. ARRIVAL AT COUNTER                                   │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  CustomerMovement.Update() detects arrival              │
│         ↓                                                │
│  customer.ArriveAtCounter()                             │
│         ↓                                                │
│  • GenerateRandomOrder() (1-3 skewers, 1-2 tea)        │
│  • orderUIPanel.SetActive(true)                         │
│  • RegisterWithServingZone()                            │
│         ↓                                                │
│  ServingZone.SetCurrentCustomer(customer)               │
│                                                          │
└──────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────┐
│ 3. COOKING & DELIVERY                                   │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  Player cooks a skewer:                                 │
│  • YakitoriSkewer (or your cooking script)             │
│  • itemIdentity.SetItemAsCooked() ← KEY INTEGRATION    │
│  • Item state: UncookedSkewer → CookedSkewer           │
│         ↓                                                │
│  Player throws/delivers food into trigger zone          │
│         ↓                                                │
│  ServingZone.OnTriggerEnter(collider)                   │
│         ↓                                                │
│  ItemIdentity item = collider.GetComponent<...>()       │
│         ↓                                                │
│  ┌─ Validate item.type                                  │
│  ├─ UncookedSkewer? → DEBUG.WARN, return (reject)       │
│  ├─ CookedSkewer? → currentCustomer.ReceiveFood()       │
│  └─ Tea? → currentCustomer.ReceiveFood()                │
│         ↓                                                │
│  customer.ReceiveFood(itemType)                         │
│         ↓                                                │
│  • skewersGiven++ (or teaGiven++)                        │
│  • UpdateOrderUI()                                       │
│  • CheckIfSatisfied()                                   │
│         ↓                                                │
│  Destroy(foodObject)                                    │
│                                                          │
└──────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────┐
│ 4. ORDER COMPLETION & DEPARTURE                         │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  if (skewersGiven >= skewersNeeded &&                   │
│      teaGiven >= teaNeeded)                             │
│         ↓                                                │
│  LeaveCounter()                                          │
│         ↓                                                │
│  • orderUIPanel.SetActive(false)                         │
│  • agent.SetDestination(despawnPoint)                    │
│         ↓                                                │
│  Customer walks off-screen                              │
│         ↓                                                │
│  (Ready to instantiate next customer)                    │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

---

## Script Communication

```
CustomerMovement
    │
    ├─→ customer.ArriveAtCounter()
    │
    └─→ servingZone.SetCurrentCustomer(customer)


ServingZone (on Counter trigger)
    │
    └─→ currentCustomer.ReceiveFood(itemType)
            │
            ├─→ ValidateItemType()
            │   └─→ Reject UncookedSkewer
            │
            └─→ UpdateOrderUI()
                │
                └─→ CheckIfSatisfied()
                    └─→ LeaveCounter()


ItemIdentity (on Food objects)
    │
    └─→ SetItemAsCooked()
        └─→ type = CookedSkewer
```

---

## Key Integration Points

### 1. Cooking System → ItemIdentity

**When your skewer finishes cooking:**

```csharp
// In YakitoriSkewer.cs (or wherever cooking completes)
private void OnCookingComplete()
{
    ItemIdentity itemIdentity = GetComponent<ItemIdentity>();
    if (itemIdentity != null)
    {
        itemIdentity.SetItemAsCooked();
    }
}
```

### 2. Customer Spawning

**When spawning a new customer:**

```csharp
// In your GameManager or Spawner
Customer customerPrefab = Resources.Load<Customer>("Prefabs/Customer");
Customer newCustomer = Instantiate(
    customerPrefab, 
    spawnPoint.position, 
    Quaternion.identity
);

// Script automatically handles:
// - Navigation to counter
// - Order generation
// - Serving
// - Despawn
```

### 3. Uncooked Skewer Rejection

**The system automatically rejects uncooked skewers:**

```csharp
// In ServingZone.OnTriggerEnter()
if (item.type == Order_ItemType.UncookedSkwewer)
{
    Debug.LogWarning("Uncooked skewer attempted! Rejecting.");
    return;  // Food is not destroyed, player can try again
}
```

---

## Summary of Reference Assignments

| Script | Field | Reference |
|--------|-------|-----------|
| **Customer** | orderUIPanel | Canvas with TextMeshPro child |
| | orderText | TextMeshPro component |
| | spawnPoint | Empty transform at spawn |
| | counterLocation | Empty transform at counter |
| | despawnPoint | Empty transform off-stage |
| **CustomerMovement** | agent | Auto (GetComponent) |
| | counterLocation | Empty transform at counter |
| | customer | Auto (GetComponent) |
| | arrivalThreshold | 0.5 (adjust if needed) |
| **ServingZone** | — | Placed on counter trigger |
| **ItemIdentity** | type | Order_ItemType value |

---

## State Machine (Customer Lifecycle)

```
┌─────────────┐
│   SPAWNED   │
└──────┬──────┘
       │ (Navigating to counter)
       ↓
┌─────────────┐
│   TRAVELING │
└──────┬──────┘
       │ (Arrived at counter)
       ↓
┌─────────────┐
│   WAITING   │ ← OrderUI visible, ServingZone active
└──────┬──────┘
       │ (Food delivered & validated)
       ↓
┌─────────────┐
│  RECEIVING  │
└──────┬──────┘
       │ (Order complete)
       ↓
┌─────────────┐
│  DEPARTING  │ ← OrderUI hidden, walking to despawn
└──────┬──────┘
       │ (Off-screen)
       ↓
   READY FOR
   REUSE/DESTROY
```

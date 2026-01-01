# VR Order System - Setup Guide

## Overview
This is a complete arcade-style customer order fulfillment system using Unity's NavMesh and XR Interaction Toolkit. Customers arrive at a counter, place orders, and players must deliver cooked food items to fulfill them.

---

## Architecture

### Core Scripts

1. **Order_ItemType.cs** - Enum defining food types
   - `UncookedSkwewer`
   - `CookedSkwewer`
   - `Tea`

2. **ItemIdentity.cs** - Attached to food prefabs
   - Tracks the current item type
   - `SetItemAsCooked()` - Called by cooking system to update state from Uncooked → Cooked

3. **Customer.cs** - Manages customer data & behavior
   - Stores order requirements (random 1-3 skewers, 1-2 tea)
   - Tracks delivered items
   - Manages UI visibility (show at counter, hide when complete)
   - Handles spawn/despawn navigation

4. **CustomerMovement.cs** - Handles NavMesh navigation
   - Spawns customer and navigates to counter
   - Detects arrival at counter
   - Registers customer with ServingZone
   - Validates all references in Start()

5. **ServingZone.cs** - Trigger-based delivery system
   - Placed on the counter trigger zone
   - Receives food items and validates them
   - Rejects uncooked skewers
   - Destroys delivered items

---

## Scene Setup Instructions

### 1. Create Navigation Points (Transform Objects)

Create three empty GameObjects to serve as navigation waypoints:

```
SceneRoot/
├── CustomerSpawnPoint (Transform)
├── CounterLocation (Transform)
└── CustomerDespawnPoint (Transform)
```

**Positioning Tips:**
- **SpawnPoint**: Away from player view, where customers initially appear
- **CounterLocation**: At the counter where orders are taken and food delivered
- **DespawnPoint**: Off-stage or far from counter, where satisfied customers walk

### 2. Create the Customer Prefab

1. Create a new empty GameObject: `Customer`
2. Add these components:
   - **NavMeshAgent** - For pathfinding
   - **Customer** (script) - Order management
   - **CustomerMovement** (script) - Navigation control
   - **Collider** (optional) - For physical interaction

3. Create a child GameObject for the UI:
   ```
   Customer/
   └── OrderUIPanel (Canvas - World Space)
       └── OrderText (TextMeshPro)
   ```

4. Assign references in the Inspector:
   - **Customer Component:**
     - `orderUIPanel` → Drag the OrderUIPanel Canvas here
     - `orderText` → Drag the TextMeshPro text object here
     - `spawnPoint` → Reference the SpawnPoint transform
     - `counterLocation` → Reference the CounterLocation transform
     - `despawnPoint` → Reference the DespawnPoint transform

   - **CustomerMovement Component:**
     - `agent` → Auto-populated (uses GetComponent)
     - `counterLocation` → Reference the CounterLocation transform
     - `customer` → Auto-populated (uses GetComponent)
     - `arrivalThreshold` → Set to 0.5 (default) or adjust based on your scale

5. Save as prefab: `Prefabs/Customer.prefab`

### 3. Set Up the Serving Zone

1. Select your counter GameObject (or create one)
2. Add a **Box Collider** and set as trigger:
   - Adjust size to cover the counter area
   - Enable "Is Trigger"

3. Add the **ServingZone** script to the same GameObject

The serving zone will automatically detect food items and validate them.

### 4. Prepare Food Prefabs

For each food type (Skewer, Tea):

1. Create the food prefab (or use existing ones)
2. Add an **ItemIdentity** component
3. Set the `type` to the appropriate value in Inspector:
   - Uncooked Skewers: `Order_ItemType.UncookedSkwewer`
   - Cooked Skewers: `Order_ItemType.CookedSkwewer`
   - Tea: `Order_ItemType.Tea`

4. Ensure the food prefab has a **Collider** (not trigger) for detection

### 5. NavMesh Baking

1. Open **Window → AI → Navigation**
2. Select all ground/floor GameObjects and mark as "Walkable"
3. Click **Bake** to generate the NavMesh
4. Verify the NavMesh covers the path from spawn → counter → despawn

---

## Game Flow

```
1. Customer spawns at SpawnPoint
2. CustomerMovement navigates customer to CounterLocation
3. Upon arrival, Customer.ArriveAtCounter() is called:
   - Generates random order (1-3 skewers, 1-2 tea)
   - Enables OrderUIPanel to show the order
   - Registers with ServingZone
4. Player cooks and delivers food items
5. ServingZone.OnTriggerEnter() validates items:
   - Accepts: CookedSkwewer, Tea
   - Rejects: UncookedSkwewer
6. Customer.ReceiveFood() updates order progress
7. When order is complete:
   - OrderUIPanel is disabled
   - Customer walks to DespawnPoint
   - Repeat for next customer

```

---

## Integration with Cooking System

### ItemIdentity State Changes

When your cooking system completes a skewer:

```csharp
// In your cooking script (e.g., YakitoriSkewer.cs)
ItemIdentity itemIdentity = GetComponent<ItemIdentity>();
if (itemIdentity != null)
{
    itemIdentity.SetItemAsCooked();
}
```

This transitions the item from `UncookedSkwewer` → `CookedSkwewer`, allowing it to be accepted at the counter.

---

## Key Features

✅ **Null Checks**: All references validated in Start()  
✅ **UI Management**: Panels enabled/disabled instead of destroyed  
✅ **Item Validation**: Uncooked skewers rejected at counter  
✅ **Navigation**: Full customer lifecycle (spawn → counter → despawn)  
✅ **Scalability**: Easy to add multiple customers later (just instantiate multiple Customer prefabs)  
✅ **Debug Logging**: Comprehensive console messages for troubleshooting  

---

## Debug Console Messages

When playing, you'll see logs like:

```
Customer starting navigation to counter.
Customer has arrived at the counter!
Customer order generated: 2 skewers, 1 tea
Customer registered with serving zone.
Received CookedSkewer. Count: 1/2
Received Tea. Count: 1/1
Order Complete! Customer walking to despawn point.
```

Use these to verify the system is working correctly.

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Customer doesn't move | Check NavMesh is baked and counterLocation is assigned |
| UI doesn't appear | Verify orderUIPanel reference and WorldSpace canvas |
| Food not detected | Ensure ItemIdentity component exists on food prefab |
| Uncooked skewers accepted | Check that item.type is updated by your cooking system |
| Customer doesn't despawn | Verify despawnPoint is assigned and NavMesh covers it |

---

## Future Enhancements

- [ ] Multiple customers in queue
- [ ] Customer satisfaction meter
- [ ] Timeout if order not fulfilled in time
- [ ] Sound effects for order complete/rejection
- [ ] Animation states for customer (waiting, happy, sad)
- [ ] Order difficulty levels (more items, stricter timing)

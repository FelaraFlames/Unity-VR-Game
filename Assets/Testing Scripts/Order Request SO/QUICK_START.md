# Quick Start Guide - 5 Minute Setup

Follow these steps to get your customer order system working quickly.

---

## Step 1: Create Navigation Points (1 minute)

1. Create 3 empty GameObjects in your scene:
   - Name: `CustomerSpawnPoint` → Position: `(0, 0, -10)`
   - Name: `CounterLocation` → Position: `(0, 0, 0)`
   - Name: `CustomerDespawnPoint` → Position: `(0, 0, 20)`

2. Remember these object names - you'll reference them next.

---

## Step 2: Set Up Counter (1 minute)

1. Select your counter GameObject
2. Add component: **BoxCollider**
   - Set `Is Trigger` = ✓ (checked)
   - Adjust size to cover counter area
3. Add component: **ServingZone** script
4. Done! Counter is ready.

---

## Step 3: Create Customer Prefab (2 minutes)

### Part A: Create the Customer GameObject

1. Create empty GameObject: `Customer`
2. Add components:
   - **NavMeshAgent** (Add Component → Search "NavMeshAgent")
   - **Customer** script (Add Component → Customer)
   - **CustomerMovement** script (Add Component → CustomerMovement)

### Part B: Create the UI

1. Right-click Customer → 3D Object → TextMeshPro → Text
   - Rename to: `OrderUIPanel`
   - This creates a Canvas automatically
2. Select the new Canvas in the hierarchy
3. In Inspector → Canvas → Render Mode: `World Space`
4. In the OrderUIPanel (Canvas) → Add child TextMeshPro text object:
   - Right-click OrderUIPanel → 3D Object → TextMeshPro → Text
   - Rename to: `OrderText`

### Part C: Assign References

**Select the Customer root GameObject:**

In **Customer script**:
- Drag `OrderUIPanel` to `orderUIPanel` field
- Drag `OrderText` to `orderText` field
- Drag `CustomerSpawnPoint` to `spawnPoint` field
- Drag `CounterLocation` to `counterLocation` field
- Drag `CustomerDespawnPoint` to `despawnPoint` field

In **CustomerMovement script**:
- Drag `CounterLocation` to `counterLocation` field

### Part D: Save as Prefab

1. Drag the Customer GameObject from hierarchy to your Prefabs folder
2. This creates `Customer.prefab`

---

## Step 4: Prepare Food (1 minute)

### For Uncooked Skewers:
1. Select uncooked skewer prefab
2. Add component: **ItemIdentity** script
3. In Inspector → ItemIdentity → type: `UncookedSkwewer`
4. Ensure it has a **Collider** (not set as trigger)

### For Cooked Skewers:
1. Select cooked skewer prefab
2. Add component: **ItemIdentity** script
3. In Inspector → ItemIdentity → type: `CookedSkwewer`
4. Ensure it has a **Collider** (not set as trigger)

### For Tea:
1. Select tea prefab
2. Add component: **ItemIdentity** script
3. In Inspector → ItemIdentity → type: `Tea`
4. Ensure it has a **Collider** (not set as trigger)

---

## Step 5: Bake NavMesh (1 minute)

1. Window → AI → Navigation
2. Select all ground/floor objects in scene
3. Mark them as "Walkable"
4. Click **Bake**
5. Verify NavMesh covers path: spawn → counter → despawn

---

## Test It! (Play)

1. Press Play
2. Check Console for these messages:
   - "Customer starting navigation to counter"
   - "Customer has arrived at the counter!"
   - "Customer order generated: X skewers, Y tea"
3. Look for Order UI above the customer
4. Test: Throw a cooked item → should be accepted
5. Test: Throw an uncooked item → should be rejected

---

## If Something Doesn't Work

### Customer doesn't move:
- [ ] NavMeshAgent component exists
- [ ] CounterLocation is assigned
- [ ] NavMesh is baked
- Check console for errors

### UI doesn't show:
- [ ] orderUIPanel is assigned
- [ ] orderUIPanel is disabled at start (unchecked in hierarchy)
- [ ] Canvas is set to World Space
- [ ] orderText is assigned

### Food not detected:
- [ ] ItemIdentity component exists on prefab
- [ ] Collider exists and IS NOT trigger
- [ ] ServingZone script on counter

### Uncooked items accepted:
- [ ] You haven't integrated cooking system yet
- For now: Manually test by creating a cooked instance

---

## Next: Cooking Integration

When your cooking system completes a skewer, add this:

```csharp
ItemIdentity itemIdentity = GetComponent<ItemIdentity>();
if (itemIdentity != null)
{
    itemIdentity.SetItemAsCooked();
}
```

That's it! See `COOKING_INTEGRATION.md` for more details.

---

## Documentation Reference

- **SETUP_GUIDE.md** - Detailed step-by-step
- **PREFAB_STRUCTURE.md** - Complete hierarchy reference
- **COOKING_INTEGRATION.md** - How to integrate with cooking
- **INTEGRATION_GUIDE.md** - Architecture and data flow
- **README.md** - Full summary of changes

---

## Common Settings Reference

### NavMeshAgent
- Stopping Distance: 0.1
- Arrival Threshold: 0.5 (in CustomerMovement)

### Canvas
- Render Mode: **World Space**
- Position: (0, 2, 0) - above customer

### Colliders
- Food: Is Trigger = ❌ (unchecked)
- Counter: Is Trigger = ✓ (checked)

### order_ItemType Values
```
UncookedSkwewer
CookedSkwewer  
Tea
```

---

## You're Done!

Your system is now functional. Customize as needed:
- Adjust order ranges (1-3 skewers → 1-5)
- Customize UI appearance
- Add sounds/particles
- Integrate with your cooking system

Good luck! 🎮

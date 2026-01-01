# Prefab Structure Reference

This file shows the exact GameObject hierarchy and component setup needed for the customer order system.

---

## Customer Prefab Structure

```
Customer (Root GameObject)
├── Components:
│   ├── Transform
│   ├── NavMeshAgent
│   ├── Customer (script)
│   ├── CustomerMovement (script)
│   ├── Collider (optional - for physics interactions)
│   └── Rigidbody (optional - if using physics)
│
└── Children:
    └── OrderUIPanel (Canvas - World Space)
        ├── Components:
        │   ├── Transform
        │   ├── Canvas (Set Render Mode: World Space)
        │   ├── CanvasScaler
        │   └── GraphicRaycaster
        │
        └── Children:
            ├── Background (Image) - Optional background for the UI
            │   └── Image component
            │
            └── OrderText (TextMeshPro)
                ├── Components:
                │   ├── Transform
                │   ├── TextMeshProUGUI (script)
                │   └── RectTransform
                │
                └── Text Content:
                    "Skewers: 0/2\nTea: 0/1"
```

---

## Component Configuration Details

### Customer Root GameObject

**Transform:**
- Position: (0, 0, 0) - Will be set by spawner
- Rotation: (0, 0, 0)
- Scale: (1, 1, 1)

**NavMeshAgent:**
- Agent Type: "Humanoid" (or your project's default)
- Base Offset: 0.9
- Stopping Distance: 0.1
- Auto Braking: enabled
- Auto Repath: enabled

**Customer Script Inspector Setup:**

| Field | Value | Notes |
|-------|-------|-------|
| skewersNeeded | 0 | Set at runtime |
| skewersGiven | 0 | Set at runtime |
| teaNeeded | 0 | Set at runtime |
| teaGiven | 0 | Set at runtime |
| **orderUIPanel** | [Drag OrderUIPanel here] | **REQUIRED** |
| **orderText** | [Drag OrderText here] | **REQUIRED** |
| **spawnPoint** | [Reference in scene] | **REQUIRED** |
| **counterLocation** | [Reference in scene] | **REQUIRED** |
| **despawnPoint** | [Reference in scene] | **REQUIRED** |

**CustomerMovement Script Inspector Setup:**

| Field | Value | Notes |
|-------|-------|-------|
| agent | [Auto-filled] | Leave empty, auto-populated |
| **counterLocation** | [Reference in scene] | **REQUIRED** |
| customer | [Auto-filled] | Leave empty, auto-populated |
| arrivalThreshold | 0.5 | Adjust if needed |

---

### OrderUIPanel (Child Canvas)

**Canvas Settings:**
- Render Mode: **World Space** (Important!)
- Canvas Scaler: 
  - UI Scale Mode: Scale With Screen Size
  - Reference Resolution: 1920 x 1080

**RectTransform:**
- Position: (0, 2, 0) - Above the customer's head
- Width: 200
- Height: 100
- Rotation: (0, 0, 0)

**Important:** The panel should start **disabled** (unchecked) in the inspector!
The Customer script enables/disables it based on game state.

---

### OrderText (TextMeshPro)

**TextMeshProUGUI:**
- Text: "Skewers: 0/0\nTea: 0/0"
- Font Size: 36
- Alignment: Center / Middle
- Text Color: White (or your preferred color)

**RectTransform:**
- Width: 200
- Height: 100
- Anchors: Stretch, Stretch
- Offsets: All zeros

---

## Food Prefab Structures

### CookedSkewer Prefab

```
CookedSkewer (Root)
├── Components:
│   ├── Transform
│   ├── MeshFilter
│   ├── MeshRenderer
│   ├── Collider (BoxCollider / SphereCollider / CapsuleCollider)
│   │   └── Is Trigger: FALSE ← IMPORTANT!
│   ├── Rigidbody (optional)
│   ├── XRGrabInteractable (for VR grabbing)
│   └── ItemIdentity (script)
│       └── type: CookedSkwewer ← SET IN INSPECTOR
│
└── Optional Children:
    ├── SkewerId (visual identifier)
    └── CookingParticles (visual feedback)
```

**ItemIdentity Configuration:**
- type: `Order_ItemType.CookedSkwewer`

**Collider:**
- Is Trigger: **FALSE** (Must be checkable collider!)
- Size/radius: Matches your skewer model

---

### UncookedSkewer Prefab

```
UncookedSkewer (Root)
├── Components:
│   ├── Transform
│   ├── MeshFilter
│   ├── MeshRenderer
│   ├── Collider (BoxCollider / SphereCollider)
│   │   └── Is Trigger: FALSE
│   ├── Rigidbody
│   ├── XRGrabInteractable (for VR grabbing)
│   ├── YakitoriSkewer (your cooking script)
│   │   └── [Your cooking configuration]
│   └── ItemIdentity (script)
│       └── type: UncookedSkwewer ← SET IN INSPECTOR
│
├── Children:
│   ├── Side1Mesh (visual)
│   ├── Side2Mesh (visual)
│   └── CookedMesh (disabled initially)
│
└── Optional:
    └── CookingProgress (visual indicator)
```

**ItemIdentity Configuration:**
- type: `Order_ItemType.UncookedSkwewer`

**Cooking Script (YakitoriSkewer):**
- When cooking completes, call:
```csharp
GetComponent<ItemIdentity>().SetItemAsCooked();
```

---

### Tea Prefab

```
Tea (Root)
├── Components:
│   ├── Transform
│   ├── MeshFilter (Cup/Glass mesh)
│   ├── MeshRenderer
│   ├── Collider
│   │   └── Is Trigger: FALSE
│   ├── Rigidbody
│   ├── XRGrabInteractable (for VR grabbing)
│   └── ItemIdentity (script)
│       └── type: Tea ← SET IN INSPECTOR
│
└── Optional Children:
    └── Liquid (visual for tea inside cup)
```

**ItemIdentity Configuration:**
- type: `Order_ItemType.Tea`

**Note:** Tea doesn't need cooking logic, just ItemIdentity.

---

## Counter/Serving Zone Setup

```
Counter (Root GameObject)
├── Components:
│   ├── Transform
│   ├── MeshFilter (visual counter)
│   ├── MeshRenderer
│   ├── BoxCollider
│   │   └── Is Trigger: TRUE ← IMPORTANT! This is the delivery zone!
│   └── ServingZone (script)
│
└── Optional Children:
    ├── CounterVisuals (mesh)
    ├── DeliveryZoneIndicator (visual feedback)
    └── Particles (optional effect on delivery)
```

**BoxCollider Configuration:**
- Is Trigger: **TRUE**
- Size: Match your counter surface area (e.g., 1, 0.1, 1)
- Position: Centered on counter

---

## Scene Navigation Points Setup

```
SceneRoot
├── CustomerSpawnPoint (Empty Transform)
│   └── Position: Away from player view
│       Example: (0, 0, -10)
│
├── CounterLocation (Empty Transform)
│   └── Position: At the counter
│       Example: (0, 0, 0)
│
└── CustomerDespawnPoint (Empty Transform)
    └── Position: Off-screen/far away
        Example: (0, 0, 20)
```

---

## Complete Hierarchy Example

```
Scene
├── Player (VR Rig)
│   └── [Your VR setup]
│
├── Environment
│   └── Counter
│       ├── MeshFilter (counter visual)
│       ├── MeshRenderer
│       ├── BoxCollider (Is Trigger: TRUE)
│       └── ServingZone (script)
│
├── Navigation Points
│   ├── CustomerSpawnPoint
│   ├── CounterLocation
│   └── CustomerDespawnPoint
│
├── Customers (Folder)
│   └── Customer (Prefab Instance)
│       ├── NavMeshAgent
│       ├── Customer (script)
│       ├── CustomerMovement (script)
│       └── OrderUIPanel
│           └── OrderText
│
└── Food (Folder)
    ├── CookedSkewer (Prefab Instance) [When delivered]
    ├── UncookedSkewer (Prefab Instance) [Being cooked]
    └── Tea (Prefab Instance) [Ready or being delivered]
```

---

## Inspector Checklist

### Customer Prefab

- [ ] NavMeshAgent added and configured
- [ ] Customer script attached
  - [ ] orderUIPanel assigned
  - [ ] orderText assigned
  - [ ] spawnPoint assigned
  - [ ] counterLocation assigned
  - [ ] despawnPoint assigned
- [ ] CustomerMovement script attached
  - [ ] counterLocation assigned
- [ ] OrderUIPanel is a Canvas (World Space)
- [ ] OrderUIPanel is DISABLED at start
- [ ] OrderText has TextMeshProUGUI component
- [ ] OrderUIPanel is child of Customer

### Food Prefabs (All Types)

- [ ] ItemIdentity script attached
- [ ] type is set to correct Order_ItemType
- [ ] Collider exists and is NOT trigger
- [ ] If grabbable: XRGrabInteractable added
- [ ] If cooking: YakitoriSkewer or equivalent added

### Counter

- [ ] BoxCollider added
- [ ] Is Trigger is TRUE
- [ ] Size covers delivery area
- [ ] ServingZone script attached

### Scene Navigation Points

- [ ] CustomerSpawnPoint exists (Transform)
- [ ] CounterLocation exists (Transform)
- [ ] CustomerDespawnPoint exists (Transform)
- [ ] All three are referenced in Customer prefab

### NavMesh

- [ ] Baked and covers spawn → counter → despawn path
- [ ] Floor is marked as "Walkable"

---

## Common Mistakes to Avoid

❌ **Food Collider set as Trigger** 
   → Detection won't work
   
❌ **OrderUIPanel enabled at start**
   → UI will show before customer arrives
   
❌ **Missing ItemIdentity on food**
   → Items won't be detected
   
❌ **ServingZone not set as Trigger**
   → OnTriggerEnter won't be called
   
❌ **NavMesh not baked**
   → Customer won't move
   
❌ **Navigation points not assigned**
   → Errors in console, customer won't move
   
❌ **Wrong Order_ItemType assigned**
   → Items accepted/rejected incorrectly

---

## Quick Validation

Before playing, verify:

1. **Select Customer prefab in scene:**
   - NavMeshAgent component visible ✓
   - All Customer script fields assigned ✓
   - All CustomerMovement fields assigned ✓

2. **Select Counter:**
   - BoxCollider.isTrigger == true ✓
   - ServingZone script attached ✓

3. **Select OrderUIPanel:**
   - Active in hierarchy == FALSE ✓
   - Canvas.renderMode == WorldSpace ✓

4. **Scene validation:**
   - Press Play
   - Check console for errors
   - Customer should start moving to counter
   - "Customer arrived" message should appear
   - Order UI should become visible

5. **Food validation:**
   - Throw uncooked skewer: Should be rejected
   - Throw cooked skewer: Should be accepted
   - Throw tea: Should be accepted

---

## Assets Path Examples

```
Prefabs/
├── Customer.prefab
├── Food/
│   ├── CookedSkewer.prefab
│   ├── UncookedSkewer.prefab
│   └── Tea.prefab
└── ...

Scenes/
└── Game.scene
    ├── [Scene with counter and navigation points]
    └── Instances of Customer prefab

Scripts/
├── Order Request SO/
│   ├── Customer.cs
│   ├── CustomerMovement.cs
│   ├── ServingZone.cs
│   ├── ItemIdentity.cs
│   └── Order_ItemType.cs
└── ...
```

That's everything you need! Follow this structure and your system will work perfectly.

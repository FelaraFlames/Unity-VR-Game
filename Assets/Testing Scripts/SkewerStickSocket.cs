using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Handles skewer stick attachment logic with meatball integration.
/// This script should be attached to the Skewer Stick GameObject.
/// It manages the XR Socket Interactor and prefab variant swapping.
/// </summary>
public class SkewerStickSocket : MonoBehaviour
{
    [Header("Slot References")]
    [SerializeField] private Transform yakitoriSlot; // The child object containing XR Socket Interactor
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socketInteractor;

    [Header("Prefab Variants")]
    [SerializeField] private GameObject skewerWithMeatVariant; // The "Skewer with meat Variant" prefab

    private MeatballAttachable attachedMeatball;
    [Header("State")]
    [SerializeField] private bool hasMeatball = false;

    void Start()
    {
        // Debug.Log($"[SkewerStickSocket] Starting initialization...");
        // Debug.Log($"[SkewerStickSocket] yakitoriSlot assigned: {yakitoriSlot != null}");
        // Debug.Log($"[SkewerStickSocket] socketInteractor assigned: {socketInteractor != null}");

        // Get the socket interactor if not assigned
        if (socketInteractor == null && yakitoriSlot != null)
        {
            // Debug.Log($"[SkewerStickSocket] Trying to find XRSocketInteractor on yakitoriSlot: {yakitoriSlot.name}");
            socketInteractor = yakitoriSlot.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
            // Debug.Log($"[SkewerStickSocket] After GetComponent, socketInteractor is: {socketInteractor}");
        }

        if (socketInteractor != null)
        {
            // Debug.Log("[SkewerStickSocket] XRSocketInteractor found! Subscribing to events.");
            // Subscribe to socket interactor events
            socketInteractor.selectEntered.AddListener(OnMeatballInserted);
            socketInteractor.selectExited.AddListener(OnMeatballRemoved);
        }
        else
        {
            Debug.LogError($"[SkewerStickSocket] XRSocketInteractor not found! yakitoriSlot={yakitoriSlot}, socketInteractor={socketInteractor}");
        }
    }

    void OnDestroy()
    {
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.RemoveListener(OnMeatballInserted);
            socketInteractor.selectExited.RemoveListener(OnMeatballRemoved);
        }
    }

    /// <summary>
    /// Called when a meatball enters the socket interactor
    /// </summary>
    private void OnMeatballInserted(SelectEnterEventArgs args)
    {
        MeatballAttachable meatball = args.interactableObject.transform.GetComponent<MeatballAttachable>();
        
        if (meatball != null && !meatball.IsAttachedToSkewer())
        {
            // Debug.Log("Meatball detected in socket! Hover mesh preview is active.");
        }
    }

    /// <summary>
    /// Called when a meatball is removed from the socket (either by dropping or other action)
    /// </summary>
    private void OnMeatballRemoved(SelectExitEventArgs args)
    {
        MeatballAttachable meatball = args.interactableObject.transform.GetComponent<MeatballAttachable>();
        
        if (meatball != null && !meatball.IsAttachedToSkewer())
        {
            // Debug.Log("Meatball removed from socket preview!");
        }
    }

    /// <summary>
    /// Attach the meatball to the skewer and transform the prefab variant
    /// Call this when the player releases the meatball while it's in the socket
    /// </summary>
    public void AttachMeatballToSkewer(MeatballAttachable meatball)
    {
        if (hasMeatball)
        {
            Debug.LogWarning("Skewer already has a meatball attached!");
            return;
        }

        if (meatball == null)
        {
            Debug.LogError("Trying to attach null meatball!");
            return;
        }

        Debug.Log("[SkewerStickSocket] AttachMeatballToSkewer called!");

        // Attach the meatball to the socket slot
        meatball.AttachToSkewer(yakitoriSlot, transform);
        attachedMeatball = meatball;
        hasMeatball = true;

        // Swap to the "Skewer with meat" variant
        Debug.Log("[SkewerStickSocket] About to call SwapToMeatVariant()");
        SwapToMeatVariant();

        Debug.Log("Meatball successfully attached to skewer!");
    }

    /// <summary>
    /// Swap the current skewer prefab to the "Skewer with meat Variant"
    /// </summary>
    private void SwapToMeatVariant()
    {
        Debug.Log("[SwapToMeatVariant] Starting swap...");
        Debug.Log($"[SwapToMeatVariant] skewerWithMeatVariant is: {skewerWithMeatVariant}");
        
        if (skewerWithMeatVariant == null)
        {
            Debug.LogError("Skewer with meat variant prefab not assigned!");
            return;
        }

        // Get the current position and rotation
        Vector3 currentPos = transform.position;
        Quaternion currentRot = transform.rotation;
        Transform currentParent = transform.parent;
        int currentSiblingIndex = transform.GetSiblingIndex();

        // Store the meatball reference before destroying
        MeatballAttachable meatballRef = attachedMeatball;

        Debug.Log($"[SwapToMeatVariant] Instantiating new variant at {currentPos}");
        
        // Instantiate the new variant at the same position
        GameObject newSkewerVariant = Instantiate(skewerWithMeatVariant, currentPos, currentRot, currentParent);
        newSkewerVariant.name = skewerWithMeatVariant.name;
        newSkewerVariant.transform.SetSiblingIndex(currentSiblingIndex);

        Debug.Log($"[SwapToMeatVariant] New variant created: {newSkewerVariant.name}");

        // Find the yakitori slot in the new variant
        Transform newYakitoriSlot = newSkewerVariant.transform.Find("Yakitori_Slot");
        if (newYakitoriSlot != null)
        {
            // Move the meatball to the new variant's slot
            meatballRef.transform.SetParent(newYakitoriSlot, false);
            meatballRef.transform.localPosition = Vector3.zero;
            meatballRef.transform.localRotation = Quaternion.identity;
            Debug.Log("[SwapToMeatVariant] Meatball moved to new variant!");
        }
        else
        {
            Debug.LogWarning("Yakitori_Slot not found in new variant!");
        }

        // Copy any additional properties if needed (like the YakitoriSkewer component)
        YakitoriSkewer oldSkewerScript = GetComponent<YakitoriSkewer>();
        if (oldSkewerScript != null)
        {
            // Copy cooking state if needed
            YakitoriSkewer newSkewerScript = newSkewerVariant.GetComponent<YakitoriSkewer>();
            if (newSkewerScript != null)
            {
                // Copy any relevant state
                Debug.Log("YakitoriSkewer component found in variant");
            }
        }

        Debug.Log("[SwapToMeatVariant] Destroying old skewer...");
        
        // Destroy the old skewer
        Destroy(gameObject);
        
        Debug.Log("[SwapToMeatVariant] Swap complete!");
    }

    /// <summary>
    /// Check if the skewer has a meatball attached
    /// </summary>
    public bool HasMeatball() => hasMeatball;

    /// <summary>
    /// Get the attached meatball (if any)
    /// </summary>
    public MeatballAttachable GetAttachedMeatball() => attachedMeatball;
}

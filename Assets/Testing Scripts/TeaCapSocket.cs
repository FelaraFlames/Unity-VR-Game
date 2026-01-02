using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnitySimpleLiquid;

// Handles tea cap attachment logic.
// This script should be attached to the Cap Slot GameObject.
// It manages the XR Socket Interactor and updates parent LiquidContainer and ItemIdentity components.
public class TeaCapSocket : MonoBehaviour
{
    [Header("Socket References")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socketInteractor;

    [Header("Parent References")]
    [SerializeField] private LiquidContainer liquidContainer;
    [SerializeField] private ItemIdentity itemIdentity;

    [Header("State")]
    [SerializeField] private bool hasCap = false;

    void Start()
    {
        // Get the socket interactor if not assigned
        if (socketInteractor == null)
        {
            socketInteractor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        }

        // Get parent references if not assigned
        if (liquidContainer == null)
        {
            liquidContainer = GetComponentInParent<LiquidContainer>();
        }

        if (itemIdentity == null)
        {
            itemIdentity = GetComponentInParent<ItemIdentity>();
        }

        if (socketInteractor != null)
        {
            // Subscribe to socket interactor events
            socketInteractor.selectEntered.AddListener(OnCapInserted);
            socketInteractor.selectExited.AddListener(OnCapRemoved);
        }
        else
        {
            Debug.LogError($"[TeaCapSocket] XRSocketInteractor not found on {gameObject.name}!");
        }

        if (liquidContainer == null)
        {
            Debug.LogError($"[TeaCapSocket] LiquidContainer not found in parent of {gameObject.name}!");
        }

        if (itemIdentity == null)
        {
            Debug.LogError($"[TeaCapSocket] ItemIdentity not found in parent of {gameObject.name}!");
        }
    }

    void OnDestroy()
    {
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.RemoveListener(OnCapInserted);
            socketInteractor.selectExited.RemoveListener(OnCapRemoved);
        }
    }

    // Called when a cap enters the socket interactor
    private void OnCapInserted(SelectEnterEventArgs args)
    {
        if (hasCap)
        {
            Debug.LogWarning("[TeaCapSocket] Cap slot already has a cap attached!");
            return;
        }

        GameObject capPrefab = args.interactableObject.transform.gameObject;
        
        hasCap = true;

        // Destroy the cap prefab (the cap mesh on Tea Cup will be activated by LiquidContainer.IsOpen)
        Destroy(capPrefab);
        Debug.Log("[TeaCapSocket] Cap prefab destroyed");

        // Update parent LiquidContainer to close the container (this will activate the cap mesh GameObject)
        if (liquidContainer != null)
        {
            liquidContainer.IsOpen = false;
            Debug.Log("[TeaCapSocket] Container closed - IsOpen set to false (cap mesh activated by LiquidContainer)");
        }

        // Update parent ItemIdentity type from UncappedTea to Tea
        if (itemIdentity != null)
        {
            if (itemIdentity.type == Order_ItemType.UncappedTea)
            {
                itemIdentity.type = Order_ItemType.Tea;
                Debug.Log("[TeaCapSocket] Item type changed from UncappedTea to Tea");
            }
            else
            {
                Debug.LogWarning($"[TeaCapSocket] ItemIdentity type is {itemIdentity.type}, expected UncappedTea");
            }
        }
    }

    // Called when a cap is removed from the socket
    // Note: This should not normally be called after OnCapInserted destroys the prefab
    // But handle it in case of edge cases
    private void OnCapRemoved(SelectExitEventArgs args)
    {
        if (!hasCap)
        {
            // Cap was already removed or never existed, ignore
            return;
        }

        hasCap = false;

        // Update parent LiquidContainer to open the container (this will deactivate the cap mesh GameObject)
        if (liquidContainer != null)
        {
            liquidContainer.IsOpen = true;
            Debug.Log("[TeaCapSocket] Container opened - IsOpen set to true (cap mesh deactivated by LiquidContainer)");
        }

        // Update parent ItemIdentity type back to UncappedTea
        if (itemIdentity != null)
        {
            if (itemIdentity.type == Order_ItemType.Tea)
            {
                itemIdentity.type = Order_ItemType.UncappedTea;
                Debug.Log("[TeaCapSocket] Item type changed from Tea back to UncappedTea");
            }
        }
    }

    // Check if the cap slot has a cap attached
    public bool HasCap() => hasCap;
}
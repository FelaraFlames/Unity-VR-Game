using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// This script automatically handles meatball attachment when placed in the socket.
/// Attach this to the Yakitori_Slot GameObject (which contains the XRSocketInteractor).
/// </summary>
public class YakitoriSlotHandler : MonoBehaviour
{
    private SkewerStickSocket skewerStickSocket;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socketInteractor;
    private MeatballAttachable pendingMeatball;
    private float attachmentCheckTimer = 0f;
    private const float ATTACHMENT_DELAY = 0.1f; // Small delay to ensure socket has control

    void Start()
    {
        skewerStickSocket = GetComponentInParent<SkewerStickSocket>();
        socketInteractor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();

        if (skewerStickSocket == null)
        {
            Debug.LogError("SkewerStickSocket not found in parent!");
        }

        if (socketInteractor == null)
        {
            Debug.LogError("XRSocketInteractor not found!");
        }

        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.AddListener(OnMeatballEntered);
            socketInteractor.selectExited.AddListener(OnMeatballExited);
        }
    }

    void OnDestroy()
    {
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.RemoveListener(OnMeatballEntered);
            socketInteractor.selectExited.RemoveListener(OnMeatballExited);
        }
    }

    private void OnMeatballEntered(SelectEnterEventArgs args)
    {
        MeatballAttachable meatball = args.interactableObject.transform.GetComponent<MeatballAttachable>();
        
        if (meatball != null && !meatball.IsAttachedToSkewer())
        {
            Debug.Log("[YakitoriSlotHandler] Meatball entered socket - scheduling attachment check");
            pendingMeatball = meatball;
            attachmentCheckTimer = 0f;
        }
    }

    private void OnMeatballExited(SelectExitEventArgs args)
    {
        pendingMeatball = null;
    }

    void Update()
    {
        // Check if we have a pending meatball to attach
        if (pendingMeatball != null && !pendingMeatball.IsAttachedToSkewer())
        {
            attachmentCheckTimer += Time.deltaTime;
            
            // After a small delay, attach the meatball
            if (attachmentCheckTimer >= ATTACHMENT_DELAY)
            {
                Debug.Log("[YakitoriSlotHandler] Attaching meatball!");
                skewerStickSocket.AttachMeatballToSkewer(pendingMeatball);
                pendingMeatball = null;
            }
        }
    }
}

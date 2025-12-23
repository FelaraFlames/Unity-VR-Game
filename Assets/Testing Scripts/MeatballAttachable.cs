using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Handles meatball behavior when grabbed and attached to a skewer stick.
/// This script should be attached to the Meatball GameObject.
/// </summary>
public class MeatballAttachable : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private bool isAttachedToSkewer = false;

    void Start()
    {
        // Get the grab interactable component
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        if (grabInteractable != null)
        {
            // Subscribe to select/deselect events
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log("Meatball grabbed!");
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        Debug.Log("Meatball released!");
        
        // If not attached to skewer, meatball will fall naturally
        if (!isAttachedToSkewer && rb != null)
        {
            rb.isKinematic = false;
        }
    }

    /// <summary>
    /// Called by SkewerStickSocket when meatball is successfully attached
    /// </summary>
    public void AttachToSkewer(Transform attachPoint, Transform skewerStickParent)
    {
        if (isAttachedToSkewer)
        {
            Debug.LogWarning("Meatball is already attached to a skewer!");
            return;
        }

        isAttachedToSkewer = true;

        // Disable physics and grab interactable
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }

        // Parent the meatball to the attach point
        transform.SetParent(attachPoint, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Debug.Log("Meatball attached to skewer!");
    }

    public bool IsAttachedToSkewer() => isAttachedToSkewer;
}

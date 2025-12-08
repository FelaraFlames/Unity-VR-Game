using UnityEngine;

public class GrillTrigger : MonoBehaviour
{
    // This function is called automatically when a Collider enters the trigger zone
    void OnTriggerEnter(Collider other)
    {
        // Get the YakitoriSkewer component from the parent (since colliders are on child objects)
        YakitoriSkewer skewer = other.GetComponentInParent<YakitoriSkewer>();
        
        if (skewer == null)
        {
            // Try getting it directly in case the script is on the same object
            skewer = other.GetComponent<YakitoriSkewer>();
        }

        if (skewer != null)
        {
            // Check which side collider entered by comparing the collider reference
            // We need to get the collider references from the skewer to compare
            Collider side1Collider = skewer.GetComponent<Collider>();
            Collider side2Collider = null;
            
            // Try to find the colliders - this is a bit tricky since we need references
            // Better approach: use a tag or check the collider name, or pass the collider reference
            
            // Alternative: Check if the collider is side1 or side2 by checking the skewer's colliders
            // For now, we'll use a helper method on YakitoriSkewer to identify which side
            if (skewer.IsSide1Collider(other))
            {
                skewer.StartCookingSide1();
                Debug.Log("Side 1 detected on grill, starting cooking timer.");
            }
            else if (skewer.IsSide2Collider(other))
            {
                skewer.StartCookingSide2();
                Debug.Log("Side 2 detected on grill, starting cooking timer.");
            }
        }
    }

    // Optional: Use OnTriggerExit to handle when a meat is removed mid-cook
    void OnTriggerExit(Collider other)
    {
        // Get the YakitoriSkewer component from the parent
        YakitoriSkewer skewer = other.GetComponentInParent<YakitoriSkewer>();
        
        if (skewer == null)
        {
            skewer = other.GetComponent<YakitoriSkewer>();
        }

        if (skewer != null)
        {
            // Check which side collider exited
            if (skewer.IsSide1Collider(other))
            {
                skewer.StopCookingSide1();
                Debug.Log("Side 1 removed from grill. Cooking stopped.");
            }
            else if (skewer.IsSide2Collider(other))
            {
                skewer.StopCookingSide2();
                Debug.Log("Side 2 removed from grill. Cooking stopped.");
            }
        }
    }
}

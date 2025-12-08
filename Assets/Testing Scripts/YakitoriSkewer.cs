using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class YakitoriSkewer : MonoBehaviour
{
    [Header("Cooking Settings")]
    [SerializeField] private float timeToCookSide = 8f;
    [SerializeField] private float timeUntilBurned = 16f;
    [SerializeField] private float targetCookProgress = 0.9f; // Progress needed for each side to be considered cooked
    
    [Header("Mesh References")]
    [SerializeField] private GameObject side1Mesh; // First side mesh
    [SerializeField] private GameObject side2Mesh; // Second side mesh
    [SerializeField] private GameObject cookedMesh; // Fully cooked mesh
    [SerializeField] private GameObject burnedMesh; // Burnt mesh
    
    [Header("Collider References")]
    [SerializeField] private Collider side1Collider; // Collider for side 1
    [SerializeField] private Collider side2Collider; // Collider for side 2
    
    // Cooking progress for each side (0 = raw, 1 = fully cooked/burned)
    private float side1Progress = 0f;
    private float side2Progress = 0f;
    
    // Active cooking coroutines for each side
    private Coroutine side1CookingCoroutine;
    private Coroutine side2CookingCoroutine;
    
    // State tracking
    private bool isCooked = false;
    private bool isBurned = false;
    
    // References to renderers for visual feedback on each side mesh
    private Renderer side1Renderer;
    private Renderer side2Renderer;

    void Start()
    {
        // Initialize meshes - show side meshes, hide cooked/burned
        if (side1Mesh != null) side1Mesh.SetActive(true);
        if (side2Mesh != null) side2Mesh.SetActive(true);
        if (cookedMesh != null) cookedMesh.SetActive(false);
        if (burnedMesh != null) burnedMesh.SetActive(false);
        
        // Get renderers from the meat mesh GameObjects (not the parent stick)
        if (side1Mesh != null)
        {
            side1Renderer = side1Mesh.GetComponent<Renderer>();
        }
        if (side2Mesh != null)
        {
            side2Renderer = side2Mesh.GetComponent<Renderer>();
        }
    }

    // Called when side 1 collider enters the grill
    public void StartCookingSide1()
    {
        // Don't cook if already fully cooked or burned
        if (isCooked || isBurned)
        {
            return;
        }
        
        // Don't restart if already cooking
        if (side1CookingCoroutine != null)
        {
            return;
        }
        
        // Start cooking side 1
        side1CookingCoroutine = StartCoroutine(CookSideRoutine(1));
        Debug.Log("Started cooking side 1.");
    }
    
    // Called when side 2 collider enters the grill
    public void StartCookingSide2()
    {
        // Don't cook if already fully cooked or burned
        if (isCooked || isBurned)
        {
            return;
        }
        
        // Don't restart if already cooking
        if (side2CookingCoroutine != null)
        {
            return;
        }
        
        // Start cooking side 2
        side2CookingCoroutine = StartCoroutine(CookSideRoutine(2));
        Debug.Log("Started cooking side 2.");
    }
    
    // Called when side 1 collider exits the grill
    public void StopCookingSide1()
    {
        if (side1CookingCoroutine != null)
        {
            StopCoroutine(side1CookingCoroutine);
            side1CookingCoroutine = null;
            Debug.Log("Stopped cooking side 1. Progress: " + (side1Progress * 100f).ToString("F1") + "%");
        }
    }
    
    // Called when side 2 collider exits the grill
    public void StopCookingSide2()
    {
        if (side2CookingCoroutine != null)
        {
            StopCoroutine(side2CookingCoroutine);
            side2CookingCoroutine = null;
            Debug.Log("Stopped cooking side 2. Progress: " + (side2Progress * 100f).ToString("F1") + "%");
        }
    }
    
    // Cooking coroutine for a specific side
    IEnumerator CookSideRoutine(int sideNumber)
    {
        // Get reference to the appropriate progress variable
        float currentProgress = (sideNumber == 1) ? side1Progress : side2Progress;
        float elapsedTime = currentProgress * timeToCookSide; // Resume from current progress
        
        // Cook until target progress
        while (currentProgress < targetCookProgress && !isBurned)
        {
            elapsedTime += Time.deltaTime;
            currentProgress = Mathf.Clamp01(elapsedTime / timeToCookSide);
            
            // Update the appropriate progress variable
            if (sideNumber == 1)
            {
                side1Progress = currentProgress;
            }
            else
            {
                side2Progress = currentProgress;
            }
            
            UpdateVisualFeedback(sideNumber, currentProgress);
            
            yield return null; // Wait one frame
        }
        
        // Check if we reached target progress
        if (currentProgress >= targetCookProgress && !isBurned)
        {
            Debug.Log($"Side {sideNumber} reached target cook progress!");
            CheckIfFullyCooked();
        }
        
        // Continue cooking past target (overcooking phase)
        float overcookTime = 0f;
        while (currentProgress < 1f && !isBurned)
        {
            elapsedTime += Time.deltaTime;
            overcookTime += Time.deltaTime;
            
            // Calculate progress including overcooking
            float overcookProgress = overcookTime / timeUntilBurned;
            currentProgress = Mathf.Clamp01(targetCookProgress + (1f - targetCookProgress) * overcookProgress);
            
            // Update the appropriate progress variable
            if (sideNumber == 1)
            {
                side1Progress = currentProgress;
            }
            else
            {
                side2Progress = currentProgress;
            }
            
            UpdateVisualFeedback(sideNumber, currentProgress);
            
            yield return null;
        }
        
        // If we reach here, the side is burned
        if (currentProgress >= 1f)
        {
            SetBurned();
        }
        
        // Clear the coroutine reference
        if (sideNumber == 1)
        {
            side1CookingCoroutine = null;
        }
        else
        {
            side2CookingCoroutine = null;
        }
    }
    
    // Check if both sides are cooked enough to become fully cooked
    void CheckIfFullyCooked()
    {
        if (isCooked || isBurned)
        {
            return;
        }
        
        if (side1Progress >= targetCookProgress && side2Progress >= targetCookProgress)
        {
            SetCooked();
        }
    }
    
    // Set the skewer to fully cooked state
    void SetCooked()
    {
        if (isBurned)
        {
            return; // Can't become cooked if already burned
        }
        
        isCooked = true;
        
        // Stop all cooking
        StopCookingSide1();
        StopCookingSide2();
        
        // Swap to cooked mesh
        if (side1Mesh != null) side1Mesh.SetActive(false);
        if (side2Mesh != null) side2Mesh.SetActive(false);
        if (cookedMesh != null) cookedMesh.SetActive(true);
        if (burnedMesh != null) burnedMesh.SetActive(false);
        
        Debug.Log("Skewer is perfectly cooked!");
    }
    
    // Set the skewer to burned state
    void SetBurned()
    {
        isBurned = true;
        isCooked = false;
        
        // Stop all cooking
        StopCookingSide1();
        StopCookingSide2();
        
        // Swap to burned mesh
        if (side1Mesh != null) side1Mesh.SetActive(false);
        if (side2Mesh != null) side2Mesh.SetActive(false);
        if (cookedMesh != null) cookedMesh.SetActive(false);
        if (burnedMesh != null) burnedMesh.SetActive(true);
        
        Debug.Log("Skewer burned!");
    }
    
    // Update visual feedback based on cooking progress
    void UpdateVisualFeedback(int sideNumber, float progress)
    {
        // Get the renderer for the side that's cooking
        Renderer targetRenderer = (sideNumber == 1) ? side1Renderer : side2Renderer;
        
        if (targetRenderer != null)
        {
            // Interpolate color based on progress
            Color rawColor = new Color(0.95f, 0.65f, 0.55f);
            Color cookedColor = new Color(0.75f, 0.54f, 0.12f);
            Color burnedColor = Color.black;
            
            // If progress is past target, start darkening toward burned
            if (progress >= targetCookProgress)
            {
                float burnProgress = (progress - targetCookProgress) / (1f - targetCookProgress);
                targetRenderer.material.color = Color.Lerp(cookedColor, burnedColor, burnProgress);
            }
            else
            {
                // Normal cooking progression
                float cookProgress = progress / targetCookProgress;
                targetRenderer.material.color = Color.Lerp(rawColor, cookedColor, cookProgress);
            }
        }
    }
    
    // Public getters for progress (useful for UI or debugging)
    public float GetSide1Progress() => side1Progress;
    public float GetSide2Progress() => side2Progress;
    public bool IsCooked() => isCooked;
    public bool IsBurned() => isBurned;
    
    // Helper methods to identify which side collider is being used
    public bool IsSide1Collider(Collider collider)
    {
        return collider == side1Collider || (side1Collider != null && collider.transform == side1Collider.transform);
    }
    
    public bool IsSide2Collider(Collider collider)
    {
        return collider == side2Collider || (side2Collider != null && collider.transform == side2Collider.transform);
    }
}

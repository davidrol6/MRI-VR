/* * SCRIPT SUMMARY:
 * This script detects when the user grabs and holds a specific object to complete an objective.
 * * * How it works:
 * - It listens for 'OnGrab' and 'OnRelease' events (typically triggered by VR interaction components like the Meta SDK).
 * - When the object is held, a timer starts counting up.
 * - If the user holds the object for the required duration (e.g., 2 seconds), the objective is marked as complete.
 * - If the object is dropped before the time is up, the timer resets to zero.
 */

using UnityEngine;

public class GrabObjective : MonoBehaviour
{
    [Header("Reference to ObjectiveManager")]
    public ObjectiveManager objectiveManager; // Reference to the core logic tracking progress

    [Header("Exact description of the objective")]
    public string objectiveDescription; // The key string that matches the objective in the Manager

    [Header("Time required to complete the objective (seconds)")]
    public float requiredHoldTime = 2f; // How long the user must hold the item to register the action

    private float holdTimer = 0f;       // Internal timer
    private bool isHeld = false;        // State flag
    private bool objectiveCompleted = false; // Flag to prevent multiple completions

    private void Awake()
    {
        // Auto-assign ObjectiveManager if not set manually in the Inspector
        if (objectiveManager == null && ObjectiveManager.Instance != null)
        {
            objectiveManager = ObjectiveManager.Instance;
        }
    }

    private void Update()
    {
        // Stop logic if the objective is already done or the manager is missing
        if (objectiveCompleted || objectiveManager == null)
            return;

        // Count time only while the object is actively being held
        if (isHeld)
        {
            holdTimer += Time.deltaTime;

            // Check if the hold duration has met the requirement
            if (holdTimer >= requiredHoldTime)
            {
                objectiveCompleted = true;

                Debug.Log($"Objective completed after {requiredHoldTime} seconds: {objectiveDescription}");

                // Notify the manager to mark the task as done
                objectiveManager.CompleteObjective(objectiveDescription);
                objectiveManager.PrintObjectivesStatus();
            }
        }
        else
        {
            // Reset timer when the object is released early (forcing a continuous hold)
            holdTimer = 0f;
        }
    }

    // --- Interaction Callbacks ---
    // These functions are intended to be triggered by Unity Events from the VR SDK 
    // (e.g., OVRGrabbable's Unity Events or XR Interaction Toolkit's Select Enter/Exit)

    public void OnGrab()
    {
        Debug.Log($"OnGrab for objective: {objectiveDescription}");
        isHeld = true;
    }

    public void OnRelease()
    {
        Debug.Log($"OnRelease for objective: {objectiveDescription}");
        isHeld = false;
    }
}
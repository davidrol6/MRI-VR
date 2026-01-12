/* * SCRIPT SUMMARY:
 * This script detects when a specific interactable object is released within a defined target zone.
 * * * How it works:
 * - It monitors the grab state using the 'ItemGrabEvents' component.
 * - When the user releases the object, it checks if the object's position is inside the 'targetZone' collider.
 * - If inside, it "snaps" the object to a precise position and rotation (Snap Point) and freezes its physics (Kinematic).
 * - It then notifies the ObjectiveManager that the specific task (e.g., "Place Head Coil") is complete.
 */

using UnityEngine;

public class DropInZoneObjective : MonoBehaviour
{
    [Header("Zone where the object must be dropped")]
    public Collider targetZone;        // The trigger or collider defining the valid drop area

    [Header("Exact placement point")]
    public Transform snapPoint;        // The final position/rotation the object will snap to

    [Header("Linked objective")]
    public ObjectiveManager objectiveManager; // Reference to the core logic
    public string objectiveDescription;       // The ID string for the objective

    [Tooltip("If true, snapping/completion will only happen the first time.")]
    public bool onlyOnce = true;

    [Tooltip("If true, logic (not implemented in this snippet) prevents picking it up again.")]
    public bool disableGrabbableOnSnap = true;

    // Internal references
    private Rigidbody rb;
    private ItemGrabEvents grabEvents;
    private bool objectiveCompleted = false;
    private bool hasSnapped = false;
    private bool wasGrabbedLastFrame = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabEvents = GetComponent<ItemGrabEvents>();

        // Auto-assign ObjectiveManager if missing
        if (objectiveManager == null && ObjectiveManager.Instance != null)
            objectiveManager = ObjectiveManager.Instance;
    }

    private void Update()
    {
        // Exit if the task is done and we only do it once
        if (onlyOnce && hasSnapped) return;
        
        // Safety checks
        if (targetZone == null || snapPoint == null) return;
        if (grabEvents == null) return;  

        // Check current grab state
        bool grabbedNow = grabEvents.IsGrabbed;

        // LOGIC: Detect the specific frame where the user *releases* the object
        // (Was grabbed last frame == true, Is grabbed now == false)
        if (wasGrabbedLastFrame && !grabbedNow)
        {
            // Check spatial condition
            if (IsInsideZone())
            {
                Debug.Log($"{name} was released inside zone {targetZone.name}. Snapping to point.");
                
                // Perform actions
                SnapToPoint();
                CompleteObjective();
                
                hasSnapped = true;
            }
            else
            {
                Debug.Log($"{name} was released outside zone {targetZone.name}.");
            }
        }

        // Update state for the next frame
        wasGrabbedLastFrame = grabbedNow;
    }

    // Checks if the object's pivot point is contained within the Target Zone's bounding box
    private bool IsInsideZone()
    {
        Bounds b = targetZone.bounds;
        Vector3 pos = transform.position;
        return b.Contains(pos);
    }

    // Forces the object to the perfect position and disables physics to keep it there
    private void SnapToPoint()
    {
        Debug.Log($"SnapToPoint: moving {name} to {snapPoint.position} / rot {snapPoint.rotation.eulerAngles}");

        transform.position = snapPoint.position;
        transform.rotation = snapPoint.rotation;

        if (rb != null)
        {
            // Stop all momentum
            // Note: 'linearVelocity' is used in Unity 6+. For older versions, use 'velocity'.
            rb.linearVelocity = Vector3.zero;  
            rb.angularVelocity = Vector3.zero;
            
            // Set Kinematic so gravity/collisions don't move it anymore
            rb.isKinematic = true;            
        }
    }

    // Updates the global game state
    private void CompleteObjective()
    {
        if (objectiveCompleted) return;

        if (objectiveManager != null && !string.IsNullOrEmpty(objectiveDescription))
        {
            objectiveCompleted = true;
            Debug.Log($"Drop objective completed: {objectiveDescription}");
            objectiveManager.CompleteObjective(objectiveDescription);
            objectiveManager.PrintObjectivesStatus();
        }
    }
}
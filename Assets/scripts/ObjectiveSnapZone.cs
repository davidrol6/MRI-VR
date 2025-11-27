using UnityEngine;

public class DropInZoneObjective : MonoBehaviour
{
    [Header("Zone where the object must be dropped")]
    public Collider targetZone;        // BoxCollider of the drop zone

    [Header("Exact placement point")]
    public Transform snapPoint;        // Final position/rotation

    [Header("Linked objective")]
    public ObjectiveManager objectiveManager;
    public string objectiveDescription;

    [Tooltip("If true, snapping/completion will only happen the first time.")]
    public bool onlyOnce = true;

    [Tooltip("If true, disable the grabbable after snapping so it cannot be picked up again.")]
    public bool disableGrabbableOnSnap = true;

    private Rigidbody rb;
    private ItemGrabEvents grabEvents;
    private bool objectiveCompleted = false;
    private bool hasSnapped = false;
    private bool wasGrabbedLastFrame = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabEvents = GetComponent<ItemGrabEvents>();

        if (objectiveManager == null && ObjectiveManager.Instance != null)
            objectiveManager = ObjectiveManager.Instance;
    }

    private void Update()
    {
        if (onlyOnce && hasSnapped) return;
        if (targetZone == null || snapPoint == null) return;
        if (grabEvents == null) return;  // we need grab state

        bool grabbedNow = grabEvents.IsGrabbed;

        // Transition: was grabbed last frame and now is not → just released
        if (wasGrabbedLastFrame && !grabbedNow)
        {
            // On the frame of release, check if it's inside the zone
            if (IsInsideZone())
            {
                Debug.Log($"{name} was released inside zone {targetZone.name}. Snapping to point.");
                SnapToPoint();
                CompleteObjective();
                hasSnapped = true;
            }
            else
            {
                Debug.Log($"{name} was released outside zone {targetZone.name}.");
            }
        }

        wasGrabbedLastFrame = grabbedNow;
    }

    private bool IsInsideZone()
    {
        Bounds b = targetZone.bounds;
        Vector3 pos = transform.position;
        return b.Contains(pos);
    }

    private void SnapToPoint()
    {
        Debug.Log($"SnapToPoint: moving {name} to {snapPoint.position} / rot {snapPoint.rotation.eulerAngles}");

        // Place object exactly at the defined point
        transform.position = snapPoint.position;
        transform.rotation = snapPoint.rotation;

        // Stop physics
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;   // Unity 6
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;             // keep it fixed
        }

        // If you ever want to use 'disableGrabbableOnSnap', 
        // this is the place to disable grab components.
    }

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

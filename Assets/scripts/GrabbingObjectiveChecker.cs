using UnityEngine;

public class GrabObjective : MonoBehaviour
{
    [Header("Reference to ObjectiveManager")]
    public ObjectiveManager objectiveManager;

    [Header("Exact description of the objective")]
    public string objectiveDescription;

    [Header("Time required to complete the objective (seconds)")]
    public float requiredHoldTime = 2f;

    private float holdTimer = 0f;
    private bool isHeld = false;
    private bool objectiveCompleted = false;

    private void Awake()
    {
        // Auto-assign ObjectiveManager if not set manually
        if (objectiveManager == null && ObjectiveManager.Instance != null)
        {
            objectiveManager = ObjectiveManager.Instance;
        }
    }

    private void Update()
    {
        if (objectiveCompleted || objectiveManager == null)
            return;

        // Count time only while the object is being held
        if (isHeld)
        {
            holdTimer += Time.deltaTime;

            // Objective completed only if held long enough
            if (holdTimer >= requiredHoldTime)
            {
                objectiveCompleted = true;

                Debug.Log($"Objective completed after {requiredHoldTime} seconds: {objectiveDescription}");

                objectiveManager.CompleteObjective(objectiveDescription);
                objectiveManager.PrintObjectivesStatus();
            }
        }
        else
        {
            // Reset timer when the object is released early
            holdTimer = 0f;
        }
    }

    // These functions are triggered by Meta SDK grab events
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

using UnityEngine;

public class GrabObjective : MonoBehaviour
{
    [Header("Referencia al ObjectiveManager")]
    public ObjectiveManager objectiveManager;

    [Header("Descripción exacta del objetivo")]
    public string objectiveDescription;

    [Header("Tiempo necesario para cumplir el objetivo (segundos)")]
    public float requiredHoldTime = 2f;

    private float holdTimer = 0f;
    private bool isHeld = false;
    private bool objectiveCompleted = false;

    private void Awake()
    {
        // Por si se te olvida asignarlo a mano
        if (objectiveManager == null && ObjectiveManager.Instance != null)
        {
            objectiveManager = ObjectiveManager.Instance;
        }
    }

    private void Update()
    {
        if (objectiveCompleted || objectiveManager == null)
            return;

        if (isHeld)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= requiredHoldTime)
            {
                objectiveCompleted = true;

                Debug.Log($"🟢 Objetivo cumplido tras {requiredHoldTime} segundos: {objectiveDescription}");

                objectiveManager.CompleteObjective(objectiveDescription);
                objectiveManager.PrintObjectivesStatus();
            }
        }
        else
        {
            // Si suelta antes del tiempo, se resetea
            holdTimer = 0f;
        }
    }

    // Estas funciones se llamarán desde los eventos del Meta SDK
    public void OnGrab()
    {
        Debug.Log($"✋ OnGrab para objetivo: {objectiveDescription}");
        isHeld = true;
    }

    public void OnRelease()
    {
        Debug.Log($"❌ OnRelease para objetivo: {objectiveDescription}");
        isHeld = false;
    }
}

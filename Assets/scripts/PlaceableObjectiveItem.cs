using Oculus.Interaction;
using UnityEngine;

public class PlaceableObjectiveItem : MonoBehaviour
{
    [Header("Objetivo asociado a este objeto")]
    public ObjectiveManager objectiveManager;
    public string objectiveDescription;

    [Tooltip("Rigidbody del objeto (se busca automáticamente si no se asigna)")]
    public Rigidbody rb;

    [Tooltip("Si se completa el objetivo, no se repite")]
    public bool objectiveCompleted = false;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (objectiveManager == null && ObjectiveManager.Instance != null)
            objectiveManager = ObjectiveManager.Instance;
    }

    public void SnapTo(Transform snapPoint)
    {
        if (snapPoint == null) return;

        // 1) Colocamos posición y rotación
        transform.position = snapPoint.position;
        transform.rotation = snapPoint.rotation;

        // 2) Reset de físicas y congelar el objeto
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;   // Unity 6
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;              // que se quede fijo
        }

        // 4) Marcar objetivo
        if (!objectiveCompleted && objectiveManager != null && !string.IsNullOrEmpty(objectiveDescription))
        {
            objectiveCompleted = true;
            Debug.Log($"🟢 Objetivo por colocación cumplido: {objectiveDescription}");
            objectiveManager.CompleteObjective(objectiveDescription);
            objectiveManager.PrintObjectivesStatus();
        }
    }
}

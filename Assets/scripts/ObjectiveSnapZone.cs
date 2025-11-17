using UnityEngine;

public class DropInZoneObjective : MonoBehaviour
{
    [Header("Zona donde debe dejarse el objeto")]
    public Collider targetZone;        // BoxCollider de la zona

    [Header("Punto exacto de colocación")]
    public Transform snapPoint;        // Transform con la posición/rotación final

    [Header("Objetivo asociado")]
    public ObjectiveManager objectiveManager;
    public string objectiveDescription;

    [Tooltip("Si es true, solo se completará/snapeará la primera vez.")]
    public bool onlyOnce = true;

    [Tooltip("Si es true, desactiva el Grabbable al hacer snap para evitar que vuelva a la mano.")]
    public bool disableGrabbableOnSnap = true;

    private Rigidbody rb;
    private ItemGrabEvents grabEvents;   // 👈 el script que ya tienes en el objeto
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
        if (grabEvents == null) return;  // necesitamos saber si está agarrado

        bool grabbedNow = grabEvents.IsGrabbed;

        // Transición: ESTABA agarrado y AHORA ya no → acaban de soltarlo
        if (wasGrabbedLastFrame && !grabbedNow)
        {
            // Justo en el frame en el que se ha soltado miramos si está dentro de la zona
            if (IsInsideZone())
            {
                Debug.Log($"📌 {name} se ha soltado dentro de la zona {targetZone.name}. Haciendo snap limpio…");
                SnapToPoint();
                CompleteObjective();
                hasSnapped = true;
            }
            else
            {
                Debug.Log($"{name} se ha soltado fuera de la zona {targetZone.name}.");
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
        Debug.Log($"🔧 SnapToPoint: moviendo {name} a {snapPoint.position} / rot {snapPoint.rotation.eulerAngles}");

        // Colocar el objeto exactamente en el punto definido
        transform.position = snapPoint.position;
        transform.rotation = snapPoint.rotation;

        // Parar físicas
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;   // Unity 6
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;             // que se quede fijo
        }

        
    }

    private void CompleteObjective()
    {
        if (objectiveCompleted) return;

        if (objectiveManager != null && !string.IsNullOrEmpty(objectiveDescription))
        {
            objectiveCompleted = true;
            Debug.Log($"🟢 Objetivo por colocar objeto cumplido: {objectiveDescription}");
            objectiveManager.CompleteObjective(objectiveDescription);
            objectiveManager.PrintObjectivesStatus();
        }
    }
}

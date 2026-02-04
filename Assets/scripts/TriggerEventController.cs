using UnityEngine;
using UnityEngine.Events;

public class TriggerManager : MonoBehaviour
{
    [Header("Configuración de Referencias")]
    [Tooltip("El objeto que tiene el Collider físico (debe ser Trigger)")]
    public GameObject detectorArea;

    [Tooltip("El objeto principal que quieres detectar (ej: el Player)")]
    public GameObject targetToDetect;

    [Header("Eventos de Acción")]
    public UnityEvent onInside;
    public UnityEvent onOutside;

    private void Start()
    {
        if (detectorArea == null || targetToDetect == null)
        {
            Debug.LogError($"[AliTrigger] Faltan referencias en {gameObject.name}");
            return;
        }

        // Inyectamos el componente "oyente" en el detector automáticamente
        var proxy = detectorArea.GetComponent<AliTriggerProxy>();
        if (proxy == null) proxy = detectorArea.AddComponent<AliTriggerProxy>();

        // Nos suscribimos a sus señales
        proxy.OnTriggerEntered += CheckEntry;
        proxy.OnTriggerExited += CheckExit;
    }

    private void CheckEntry(Collider other)
    {
        // Lógica estilo PassengerZone: buscamos la raíz o el Rigidbody
        Transform incomingRoot = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;

        // Comprobamos si lo que ha entrado es el target o un hijo del target
        if (incomingRoot == targetToDetect.transform || incomingRoot.IsChildOf(targetToDetect.transform))
        {
            onInside.Invoke();
            Debug.Log($"<color=cyan>Ali, el objeto {targetToDetect.name} ha entrado correctamente.</color>");
        }
    }

    private void CheckExit(Collider other)
    {
        Transform incomingRoot = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;

        if (incomingRoot == targetToDetect.transform || incomingRoot.IsChildOf(targetToDetect.transform))
        {
            onOutside.Invoke();
            Debug.Log($"<color=orange>Ali, el objeto {targetToDetect.name} ha salido.</color>");
        }
    }
}

// Componente auxiliar que vive en el objeto con el Collider
public class AliTriggerProxy : MonoBehaviour 
{
    public System.Action<Collider> OnTriggerEntered;
    public System.Action<Collider> OnTriggerExited;

    private void OnTriggerEnter(Collider other) => OnTriggerEntered?.Invoke(other);
    private void OnTriggerExit(Collider other) => OnTriggerExited?.Invoke(other);
}
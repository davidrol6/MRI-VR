using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contenedor simple por Parenting: todo Rigidbody que entre en el trigger
/// pasa a ser hijo de la caja. Al salir, se restaura su parent original.
/// </summary>
public class GrabContainerParenting : MonoBehaviour
{
    [Header("Volumen interior (Trigger)")]
    [Tooltip("Collider marcado como IsTrigger que define el interior de la caja.")]
    public Collider containerTrigger;

    [Header("Filtros (opcionales)")]
    [Tooltip("Capas consideradas 'items' capturables. Por defecto, todas.")]
    public LayerMask itemLayers = ~0;
    [Tooltip("Si no está vacío, sólo capturará objetos con estos tags.")]
    public List<string> allowedTags = new();

    [Header("Colisiones")]
    [Tooltip("Ignorar colisiones entre el item y la caja mientras esté dentro.")]
    public bool ignoreItemVsContainerColliders = true;

    // Guarda el parent original para restaurarlo al salir
    private readonly Dictionary<Transform, Transform> _originalParents = new();
    // Colliders de la caja (para ignorar colisiones si procede)
    private readonly HashSet<Collider> _containerColliders = new();

    private void Reset()
    {
        // Intenta autoasignar el primer collider hijo como trigger
        if (containerTrigger == null)
            containerTrigger = GetComponentInChildren<Collider>();
    }

    private void Awake()
    {
        if (containerTrigger == null)
        {
            Debug.LogError("[GrabContainerParenting] Asigna 'containerTrigger' (Collider con IsTrigger).");
            return;
        }
        if (!containerTrigger.isTrigger)
        {
            Debug.LogWarning("[GrabContainerParenting] 'containerTrigger' no es Trigger. Lo marco como Trigger.");
            containerTrigger.isTrigger = true;
        }

        // Cachea todos los colliders de la caja (menos el trigger) para gestionar IgnoreCollision
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            if (col != containerTrigger)
                _containerColliders.Add(col);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsMyTrigger(other)) return;

        var rb = other.attachedRigidbody;
        if (rb == null) return;
        if (!IsInLayerMask(rb.gameObject.layer, itemLayers)) return;
        if (!PassesTagFilter(rb.gameObject)) return;

        var t = rb.transform;

        // Evita duplicados
        if (_originalParents.ContainsKey(t)) return;

        // Guarda parent original
        _originalParents[t] = t.parent;

        // Ignora colisiones con la caja si se pide
        if (ignoreItemVsContainerColliders)
            ToggleIgnoreCollisions(rb, true);

        // Parent a la caja manteniendo pose mundial
        t.SetParent(transform, true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsMyTrigger(other)) return;

        var rb = other.attachedRigidbody;
        if (rb == null) return;
        var t = rb.transform;

        // Restaura parent original si lo teníamos guardado
        if (_originalParents.TryGetValue(t, out var originalParent))
        {
            t.SetParent(originalParent, true);
            _originalParents.Remove(t);
        }

        // Restaura colisiones
        if (ignoreItemVsContainerColliders)
            ToggleIgnoreCollisions(rb, false);
    }

    private void OnDisable()
    {
        // Limpieza por si desactivas la caja con objetos dentro
        foreach (var kv in _originalParents)
        {
            var t = kv.Key;
            var originalParent = kv.Value;
            if (t != null)
            {
                t.SetParent(originalParent, true);
                var rb = t.GetComponentInParent<Rigidbody>();
                if (ignoreItemVsContainerColliders && rb != null)
                    ToggleIgnoreCollisions(rb, false);
            }
        }
        _originalParents.Clear();
    }

    // --- Utilidades ---

    private bool IsMyTrigger(Collider other)
    {
        // Acepta eventos generados por cualquier collider del item, siempre que provengan de MI trigger
        // (Unity llamará a OnTriggerEnter en este script cuando el collider 'containerTrigger' interactúe)
        return containerTrigger != null && other != null;
    }

    private static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    private bool PassesTagFilter(GameObject go)
    {
        if (allowedTags == null || allowedTags.Count == 0) return true;
        foreach (var tag in allowedTags)
            if (!string.IsNullOrEmpty(tag) && go.CompareTag(tag))
                return true;
        return false;
    }

    private void ToggleIgnoreCollisions(Rigidbody itemRb, bool ignore)
    {
        if (itemRb == null) return;
        foreach (var itemCol in itemRb.GetComponentsInChildren<Collider>())
        {
            foreach (var contCol in _containerColliders)
            {
                if (contCol == null || itemCol == null) continue;
                Physics.IgnoreCollision(itemCol, contCol, ignore);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (containerTrigger is BoxCollider box)
        {
            Gizmos.color = new Color(0f, 0.6f, 1f, 0.15f);
            Gizmos.matrix = box.transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(0f, 0.6f, 1f, 0.8f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
#endif
}

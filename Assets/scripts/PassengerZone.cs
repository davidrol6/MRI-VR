using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks objects that are "inside" (via trigger) and can move them by a world-space delta.
/// Attach to a child with a Trigger collider inside your moving box.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PassengerZone : MonoBehaviour
{
    [Tooltip("Owner to ignore (the moving box root). Anything under this will be ignored.")]
    public Transform owner;

    [Tooltip("Optional: only move objects on these layers. Leave empty to allow all.")]
    public LayerMask allowedLayers = ~0;

    [Tooltip("If true and a non-kinematic Rigidbody is found, use MovePosition. Otherwise set transform.position.")]
    public bool useRigidbodyIfPresent = true;

    private readonly HashSet<Transform> _passengers = new HashSet<Transform>();
    private Collider _trigger;

    void Awake()
    {
        _trigger = GetComponent<Collider>();
        _trigger.isTrigger = true;
        if (owner == null) owner = transform.root;
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & allowedLayers) == 0) return;

        var t = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;

        // Ignore self/children of owner
        if (t == owner || t.IsChildOf(owner)) return;

        _passengers.Add(t);
    }

    void OnTriggerExit(Collider other)
    {
        var t = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;
        _passengers.Remove(t);
    }

    /// <summary>Apply a world-space delta to all current passengers.</summary>
    public void ApplyDelta(Vector3 worldDelta)
    {
        if (worldDelta.sqrMagnitude < 1e-12f) return;

        foreach (var t in _passengers)
        {
            if (!t) continue;
            var rb = t.GetComponent<Rigidbody>();
            if (rb != null && useRigidbodyIfPresent && !rb.isKinematic)
            {
                // Note: ideally call from FixedUpdate; for simple kinematic platforms this is fine.
                rb.MovePosition(rb.position + worldDelta);
            }
            else
            {
                t.position += worldDelta;
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lets a container carry rigidbodies placed inside its trigger.
/// Two options:
///  - ParentToNeutral: items are parented to a neutral child transform.
///  - PhysicsFollow: no parenting; items follow the container using Rigidbody.MovePosition/MoveRotation.
/// Optionally ignores collisions with the container while items are inside.
/// </summary>
public class GrabContainerCarrier : MonoBehaviour
{
    public enum CarryMode { PhysicsFollow, ParentToNeutral }

    [Header("Mode")]
    public CarryMode mode = CarryMode.PhysicsFollow;

    [Header("Volume (Trigger)")]
    [Tooltip("Collider marked as IsTrigger that defines the interior of the box.")]
    public Collider containerTrigger;

    [Header("ParentToNeutral mode")]
    [Tooltip("Child transform with identity rotation and scale (1,1,1). Items will be parented here.")]
    public Transform neutralParent;

    [Header("Filters (optional)")]
    public LayerMask itemLayers = ~0;
    public List<string> allowedTags = new();

    [Header("Collisions")]
    [Tooltip("Ignore collisions between the item and the container while inside.")]
    public bool ignoreItemVsContainerColliders = true;

    private readonly HashSet<Collider> _containerColliders = new();
    private readonly Dictionary<Rigidbody, CarryInfo> _inside = new();

    private class CarryInfo
    {
        public Transform originalParent;
        public Vector3 localOffset;      // PhysicsFollow
        public Quaternion localRotation; // PhysicsFollow
        public ItemGrabEvents grab;      // optional (to check if item is grabbed)
    }

    private void Reset()
    {
        // Try to auto-assign a trigger collider from children
        if (containerTrigger == null)
            containerTrigger = GetComponentInChildren<Collider>();
    }

    private void Awake()
    {
        if (containerTrigger == null)
        {
            Debug.LogError("[GrabContainerCarrier] Assign 'containerTrigger' (IsTrigger).");
            enabled = false;
            return;
        }
        if (!containerTrigger.isTrigger)
        {
            Debug.LogWarning("[GrabContainerCarrier] 'containerTrigger' was not Trigger. Forcing IsTrigger=true.");
            containerTrigger.isTrigger = true;
        }

        // Cache all colliders that belong to the container (except the trigger)
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            if (col != containerTrigger)
                _containerColliders.Add(col);
        }

        if (mode == CarryMode.ParentToNeutral)
        {
            if (neutralParent == null)
            {
                // Auto-create a neutral child if missing
                var go = new GameObject("_CargoRoot (neutral)");
                neutralParent = go.transform;
                neutralParent.SetParent(transform, false);
                neutralParent.localPosition = Vector3.zero;
                neutralParent.localRotation = Quaternion.identity;
                neutralParent.localScale = Vector3.one;
                Debug.LogWarning("[GrabContainerCarrier] Created a neutral parent automatically. Check your scaling to avoid deformation.");
            }
            else
            {
                // Neutral parent should be clean to avoid weird scaling
                if (neutralParent.localScale != Vector3.one || neutralParent.localRotation != Quaternion.identity)
                {
                    Debug.LogWarning("[GrabContainerCarrier] 'neutralParent' should have localScale (1,1,1) and identity rotation.");
                }
            }
        }

        // Warn if the container has non-uniform world scale (can cause shear with parenting)
        var s = transform.lossyScale;
        if (!Mathf.Approximately(s.x, s.y) || !Mathf.Approximately(s.y, s.z))
        {
            Debug.LogWarning($"[GrabContainerCarrier] Non-uniform lossyScale on container ({s.x:F3},{s.y:F3},{s.z:F3}). Prefer PhysicsFollow or a neutral parent.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsMyTriggerEvent(other)) return;

        var rb = other.attachedRigidbody;
        if (rb == null) return;
        if (!IsInLayerMask(rb.gameObject.layer, itemLayers)) return;
        if (!PassesTagFilter(rb.gameObject)) return;

        if (_inside.ContainsKey(rb)) return; // already tracked

        var info = new CarryInfo
        {
            originalParent = rb.transform.parent,
            grab = rb.GetComponent<ItemGrabEvents>()
        };
        _inside[rb] = info;

        if (ignoreItemVsContainerColliders)
            ToggleIgnoreCollisions(rb, true);

        if (mode == CarryMode.ParentToNeutral)
        {
            // Parent under neutral child (or container itself if neutral is missing)
            rb.transform.SetParent(neutralParent != null ? neutralParent : transform, true);
        }
        else // PhysicsFollow
        {
            // Store pose relative to the container at entry time
            info.localOffset   = transform.InverseTransformPoint(rb.position);
            info.localRotation = Quaternion.Inverse(transform.rotation) * rb.rotation;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsMyTriggerEvent(other)) return;

        var rb = other.attachedRigidbody;
        if (rb == null) return;

        if (_inside.TryGetValue(rb, out var info))
        {
            // Restore original parent if we had parented it
            if (mode == CarryMode.ParentToNeutral)
            {
                rb.transform.SetParent(info.originalParent, true);
            }

            if (ignoreItemVsContainerColliders)
                ToggleIgnoreCollisions(rb, false);

            _inside.Remove(rb);
        }
    }

    private void FixedUpdate()
    {
        // Only relevant in PhysicsFollow mode
        if (mode != CarryMode.PhysicsFollow) return;
        if (_inside.Count == 0) return;

        foreach (var kv in _inside)
        {
            var rb = kv.Key;
            var info = kv.Value;

            if (rb == null) continue;

            // If the item is grabbed, let the player control it
            if (info.grab != null && info.grab.IsGrabbed) continue;

            // Target pose = container transform * stored local pose
            Vector3 targetPos = transform.TransformPoint(info.localOffset);
            Quaternion targetRot = transform.rotation * info.localRotation;

            // Move via physics API to avoid teleporting
            rb.MovePosition(targetPos);
            rb.MoveRotation(targetRot);
        }
    }

    private void OnDisable()
    {
        // Clean restore: parents and collision state
        foreach (var kv in _inside)
        {
            var rb = kv.Key;
            var info = kv.Value;
            if (rb == null) continue;

            if (mode == CarryMode.ParentToNeutral && info.originalParent != null)
                rb.transform.SetParent(info.originalParent, true);

            if (ignoreItemVsContainerColliders)
                ToggleIgnoreCollisions(rb, false);
        }
        _inside.Clear();
    }

    private bool IsMyTriggerEvent(Collider other)
    {
        // Guard just in case; this script should only receive events from our trigger
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
            if (itemCol == null) continue;

            foreach (var contCol in _containerColliders)
            {
                if (contCol == null) continue;
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

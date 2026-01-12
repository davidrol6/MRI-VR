/* * SCRIPT SUMMARY:
 * This script enables a container (like a box, tray, or cart) to carry physical objects placed inside it.
 * * * How it works:
 * - It uses a Trigger Collider to define the "inside" volume of the container.
 * - When an object enters, it is registered and "carried" using one of two methods:
 * 1. PhysicsFollow: The object's Rigidbody is kinematically moved to stay relative to the container (preserves physics, avoids scaling issues).
 * 2. ParentToNeutral: The object is temporarily parented to the container (simpler hierarchy, but sensitive to non-uniform scaling).
 * - It optionally disables collisions between the item and the container walls to prevent physics jitter/explosions.
 * - When the object is removed or the player grabs it, the script restores its original state.
 */

using System.Collections.Generic;
using UnityEngine;

public class GrabContainerCarrier : MonoBehaviour
{
    public enum CarryMode { PhysicsFollow, ParentToNeutral }

    [Header("Mode")]
    public CarryMode mode = CarryMode.PhysicsFollow; // Determines how items stick to the container

    [Header("Volume (Trigger)")]
    [Tooltip("Collider marked as IsTrigger that defines the interior of the box.")]
    public Collider containerTrigger;

    [Header("ParentToNeutral mode")]
    [Tooltip("Child transform with identity rotation and scale (1,1,1). Items will be parented here.")]
    public Transform neutralParent;

    [Header("Filters (optional)")]
    public LayerMask itemLayers = ~0;       // Filter which layers can be carried
    public List<string> allowedTags = new(); // Filter which tags can be carried

    [Header("Collisions")]
    [Tooltip("Ignore collisions between the item and the container while inside.")]
    public bool ignoreItemVsContainerColliders = true; // Essential to prevent physics glitches

    // Internal tracking
    private readonly HashSet<Collider> _containerColliders = new();
    private readonly Dictionary<Rigidbody, CarryInfo> _inside = new();

    // Helper class to store the original state of an object before it was picked up by the container
    private class CarryInfo
    {
        public Transform originalParent;
        public Vector3 localOffset;      // Used for PhysicsFollow
        public Quaternion localRotation; // Used for PhysicsFollow
        public ItemGrabEvents grab;      // Optional reference to check if the player is currently holding it
    }

    private void Reset()
    {
        // specific logic to auto-assign a trigger collider from children if available
        if (containerTrigger == null)
            containerTrigger = GetComponentInChildren<Collider>();
    }

    private void Awake()
    {
        // 1. Validation: Ensure the trigger exists and is configured correctly
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

        // 2. Cache physical colliders of the container (to ignore collisions with them later)
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            if (col != containerTrigger)
                _containerColliders.Add(col);
        }

        // 3. Setup for Parent mode
        if (mode == CarryMode.ParentToNeutral)
        {
            if (neutralParent == null)
            {
                // Auto-create a neutral child if missing to ensure clean scaling
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
                // Ensure the user-assigned parent is clean
                if (neutralParent.localScale != Vector3.one || neutralParent.localRotation != Quaternion.identity)
                {
                    Debug.LogWarning("[GrabContainerCarrier] 'neutralParent' should have localScale (1,1,1) and identity rotation.");
                }
            }
        }

        // 4. Warn if the container has non-uniform world scale (can cause shear/distortion with parenting)
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
        
        // Check filters
        if (!IsInLayerMask(rb.gameObject.layer, itemLayers)) return;
        if (!PassesTagFilter(rb.gameObject)) return;

        if (_inside.ContainsKey(rb)) return; // Object is already tracked

        // Create tracking info
        var info = new CarryInfo
        {
            originalParent = rb.transform.parent,
            grab = rb.GetComponent<ItemGrabEvents>()
        };
        _inside[rb] = info;

        // Disable physical collision to stop jitter
        if (ignoreItemVsContainerColliders)
            ToggleIgnoreCollisions(rb, true);

        // Apply carrying logic
        if (mode == CarryMode.ParentToNeutral)
        {
            // Parent under neutral child (or container itself if neutral is missing)
            rb.transform.SetParent(neutralParent != null ? neutralParent : transform, true);
        }
        else // PhysicsFollow
        {
            // Calculate and store the object's position relative to the container at the moment of entry
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
            // Restore original parent if we had changed it
            if (mode == CarryMode.ParentToNeutral)
            {
                rb.transform.SetParent(info.originalParent, true);
            }

            // Re-enable collisions
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

            // If the item is currently being grabbed by the player, do not force its position
            if (info.grab != null && info.grab.IsGrabbed) continue;

            // Calculate target world position: Container Transform + Stored Offset
            Vector3 targetPos = transform.TransformPoint(info.localOffset);
            Quaternion targetRot = transform.rotation * info.localRotation;

            // Move via physics API to avoid teleporting (smoother interactions)
            rb.MovePosition(targetPos);
            rb.MoveRotation(targetRot);
        }
    }

    private void OnDisable()
    {
        // Safety Clean-up: If the script is disabled, release all items
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

    // --- Helper Methods ---

    private bool IsMyTriggerEvent(Collider other)
    {
        // Ensure this script only responds to events from its specific trigger
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

    // Loops through all colliders of the item and all colliders of the container to ignore/restore collision
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
    // Visual aid in the editor to see the trigger volume
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
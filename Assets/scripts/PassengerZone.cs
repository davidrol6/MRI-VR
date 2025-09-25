using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks objects that are "inside" (via trigger) and moves them either:
///  (A) manually, by calling ApplyDelta(worldDelta), or
///  (B) automatically, by following the owner's pose delta each step (translation + rotation).
/// Attach to a child with a Trigger collider inside your moving box.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PassengerZone : MonoBehaviour
{
    public enum FollowMode { Manual, AutoFollowOwner }
    public enum Tick { FixedUpdate, LateUpdate }

    [Header("Owner / Auto-Follow")]
    [Tooltip("Owner to ignore (the moving box root). Anything under this will be ignored.")]
    public Transform owner;
    [Tooltip("Choose Manual to call ApplyDelta yourself; AutoFollowOwner to track the owner's pose.")]
    public FollowMode followMode = FollowMode.AutoFollowOwner;
    [Tooltip("When AutoFollowOwner is on, which Unity loop should apply motion.")]
    public Tick followTick = Tick.FixedUpdate;

    [Header("Filters")]
    [Tooltip("Only move objects on these layers.")]
    public LayerMask allowedLayers = ~0;

    [Header("Motion Options")]
    [Tooltip("If true and a Rigidbody is found, use MovePosition/MoveRotation (preferred).")]
    public bool useRigidbodyIfPresent = true;
    [Tooltip("If true and a CharacterController is found, use CharacterController.Move for translation.")]
    public bool useCharacterControllerIfPresent = true;
    [Tooltip("Rotate passengers with the owner when AutoFollowOwner is enabled.")]
    public bool rotatePassengers = true;
    [Tooltip("Skip moving an item while it's grabbed (ItemGrabEvents.IsGrabbed).")]
    public bool skipWhileGrabbed = true;
    [Tooltip("Wake sleeping rigidbodies before MovePosition/MoveRotation.")]
    public bool wakeRigidbodies = true;

    private readonly HashSet<Transform> _passengers = new HashSet<Transform>();
    private Collider _trigger;

    // Cached previous owner pose for AutoFollowOwner
    private Vector3 _prevOwnerPos;
    private Quaternion _prevOwnerRot;

    void Awake()
    {
        _trigger = GetComponent<Collider>();
        _trigger.isTrigger = true;
        if (owner == null) owner = transform.root;
        CacheOwnerPose();
    }

    void OnEnable() => CacheOwnerPose();

    private void CacheOwnerPose()
    {
        if (owner != null)
        {
            _prevOwnerPos = owner.position;
            _prevOwnerRot = owner.rotation;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & allowedLayers) == 0) return;

        var t = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;

        // Ignore self/children of owner
        if (t == owner || (owner != null && t.IsChildOf(owner))) return;

        _passengers.Add(t);
    }

    void OnTriggerExit(Collider other)
    {
        var t = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;
        _passengers.Remove(t);
    }

    void OnDisable() => _passengers.Clear();

    // ---------- Manual API ----------
    /// <summary>Apply a world-space translation delta to all current passengers.</summary>
    public void ApplyDelta(Vector3 worldDelta)
    {
        if (worldDelta.sqrMagnitude < 1e-12f) return;

        foreach (var t in _passengers)
        {
            if (!t) continue;

            if (skipWhileGrabbed)
            {
                var grab = t.GetComponent<ItemGrabEvents>();
                if (grab != null && grab.IsGrabbed) continue;
            }

            // CharacterController takes precedence for translation
            var cc = useCharacterControllerIfPresent ? t.GetComponent<CharacterController>() : null;
            if (cc != null && cc.enabled)
            {
                cc.Move(worldDelta);
                continue;
            }

            var rb = useRigidbodyIfPresent ? t.GetComponent<Rigidbody>() : null;
            if (rb != null)
            {
                if (wakeRigidbodies) rb.WakeUp();
                rb.MovePosition(rb.position + worldDelta);
            }
            else
            {
                t.position += worldDelta;
            }
        }
    }

    // ---------- Auto-Follow (translation + rotation) ----------
    void FixedUpdate()
    {
        if (followMode == FollowMode.AutoFollowOwner && followTick == Tick.FixedUpdate)
            FollowOwnerOnce();
    }

    void LateUpdate()
    {
        if (followMode == FollowMode.AutoFollowOwner && followTick == Tick.LateUpdate)
            FollowOwnerOnce();
    }

    private void FollowOwnerOnce()
    {
        if (owner == null || _passengers.Count == 0) { CacheOwnerPose(); return; }

        // Compute owner's pose delta since last tick
        Vector3 newPos = owner.position;
        Quaternion newRot = owner.rotation;

        Quaternion deltaRot = newRot * Quaternion.Inverse(_prevOwnerRot);
        Vector3 deltaPos = newPos - _prevOwnerPos;

        // For a point p: p' = deltaRot * (p - prevOwnerPos) + newPos
        foreach (var t in _passengers)
        {
            if (!t) continue;

            if (skipWhileGrabbed)
            {
                var grab = t.GetComponent<ItemGrabEvents>();
                if (grab != null && grab.IsGrabbed) continue;
            }

            Vector3 p = t.position;
            Vector3 pPrime = deltaRot * (p - _prevOwnerPos) + newPos;

            // Translation delta for CharacterController branch
            Vector3 worldDelta = pPrime - p;

            // CharacterController translation
            var cc = useCharacterControllerIfPresent ? t.GetComponent<CharacterController>() : null;
            if (cc != null && cc.enabled)
            {
                cc.Move(worldDelta);
                if (rotatePassengers)
                {
                    // CC has no built-in rotation; rotate transform directly
                    t.rotation = deltaRot * t.rotation;
                }
                continue;
            }

            var rb = useRigidbodyIfPresent ? t.GetComponent<Rigidbody>() : null;
            if (rb != null)
            {
                if (wakeRigidbodies) rb.WakeUp();
                rb.MovePosition(pPrime);
                if (rotatePassengers)
                    rb.MoveRotation(deltaRot * rb.rotation);
            }
            else
            {
                t.position = pPrime;
                if (rotatePassengers)
                    t.rotation = deltaRot * t.rotation;
            }
        }

        // Update cache
        _prevOwnerPos = newPos;
        _prevOwnerRot = newRot;
    }
}

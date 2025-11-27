using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class PassengerZone : MonoBehaviour
{
    public enum FollowMode { Manual, AutoFollowOwner }
    public enum Tick { FixedUpdate, LateUpdate }

    [Header("Owner / Auto-Follow")]
    [Tooltip("Root of the moving object. Anything under this will be ignored as a passenger.")]
    public Transform owner;

    [Tooltip("Manual: call ApplyDelta yourself. AutoFollowOwner: this zone follows the owner's movement.")]
    public FollowMode followMode = FollowMode.AutoFollowOwner;

    [Tooltip("When AutoFollowOwner is active, choose the update loop to apply motion.")]
    public Tick followTick = Tick.FixedUpdate;

    [Header("Filters")]
    [Tooltip("Only move objects on these layers.")]
    public LayerMask allowedLayers = ~0;

    [Header("Motion Options")]
    [Tooltip("If true and a Rigidbody exists, use MovePosition/MoveRotation.")]
    public bool useRigidbodyIfPresent = true;

    [Tooltip("If true and a CharacterController exists, use CharacterController.Move for translation.")]
    public bool useCharacterControllerIfPresent = true;

    [Tooltip("Rotate passengers with the owner when AutoFollowOwner is enabled.")]
    public bool rotatePassengers = true;

    [Tooltip("Skip moving an item while it's grabbed (ItemGrabEvents.IsGrabbed).")]
    public bool skipWhileGrabbed = true;

    [Tooltip("Wake sleeping rigidbodies before MovePosition/MoveRotation.")]
    public bool wakeRigidbodies = true;

    private readonly HashSet<Transform> _passengers = new HashSet<Transform>();
    private Collider _trigger;

    // Previous owner pose for AutoFollowOwner
    private Vector3 _prevOwnerPos;
    private Quaternion _prevOwnerRot;

    private void Awake()
    {
        _trigger = GetComponent<Collider>();
        _trigger.isTrigger = true;

        if (owner == null)
            owner = transform.root;

        CacheOwnerPose();
    }

    private void OnEnable() => CacheOwnerPose();

    private void CacheOwnerPose()
    {
        if (owner != null)
        {
            _prevOwnerPos = owner.position;
            _prevOwnerRot = owner.rotation;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Layer filter
        if (((1 << other.gameObject.layer) & allowedLayers) == 0) 
            return;

        var t = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;

        // Ignore the owner and its children
        if (t == owner || (owner != null && t.IsChildOf(owner))) 
            return;

        _passengers.Add(t);
    }

    private void OnTriggerExit(Collider other)
    {
        var t = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;
        _passengers.Remove(t);
    }

    private void OnDisable() => _passengers.Clear();

    // ---------- Manual API ----------

    /// Apply a world-space translation to all current passengers.
    public void ApplyDelta(Vector3 worldDelta)
    {
        if (worldDelta.sqrMagnitude < 1e-12f) return;

        foreach (var t in _passengers)
        {
            if (!t) continue;

            if (skipWhileGrabbed)
            {
                var grab = t.GetComponent<ItemGrabEvents>();
                if (grab != null && grab.IsGrabbed) 
                    continue;
            }

            // CharacterController has priority for translation
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

    private void FixedUpdate()
    {
        if (followMode == FollowMode.AutoFollowOwner && followTick == Tick.FixedUpdate)
            FollowOwnerOnce();
    }

    private void LateUpdate()
    {
        if (followMode == FollowMode.AutoFollowOwner && followTick == Tick.LateUpdate)
            FollowOwnerOnce();
    }

    private void FollowOwnerOnce()
    {
        if (owner == null || _passengers.Count == 0)
        {
            CacheOwnerPose();
            return;
        }

        // Owner movement since last tick
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
                if (grab != null && grab.IsGrabbed) 
                    continue;
            }

            Vector3 p = t.position;
            Vector3 pPrime = deltaRot * (p - _prevOwnerPos) + newPos;

            // Translation delta for CharacterController path
            Vector3 worldDelta = pPrime - p;

            // CharacterController translation
            var cc = useCharacterControllerIfPresent ? t.GetComponent<CharacterController>() : null;
            if (cc != null && cc.enabled)
            {
                cc.Move(worldDelta);
                if (rotatePassengers)
                {
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

        // Update cached pose
        _prevOwnerPos = newPos;
        _prevOwnerRot = newRot;
    }
}

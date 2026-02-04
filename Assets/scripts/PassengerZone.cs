/* * SCRIPT SUMMARY:
 * This script allows a moving platform (like an MRI bed or elevator) to carry other objects seamlessly.
 * * * How it works:
 * - It uses a Trigger Collider to detect "Passengers" (objects standing on the platform).
 * - It tracks the movement of its "Owner" (the moving platform root) frame by frame.
 * - When the owner moves or rotates, this script calculates the exact delta (difference) and applies it to all passengers.
 * - It supports multiple movement types:
 * 1. CharacterController (for the VR player or NPCs).
 * 2. Rigidbody (for physics objects).
 * 3. Transform (for simple objects).
 * - This prevents the common issue where moving platforms slide out from under physics objects or players.
 */

using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class PassengerZone : MonoBehaviour
{
    public enum FollowMode { Manual, AutoFollowOwner }
    public enum Tick { FixedUpdate, LateUpdate }

    [Header("Owner / Auto-Follow")]
    [Tooltip("Root of the moving object. Anything under this will be ignored as a passenger.")]
    public Transform owner; // The platform itself (e.g., the Bed)

    [Tooltip("Manual: call ApplyDelta yourself. AutoFollowOwner: this zone follows the owner's movement automatically.")]
    public FollowMode followMode = FollowMode.AutoFollowOwner;

    [Tooltip("When AutoFollowOwner is active, choose the update loop to apply motion (Physics vs Render loop).")]
    public Tick followTick = Tick.FixedUpdate;

    [Header("Filters")]
    [Tooltip("Only move objects on these layers (e.g., Player, Props).")]
    public LayerMask allowedLayers = ~0; // Default: All layers

    [Header("Motion Options")]
    [Tooltip("If true and a Rigidbody exists, use MovePosition/MoveRotation (Best for physics stability).")]
    public bool useRigidbodyIfPresent = true;

    [Tooltip("If true and a CharacterController exists, use CharacterController.Move for translation (Best for Players).")]
    public bool useCharacterControllerIfPresent = true;

    [Tooltip("Rotate passengers with the owner when AutoFollowOwner is enabled.")]
    public bool rotatePassengers = true;

    [Tooltip("Skip moving an item while it's grabbed (ItemGrabEvents.IsGrabbed) so the hand remains in control.")]
    public bool skipWhileGrabbed = true;

    [Tooltip("Wake sleeping rigidbodies before MovePosition/MoveRotation.")]
    public bool wakeRigidbodies = true;

    // Internal tracking
    private readonly HashSet<Transform> _passengers = new HashSet<Transform>();
    private Collider _trigger;

    // Previous owner pose for AutoFollowOwner logic
    private Vector3 _prevOwnerPos;
    private Quaternion _prevOwnerRot;

    private void Awake()
    {
        // Ensure the collider is a Trigger (detects entry without blocking physics)
        _trigger = GetComponent<Collider>();
        _trigger.isTrigger = true;

        // Auto-assign owner to root if not specified
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

    // --- Passenger Detection ---

    private void OnTriggerEnter(Collider other)
    {
        // 1. Layer filter check
        if (((1 << other.gameObject.layer) & allowedLayers) == 0) 
            return;

        // Get the relevant transform (Rigidbody root if it exists, otherwise the object itself)
        var t = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;

        // 2. Ignore the platform itself and its children (prevent the bed from trying to carry itself)
        if (t == owner || (owner != null && t.IsChildOf(owner))) 
            return;

        // Add to the list of tracked passengers
        _passengers.Add(t);
    }

    private void OnTriggerExit(Collider other)
    {
        var t = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;
        _passengers.Remove(t);
    }

    private void OnDisable() => _passengers.Clear();

    // ---------- Manual API ----------
    // Called explicitly by other scripts (e.g., ButtonMovesObject) when they move the platform manually

    /// Apply a world-space translation to all current passengers.
    public void ApplyDelta(Vector3 worldDelta)
    {
        if (worldDelta.sqrMagnitude < 1e-12f) return;

        foreach (var t in _passengers)
        {
            if (!t) continue;

            // Optional: Don't move objects held by the player (avoids "tug of war" with the hand)
            if (skipWhileGrabbed)
            {
                var grab = t.GetComponent<ItemGrabEvents>();
                if (grab != null && grab.IsGrabbed) 
                    continue;
            }

            // Priority 1: CharacterController (VR Player)
            var cc = useCharacterControllerIfPresent ? t.GetComponent<CharacterController>() : null;
            if (cc != null && cc.enabled)
            {
                cc.Move(worldDelta);
                continue;
            }

            // Priority 2: Rigidbody (Physics Objects)
            var rb = useRigidbodyIfPresent ? t.GetComponent<Rigidbody>() : null;
            if (rb != null)
            {
                if (wakeRigidbodies) rb.WakeUp();
                rb.MovePosition(rb.position + worldDelta);
            }
            else
            {
                // Priority 3: Direct Transform (Simple Objects)
                t.position += worldDelta;
            }
        }
    }

    // ---------- Auto-Follow (translation + rotation) ----------
    // Automatically detects movement of the "Owner" and syncs it to passengers

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

        // 1. Calculate how much the owner moved/rotated since the last frame
        Vector3 newPos = owner.position;
        Quaternion newRot = owner.rotation;

        Quaternion deltaRot = newRot * Quaternion.Inverse(_prevOwnerRot);
        Vector3 deltaPos = newPos - _prevOwnerPos;

        // 2. Apply that same transformation to every passenger
        // Math formula: p' = deltaRot * (p - prevOwnerPos) + newPos
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
            // Calculate where the passenger *should* be if they were glued to the platform
            Vector3 pPrime = deltaRot * (p - _prevOwnerPos) + newPos;

            // Calculate the translation needed
            Vector3 worldDelta = pPrime - p;

            // Priority 1: CharacterController
            var cc = useCharacterControllerIfPresent ? t.GetComponent<CharacterController>() : null;
            if (cc != null && cc.enabled)
            {
                cc.Move(worldDelta); // Move using internal physics logic
                if (rotatePassengers)
                {
                    t.rotation = deltaRot * t.rotation;
                }
                continue;
            }

            // Priority 2: Rigidbody
            var rb = useRigidbodyIfPresent ? t.GetComponent<Rigidbody>() : null;
            if (rb != null)
            {
                if (wakeRigidbodies) rb.WakeUp();
                rb.MovePosition(pPrime); // Direct physics move
                if (rotatePassengers)
                    rb.MoveRotation(deltaRot * rb.rotation);
            }
            else
            {
                // Priority 3: Transform
                t.position = pPrime;
                if (rotatePassengers)
                    t.rotation = deltaRot * t.rotation;
            }
        }

        // 3. Update cached pose for the next frame
        _prevOwnerPos = newPos;
        _prevOwnerRot = newRot;
    }
}
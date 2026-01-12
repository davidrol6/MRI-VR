/* * SCRIPT SUMMARY:
 * This script creates a functional physical button that moves another object (the "Target") when pressed.
 * It is commonly used for controlling heavy machinery, like moving a patient bed into an MRI scanner.
 * * * How it works:
 * - It supports two modes: 'Toggle' (move a set distance per press) and 'Hold' (move continuously while pressed).
 * - It animates the button itself (visual push effect).
 * - It calculates movement along a specific axis (World or Local).
 * - It handles "Passengers": If the VR player or other objects are standing on the moving target, this script ensures they move with it seamlessly.
 * - It integrates with the ObjectiveManager to mark tasks as complete when the button is used.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonMovesObject : MonoBehaviour
{
    public enum MovementSpace { World, Local }
    public enum PressMode { Toggle, Hold }

    [Header("Target to move")]
    public Transform target; // The heavy object being moved (e.g., the MRI Bed)

    [Header("Behavior")]
    public PressMode pressMode = PressMode.Hold;
    public MovementSpace space = MovementSpace.World;

    [Tooltip("Direction of movement for this button (used for sign and axis).")]
    public Vector3 direction = Vector3.right; // Defines which way the target moves

    [Tooltip("Step length along the axis per press (Toggle). Can be negative.")]
    public float distance = 0.25f; // Only used in Toggle mode

    [Header("Animation (Target)")]
    [Min(0.001f)] public float speed = 0.5f;        // Toggle: animation speed; Hold: m/s along axis
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Animation (Button visual push)")]
    public bool animateButtonSelf = true;
    public Transform buttonToTranslate; // The specific mesh part of the button that pushes in

    public enum Axis { X, Y, Z, Custom }
    public Axis buttonAxis = Axis.Z;    // Local axis the button mesh moves along
    public Vector3 customAxis = Vector3.forward;

    [Tooltip("Travel distance along the chosen axis (negative to invert).")]
    public float buttonTravel = -0.01f; // How deep the button goes when pressed

    [Min(0.001f)] public float buttonSpeed = 0.2f;
    public AnimationCurve buttonEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool buttonSnapExactlyAtEnd = true;

    [Header("Options")]
    public bool snapExactlyAtEnd = true;

    [Tooltip("If false, input is ignored while the target/button is animating.")]
    public bool allowInterrupt = false;

    [Header("Input Debounce")]
    [Min(0f)] public float minHoldBeforeRelease = 0.10f; // Minimum time a press registers to prevent flickering
    [Min(0f)] public float inputCooldown = 0.12f;

    [Header("Axis Limit")]
    [Tooltip("Clamp movement within [minAlong, maxAlong] meters from the shared origin.")]
    public bool useAxisLimit = true;
    public float minAlong = -1f; // Minimum movement limit
    public float maxAlong = 1f;  // Maximum movement limit

    [Tooltip("Shared origin for along-axis distance. If null, uses the target's initial position (shared).")]
    public Transform limitOrigin; // Reference point for calculating limits

    // PASSENGERS ----------
    [Header("Passengers (optional)")]
    public PassengerZone passengerZone; // Zone trigger to detect objects on top of the target
    public bool movePassengersOnSnap = true;

    // OBJECTIVE SYSTEM ----------
    [Header("Objective System (optional)")]
    public ObjectiveManager objectiveManager;

    [Tooltip("Exact description of the objective to complete when pressed.")]
    public string objectiveDescription;

    public bool completeObjectiveOnPress = true;
    public bool onlyOnce = false;           // If true, mark that objective only once
    private bool _objectiveCompleted;

    // VR Rig (moves only if inside the PassengerZone)
    [Header("VR Rig (conditional)")]
    public Transform rigRoot;               // e.g., "[BuildingBlock] Camera Rig" - The root of the player
    public Transform centerEyeAnchor;       // e.g., TrackingSpace/CenterEyeAnchor - The player's head
    public CharacterController rigCC;       // optional - CharacterController component on the rig

    // --- static shared state per target ---
    // This allows multiple buttons (e.g., Up/Down) to agree on the coordinate system of the same target
    private class SharedAxisState
    {
        public Vector3 origin;
        public bool originSet;
        public Vector3 axisCanonical;
        public bool axisSet;
    }

    private static readonly Dictionary<Transform, SharedAxisState> s_shared = new();

    // Coroutine references
    private Coroutine _moveRoutine;
    private Coroutine _holdRoutine;
    private Vector3 _btnRestLocalPos;
    private Vector3 _btnPressedLocalPos;
    private Coroutine _buttonRoutine;

    // Internal State
    private bool _isPressed;
    private float _pressStamp;
    private float _lastInputTime;

    private float _dirSignRelativeToCanonical = 1f;

    // Accessors for shared state
    private SharedAxisState Shared
    {
        get
        {
            if (!s_shared.TryGetValue(target, out var st))
            {
                st = new SharedAxisState();
                s_shared[target] = st;
            }
            return st;
        }
    }

    private Vector3 CanonicalAxis => Shared.axisCanonical;
    private Vector3 SharedOrigin => Shared.origin;
    private Collider _zoneCollider;

    private void Awake()
    {
        if (passengerZone != null)
            _zoneCollider = passengerZone.GetComponent<Collider>();

        if (target == null)
        {
            Debug.LogError("[ButtonMovesObject] Please assign a target.");
            enabled = false;
            return;
        }

        // 1. Initialize Shared Origin (per target)
        if (!Shared.originSet)
        {
            Shared.origin = (limitOrigin != null) ? limitOrigin.position : target.position;
            Shared.originSet = true;
        }
        else if (limitOrigin != null)
        {
            Shared.origin = limitOrigin.position;
        }

        // 2. Initialize Shared Movement Axis (per target)
        if (!Shared.axisSet)
        {
            Vector3 d = direction.sqrMagnitude < 1e-6f ? Vector3.right : direction.normalized;
            Vector3 axisWorld = (space == MovementSpace.Local) ? target.TransformDirection(d) : d;
            if (axisWorld.sqrMagnitude < 1e-6f) axisWorld = Vector3.right;
            Shared.axisCanonical = axisWorld.normalized;
            Shared.axisSet = true;
        }

        // 3. Determine sign: Is this button moving WITH or AGAINST the canonical axis?
        {
            Vector3 d = direction.sqrMagnitude < 1e-6f ? Vector3.right : direction.normalized;
            Vector3 axisWorld = (space == MovementSpace.Local) ? target.TransformDirection(d) : d;
            axisWorld = axisWorld.sqrMagnitude < 1e-6f ? Vector3.right : axisWorld.normalized;
            _dirSignRelativeToCanonical = Mathf.Sign(Vector3.Dot(axisWorld, CanonicalAxis));
            if (_dirSignRelativeToCanonical == 0f) _dirSignRelativeToCanonical = 1f;
        }

        // 4. Calculate visual positions for the button itself
        if (animateButtonSelf && buttonToTranslate != null)
        {
            _btnRestLocalPos = buttonToTranslate.localPosition;
            Vector3 axisDir = GetAxisDir();
            _btnPressedLocalPos = _btnRestLocalPos + axisDir * buttonTravel;
        }

        if (passengerZone != null && passengerZone.owner == null)
            passengerZone.owner = target;
    }

    // ----------- Events -----------
    // Called by VR Interaction Events (e.g., OnSelectEntered)

    public void OnPress()
    {
        if (_isPressed) return;
        if (Time.time - _lastInputTime < inputCooldown) return;

        _isPressed = true;
        _pressStamp = Time.time;
        _lastInputTime = Time.time;

        // Visual feedback
        if (animateButtonSelf)
            StartMoveButton(_btnPressedLocalPos);

        // Logic
        TryCompleteObjective();

        if (pressMode == PressMode.Hold)
        {
            StartHold();
            return;
        }

        // Toggle Mode: Move one discrete step
        StepOnceRelative(distance * _dirSignRelativeToCanonical);
    }

    public void OnRelease()
    {
        if (!_isPressed) return;
        if (Time.time - _pressStamp < minHoldBeforeRelease) return;
        if (Time.time - _lastInputTime < inputCooldown) return;

        _isPressed = false;
        _lastInputTime = Time.time;

        // Cleanup coroutines
        if (pressMode == PressMode.Hold)
        {
            if (_holdRoutine != null)
            {
                StopCoroutine(_holdRoutine);
                _holdRoutine = null;
            }
            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
                _moveRoutine = null;
            }
        }

        // Reset visual
        if (animateButtonSelf)
            StartMoveButton(_btnRestLocalPos);
    }

    // ----------- Objective Handling -----------

    private void TryCompleteObjective()
    {
        if (!completeObjectiveOnPress || objectiveManager == null) return;
        if (onlyOnce && _objectiveCompleted) return;

        _objectiveCompleted = true;

        Debug.Log($"Objective triggered from button: {objectiveDescription}");
        objectiveManager.CompleteObjective(objectiveDescription);
        objectiveManager.PrintObjectivesStatus();
    }

    // ----------- Movement Logic -----------

    // Calculates the next discrete position and starts movement
    private void StepOnceRelative(float stepSigned)
    {
        Vector3 current = target.position;
        // Project current position onto the canonical axis
        float curAlong = Vector3.Dot(current - SharedOrigin, CanonicalAxis);
        Vector3 ortho = (current - SharedOrigin) - CanonicalAxis * curAlong;

        float newAlong = curAlong + stepSigned;

        // Clamp to physical limits
        if (useAxisLimit)
            newAlong = Mathf.Clamp(newAlong, minAlong, maxAlong);

        if (Mathf.Approximately(newAlong, curAlong))
            return;

        Vector3 dest = SharedOrigin + ortho + CanonicalAxis * newAlong;
        StartMoveTarget(dest);
    }

    private void StartMoveTarget(Vector3 dest)
    {
        if (!allowInterrupt && _moveRoutine != null) return;
        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveTargetTo(dest));
    }

    // Coroutine for smooth movement (Toggle Mode)
    private IEnumerator MoveTargetTo(Vector3 dest)
    {
        Vector3 start = target.position;
        float totalDist = Vector3.Distance(start, dest);
        if (totalDist < 1e-4f)
        {
            _moveRoutine = null;
            yield break;
        }

        float duration = Mathf.Max(0.0001f, totalDist / speed);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = ease.Evaluate(Mathf.Clamp01(t));

            Vector3 current = target.position;
            Vector3 next = Vector3.LerpUnclamped(start, dest, k);

            Vector3 delta = next - current;
            
            // Move passengers (objects on bed)
            if (passengerZone != null && delta.sqrMagnitude > 1e-12f)
                passengerZone.ApplyDelta(delta);

            // Move VR Rig (Player)
            MoveRigByDeltaIfInside(delta);

            target.position = next;
            yield return null;
        }

        // Snap to exact end position
        if (snapExactlyAtEnd)
        {
            Vector3 final = dest;
            Vector3 delta = final - target.position;

            if (passengerZone != null && movePassengersOnSnap && delta.sqrMagnitude > 1e-12f)
                passengerZone.ApplyDelta(delta);

            MoveRigByDeltaIfInside(delta);

            target.position = final;
        }

        _moveRoutine = null;
    }

    private void StartHold()
    {
        if (!allowInterrupt && (_moveRoutine != null || _holdRoutine != null)) return;

        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }

        if (_holdRoutine != null)
            StopCoroutine(_holdRoutine);

        _holdRoutine = StartCoroutine(MoveTargetWhileHeld());
    }

    // Coroutine for continuous movement (Hold Mode)
    private IEnumerator MoveTargetWhileHeld()
    {
        float signedSpeed = speed * _dirSignRelativeToCanonical;

        while (_isPressed)
        {
            Vector3 current = target.position;
            float curAlong = Vector3.Dot(current - SharedOrigin, CanonicalAxis);
            Vector3 ortho = (current - SharedOrigin) - CanonicalAxis * curAlong;

            float desiredAlong = curAlong + signedSpeed * Time.deltaTime;

            if (useAxisLimit)
                desiredAlong = Mathf.Clamp(desiredAlong, minAlong, maxAlong);

            // Stop if we hit a limit
            if (Mathf.Approximately(desiredAlong, curAlong))
                break;

            Vector3 desired = SharedOrigin + ortho + CanonicalAxis * desiredAlong;

            Vector3 delta = desired - current;
            
            // Sync passengers and player
            if (passengerZone != null && delta.sqrMagnitude > 1e-12f)
                passengerZone.ApplyDelta(delta);

            MoveRigByDeltaIfInside(delta);

            target.position = desired;
            yield return null;
        }

        _holdRoutine = null;
    }

    // --- Button visual movement ---

    private void StartMoveButton(Vector3 destLocal)
    {
        if (buttonToTranslate == null) return;
        if (!allowInterrupt && _buttonRoutine != null) return;
        if (_buttonRoutine != null) StopCoroutine(_buttonRoutine);
        _buttonRoutine = StartCoroutine(MoveButtonTo(destLocal));
    }

    private IEnumerator MoveButtonTo(Vector3 destLocal)
    {
        Vector3 start = buttonToTranslate.localPosition;
        float totalDist = Vector3.Distance(start, destLocal);
        if (totalDist < 1e-5f)
        {
            _buttonRoutine = null;
            yield break;
        }

        float duration = Mathf.Max(0.0001f, totalDist / buttonSpeed);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = buttonEase.Evaluate(Mathf.Clamp01(t));
            buttonToTranslate.localPosition = Vector3.LerpUnclamped(start, destLocal, k);
            yield return null;
        }

        if (buttonSnapExactlyAtEnd)
            buttonToTranslate.localPosition = destLocal;

        _buttonRoutine = null;
    }

    // ----------- Utils -----------

    private Vector3 GetAxisDir()
    {
        switch (buttonAxis)
        {
            case Axis.X: return Vector3.right;
            case Axis.Y: return Vector3.up;
            case Axis.Z: return Vector3.forward;
            default:
                var v = customAxis;
                if (v.sqrMagnitude < 1e-6f) v = Vector3.forward;
                return v.normalized;
        }
    }

    // Checks if the player's head is inside the "Passenger Zone"
    private bool IsHeadInsideZone()
    {
        if (_zoneCollider == null || centerEyeAnchor == null) return false;

        // ClosestPoint logic: If the point is inside, the result is the point itself.
        Vector3 p = centerEyeAnchor.position;
        Vector3 cp = _zoneCollider.ClosestPoint(p);
        return (cp - p).sqrMagnitude <= 1e-8f;
    }

    // Manually moves the VR Player Rig
    private void MoveRigByDeltaIfInside(Vector3 delta)
    {
        if (delta.sqrMagnitude <= 1e-12f) return;
        if (rigRoot == null) return;
        if (!IsHeadInsideZone()) return; // Only move if player is actually "on" the bed

        // IMPORTANT: Temporarily disable CharacterController to allow manual transform movement
        // Otherwise, the CC tries to correct the position and fight the script.
        bool hadCC = false;
        if (rigCC != null)
        {
            hadCC = rigCC.enabled;
            rigCC.enabled = false;
        }

        rigRoot.position += delta;

        if (rigCC != null)
            rigCC.enabled = hadCC;
    }
}
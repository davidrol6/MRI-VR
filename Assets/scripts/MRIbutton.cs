using UnityEngine;
using System.Collections;

public class ButtonMovesObject : MonoBehaviour
{
    public enum MovementSpace { World, Local }
    public enum PressMode { Toggle, Hold } // kept for inspector compatibility

    [Header("Target to move")]
    public Transform target;

    [Header("Behavior")]
    public PressMode pressMode = PressMode.Hold;          // Toggle == Hold (identical behavior)
    public MovementSpace space = MovementSpace.World;

    [Tooltip("Direction of movement (normalized internally).")]
    public Vector3 direction = Vector3.right;

    [Tooltip("Distance along the chosen direction.")]
    public float distance = 0.25f;

    [Header("Animation (TARGET)")]
    [Min(0.001f)] public float speed = 0.5f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Animation (BUTTON SELF-PUSH)")]
    public bool animateButtonSelf = true;
    public Transform buttonToTranslate;
    public enum Axis { X, Y, Z, Custom }
    public Axis buttonAxis = Axis.Z;
    public Vector3 customAxis = Vector3.forward;
    [Tooltip("Travel distance along the chosen axis (negative to invert).")]
    public float buttonTravel = -0.01f;
    [Min(0.001f)] public float buttonSpeed = 0.2f;
    public AnimationCurve buttonEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool buttonSnapExactlyAtEnd = true;

    [Header("Options")]
    public bool snapExactlyAtEnd = true;

    [Tooltip("If false, input is ignored while the target/button is animating.")]
    public bool allowInterrupt = false;

    [Header("Input Debounce")]
    [Tooltip("Minimum time the press must be held before we accept a release (filters tiny unhovers).")]
    [Min(0f)] public float minHoldBeforeRelease = 0.10f;

    [Tooltip("Cooldown between accepted inputs (press or release).")]
    [Min(0f)] public float inputCooldown = 0.12f;

    [Header("Axis Limit")]
    [Tooltip("Enable to clamp movement along the motion axis within [minAlong, maxAlong] meters from the origin point.")]
    public bool useAxisLimit = true;

    [Tooltip("Minimum allowed position along the axis (meters from origin).")]
    public float minAlong = 0f;

    [Tooltip("Maximum allowed position along the axis (meters from origin).")]
    public float maxAlong = 1f;

    [Tooltip("Origin reference for the axis distance. If null, uses the target's initial position.")]
    public Transform limitOrigin;

    // PASSENGERS ----------
    [Header("Passengers (optional)")]
    public PassengerZone passengerZone;               // assign the trigger inside your box
    public bool movePassengersOnSnap = true;          // also apply delta on final snap

    // --- internal state ---
    private Coroutine _moveRoutine;       // target animation
    private Vector3 _btnRestLocalPos;
    private Vector3 _btnPressedLocalPos;
    private Coroutine _buttonRoutine;

    private Vector3 _offset;              // world-space movement offset per press
    private bool _isPressed;              // true while we consider the button held
    private float _pressStamp;            // Time.time when we accepted the press
    private float _lastInputTime;         // Time.time of the last accepted press/release

    // axis-limit helpers
    private Vector3 _axisWorld;           // normalized world-space motion axis
    private Vector3 _originPoint;         // world-space origin for along-axis measurement

    void Awake()
    {
        if (target == null)
        {
            Debug.LogError("[ButtonMovesObject] Please assign a target.");
            enabled = false;
            return;
        }

        // Build motion axis and offset in world space
        Vector3 dir = direction.sqrMagnitude < 1e-6f ? Vector3.right : direction.normalized;
        _axisWorld = (space == MovementSpace.Local) ? target.TransformDirection(dir).normalized : dir;
        _offset = _axisWorld * distance; // one step along the axis

        // Origin for axis-limits: explicit or initial target position
        _originPoint = (limitOrigin != null) ? limitOrigin.position : target.position;

        // Button endpoints
        if (animateButtonSelf && buttonToTranslate != null)
        {
            _btnRestLocalPos = buttonToTranslate.localPosition;
            Vector3 axisDir = GetAxisDir();
            _btnPressedLocalPos = _btnRestLocalPos + axisDir * buttonTravel;
        }

        // PASSENGERS: optional auto-wire owner for safety
        if (passengerZone != null && passengerZone.owner == null)
            passengerZone.owner = target;
    }

    // ----------- Events -----------
    public void OnPress()
    {
        // Cooldown & already pressed guards
        if (_isPressed) return;
        if (Time.time - _lastInputTime < inputCooldown) return;

        _isPressed = true;
        _pressStamp = Time.time;
        _lastInputTime = Time.time;

        // Intended destination
        Vector3 dest = target.position + _offset;

        // Clamp along the axis if enabled
        if (useAxisLimit)
        {
            dest = ClampDestAlongAxis(target.position, dest);
            // If clamping results in effectively no movement, skip target move
            if ((dest - target.position).sqrMagnitude < 1e-9f)
            {
                if (animateButtonSelf) StartMoveButton(_btnPressedLocalPos);
                return;
            }
        }

        StartMoveTarget(dest);

        // Button visual push-in
        if (animateButtonSelf) StartMoveButton(_btnPressedLocalPos);
    }

    public void OnRelease()
    {
        if (!_isPressed) return;
        if (Time.time - _pressStamp < minHoldBeforeRelease) return;
        if (Time.time - _lastInputTime < inputCooldown) return;

        _isPressed = false;
        _lastInputTime = Time.time;

        // NO target movement on release — only return the button visually
        if (animateButtonSelf) StartMoveButton(_btnRestLocalPos);
    }

    // ----------- Target movement -----------
    private void StartMoveTarget(Vector3 dest)
    {
        if (!allowInterrupt && _moveRoutine != null) return;
        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveTargetTo(dest));
    }

    private IEnumerator MoveTargetTo(Vector3 dest)
    {
        Vector3 start = target.position;
        float totalDist = Vector3.Distance(start, dest);
        if (totalDist < 1e-4f) { _moveRoutine = null; yield break; }

        float duration = Mathf.Max(0.0001f, totalDist / speed);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = ease.Evaluate(Mathf.Clamp01(t));

            // Compute next position (already clamped by dest)
            Vector3 current = target.position;
            Vector3 next = Vector3.LerpUnclamped(start, dest, k);

            // PASSENGERS: move riders by the same delta
            Vector3 delta = next - current;
            if (passengerZone != null && delta.sqrMagnitude > 1e-12f)
                passengerZone.ApplyDelta(delta);

            target.position = next;
            yield return null;
        }

        if (snapExactlyAtEnd)
        {
            Vector3 final = dest;
            Vector3 delta = final - target.position;

            if (passengerZone != null && movePassengersOnSnap && delta.sqrMagnitude > 1e-12f)
                passengerZone.ApplyDelta(delta);

            target.position = final;
        }

        _moveRoutine = null;
    }

    // ----------- Axis-limit helper -----------
    private Vector3 ClampDestAlongAxis(Vector3 current, Vector3 desired)
    {
        // Vector desde el origen a la posición actual
        Vector3 rCur = current - _originPoint;

        // Descomposición actual en (ortogonal + a lo largo del eje)
        float curAlong = Vector3.Dot(rCur, _axisWorld);
        Vector3 ortho = rCur - _axisWorld * curAlong;

        // Escalar deseado a lo largo del eje y clamp al rango
        float desAlong = Vector3.Dot(desired - _originPoint, _axisWorld);
        float clampedAlong = Mathf.Clamp(desAlong, minAlong, maxAlong);

        // Si no hay cambio efectivo a lo largo del eje, no muevas
        if (Mathf.Approximately(clampedAlong, curAlong))
            return current;

        // Reconstruye destino = origen + (ortogonal conservado) + (eje clamped)
        return _originPoint + ortho + _axisWorld * clampedAlong;
    }
// --- BUTTON movement ---
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
        if (totalDist < 1e-5f) { _buttonRoutine = null; yield break; }

        float duration = Mathf.Max(0.0001f, totalDist / buttonSpeed);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = buttonEase.Evaluate(Mathf.Clamp01(t));
            buttonToTranslate.localPosition = Vector3.LerpUnclamped(start, destLocal, k);
            yield return null;
        }

        if (buttonSnapExactlyAtEnd) buttonToTranslate.localPosition = destLocal;
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
}

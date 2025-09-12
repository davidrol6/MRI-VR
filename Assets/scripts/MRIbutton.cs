using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonMovesObject : MonoBehaviour
{
    public enum MovementSpace { World, Local }
    public enum PressMode { Toggle, Hold } // Toggle: paso discreto; Hold: continuo

    [Header("Target to move")]
    public Transform target;

    [Header("Behavior")]
    public PressMode pressMode = PressMode.Hold;
    public MovementSpace space = MovementSpace.World;

    [Tooltip("Direction of movement for THIS button (se usa solo para el signo en el eje canónico).")]
    public Vector3 direction = Vector3.right;

    [Tooltip("Step length along the axis per press (Toggle). Can be negative.")]
    public float distance = 0.25f;

    [Header("Animation (TARGET)")]
    [Min(0.001f)] public float speed = 0.5f; // Toggle: velocidad de animación; Hold: m/s
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
    [Min(0f)] public float minHoldBeforeRelease = 0.10f;
    [Min(0f)] public float inputCooldown = 0.12f;

    [Header("Axis Limit")]
    [Tooltip("Clamp movement within [minAlong, maxAlong] meters from the shared origin.")]
    public bool useAxisLimit = true;
    public float minAlong = -1f; // IMPORTANTE: permite negativo si quieres bajar respecto al origen
    public float maxAlong =  1f;

    [Tooltip("Shared origin for along-axis distance. If null, uses the target's initial position (shared).")]
    public Transform limitOrigin;

    // PASSENGERS ----------
    [Header("Passengers (optional)")]
    public PassengerZone passengerZone;
    public bool movePassengersOnSnap = true;

    // --- static shared state por TARGET ---
    private class SharedAxisState
    {
        public Vector3 origin;
        public bool originSet;
        public Vector3 axisCanonical; // eje canónico (mundo, unitario, con signo fijo)
        public bool axisSet;
    }

    private static readonly Dictionary<Transform, SharedAxisState> s_shared = new();

    // --- per-instance ---
    private Coroutine _moveRoutine;       // animación discreta (Toggle)
    private Coroutine _holdRoutine;       // movimiento continuo (Hold)
    private Vector3 _btnRestLocalPos;
    private Vector3 _btnPressedLocalPos;
    private Coroutine _buttonRoutine;

    private bool _isPressed;
    private float _pressStamp;
    private float _lastInputTime;

    // Cache por instancia
    private float _dirSignRelativeToCanonical = 1f; // +1 / -1 según este botón respecto al eje canónico

    // Helpers de acceso compartido
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

    void Awake()
    {
        if (target == null)
        {
            Debug.LogError("[ButtonMovesObject] Please assign a target.");
            enabled = false;
            return;
        }

        // 1) ORIGEN COMPARTIDO
        if (!Shared.originSet)
        {
            Shared.origin = (limitOrigin != null) ? limitOrigin.position : target.position;
            Shared.originSet = true;
        }
        else if (limitOrigin != null)
        {
            // Si algún botón define explícitamente un origin, imponlo como compartido
            Shared.origin = limitOrigin.position;
        }

        // 2) EJE CANÓNICO COMPARTIDO
        if (!Shared.axisSet)
        {
            Vector3 d = direction.sqrMagnitude < 1e-6f ? Vector3.right : direction.normalized;
            Vector3 axisWorld = (space == MovementSpace.Local) ? target.TransformDirection(d) : d;
            if (axisWorld.sqrMagnitude < 1e-6f) axisWorld = Vector3.right;
            Shared.axisCanonical = axisWorld.normalized; // primer botón fija el signo
            Shared.axisSet = true;
        }

        // 3) Signo relativo de ESTE botón respecto al eje canónico
        {
            Vector3 d = direction.sqrMagnitude < 1e-6f ? Vector3.right : direction.normalized;
            Vector3 axisWorld = (space == MovementSpace.Local) ? target.TransformDirection(d) : d;
            axisWorld = axisWorld.sqrMagnitude < 1e-6f ? Vector3.right : axisWorld.normalized;

            // +1 si “apunta” aproximadamente en el mismo sentido que el canónico; -1 si es opuesto
            _dirSignRelativeToCanonical = Mathf.Sign(Vector3.Dot(axisWorld, CanonicalAxis));
            if (_dirSignRelativeToCanonical == 0f) _dirSignRelativeToCanonical = 1f; // caso degenerado
        }

        // Button endpoints (visual)
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
    public void OnPress()
    {
        if (_isPressed) return;
        if (Time.time - _lastInputTime < inputCooldown) return;

        _isPressed = true;
        _pressStamp = Time.time;
        _lastInputTime = Time.time;

        if (animateButtonSelf) StartMoveButton(_btnPressedLocalPos);

        if (pressMode == PressMode.Hold)
        {
            StartHold();
            return;
        }

        // --- TOGGLE: paso RELATIVO respecto a la posición actual ---
        StepOnceRelative(distance * _dirSignRelativeToCanonical);
    }

    public void OnRelease()
    {
        if (!_isPressed) return;
        if (Time.time - _pressStamp < minHoldBeforeRelease) return;
        if (Time.time - _lastInputTime < inputCooldown) return;

        _isPressed = false;
        _lastInputTime = Time.time;

        if (pressMode == PressMode.Hold)
        {
            if (_holdRoutine != null) { StopCoroutine(_holdRoutine); _holdRoutine = null; }
            if (_moveRoutine != null) { StopCoroutine(_moveRoutine); _moveRoutine = null; }
        }

        if (animateButtonSelf) StartMoveButton(_btnRestLocalPos);
    }

    // ----------- TOGGLE: paso relativo -----------
    private void StepOnceRelative(float stepSigned)
    {
        Vector3 current = target.position;

        float curAlong = Vector3.Dot(current - SharedOrigin, CanonicalAxis);
        Vector3 ortho = (current - SharedOrigin) - CanonicalAxis * curAlong;

        float newAlong = curAlong + stepSigned;

        if (useAxisLimit)
            newAlong = Mathf.Clamp(newAlong, minAlong, maxAlong);

        if (Mathf.Approximately(newAlong, curAlong))
            return;

        Vector3 dest = SharedOrigin + ortho + CanonicalAxis * newAlong;
        StartMoveTarget(dest);
    }

    // ----------- Target movement (animación discreta) -----------
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

            Vector3 current = target.position;
            Vector3 next = Vector3.LerpUnclamped(start, dest, k);

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

    // ----------- HOLD: movimiento continuo con eje canónico compartido -----------
    private void StartHold()
    {
        if (!allowInterrupt && (_moveRoutine != null || _holdRoutine != null)) return;

        if (_moveRoutine != null) { StopCoroutine(_moveRoutine); _moveRoutine = null; }
        if (_holdRoutine != null) StopCoroutine(_holdRoutine);
        _holdRoutine = StartCoroutine(MoveTargetWhileHeld());
    }

    private IEnumerator MoveTargetWhileHeld()
    {
        // Avanza a lo largo del eje canónico; este botón aporta el signo
        float signedSpeed = speed * _dirSignRelativeToCanonical;

        while (_isPressed)
        {
            Vector3 current = target.position;

            float curAlong = Vector3.Dot(current - SharedOrigin, CanonicalAxis);
            Vector3 ortho = (current - SharedOrigin) - CanonicalAxis * curAlong;

            float desiredAlong = curAlong + signedSpeed * Time.deltaTime;

            if (useAxisLimit)
                desiredAlong = Mathf.Clamp(desiredAlong, minAlong, maxAlong);

            if (Mathf.Approximately(desiredAlong, curAlong))
                break;

            Vector3 desired = SharedOrigin + ortho + CanonicalAxis * desiredAlong;

            Vector3 delta = desired - current;
            if (passengerZone != null && delta.sqrMagnitude > 1e-12f)
                passengerZone.ApplyDelta(delta);

            target.position = desired;
            yield return null;
        }

        _holdRoutine = null;
    }

    // --- BUTTON movement (visual) ---
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

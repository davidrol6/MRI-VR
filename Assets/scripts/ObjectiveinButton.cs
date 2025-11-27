using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class PhysicalPokeObjectiveTrigger : MonoBehaviour
{
    public enum PressMode { Toggle, Hold }

    [Header("Objective Logic")]
    public ObjectiveManager objectiveManager;
    public string objectiveDescription;

    [Header("Button Behavior")]
    public PressMode pressMode = PressMode.Toggle;
    public float minHoldBeforeRelease = 0.1f;
    public float inputCooldown = 0.15f;

    [Header("Visual Feedback (optional)")]
    public bool animateButton = true;
    public Transform buttonVisual;
    public Vector3 pressOffset = new Vector3(0, -0.01f, 0);
    [Min(0.001f)] public float buttonSpeed = 0.2f;
    public AnimationCurve buttonEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool snapAtEnd = true;

    private bool _isPressed;
    private float _pressStamp;
    private float _lastInputTime;
    private Vector3 _btnRestLocalPos;
    private Vector3 _btnPressedLocalPos;
    private Coroutine _buttonRoutine;

    private void Awake()
    {
        // Use trigger collider to detect finger/contact without applying forces
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        if (buttonVisual != null)
        {
            _btnRestLocalPos = buttonVisual.localPosition;
            _btnPressedLocalPos = _btnRestLocalPos + pressOffset;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Optionally filter by tag (e.g. "Finger" or "Hand") if needed
        OnPress();
    }

    private void OnTriggerExit(Collider other)
    {
        OnRelease();
    }

    // ---------------- Press / Release Logic ----------------

    public void OnPress()
    {
        if (_isPressed) return;
        if (Time.time - _lastInputTime < inputCooldown) return;

        _isPressed = true;
        _pressStamp = Time.time;
        _lastInputTime = Time.time;

        if (animateButton && buttonVisual != null)
            StartMoveButton(_btnPressedLocalPos);

        Debug.Log($"Physical poke detected on {gameObject.name}");

        if (pressMode == PressMode.Toggle)
        {
            TriggerObjective();
        }
        else if (pressMode == PressMode.Hold)
        {
            StartCoroutine(HoldRoutine());
        }
    }

    public void OnRelease()
    {
        if (!_isPressed) return;
        if (Time.time - _pressStamp < minHoldBeforeRelease) return;

        _isPressed = false;
        _lastInputTime = Time.time;

        if (animateButton && buttonVisual != null)
            StartMoveButton(_btnRestLocalPos);

        if (pressMode == PressMode.Hold)
        {
            StopAllCoroutines();
        }
    }

    // ---------------- Objective Action ----------------

    private void TriggerObjective()
    {
        if (objectiveManager != null)
        {
            Debug.Log($"Objective triggered: {objectiveDescription}");
            objectiveManager.CompleteObjective(objectiveDescription);
            objectiveManager.PrintObjectivesStatus();
        }
        else
        {
            Debug.LogWarning("[PhysicalPokeObjectiveTrigger] No ObjectiveManager assigned.");
        }
    }

    private IEnumerator HoldRoutine()
    {
        while (_isPressed)
        {
            TriggerObjective();
            yield return new WaitForSeconds(0.5f); // repeat while held
        }
    }

    // ---------------- Button Animation ----------------

    private void StartMoveButton(Vector3 dest)
    {
        if (_buttonRoutine != null) StopCoroutine(_buttonRoutine);
        _buttonRoutine = StartCoroutine(MoveButton(dest));
    }

    private IEnumerator MoveButton(Vector3 dest)
    {
        Vector3 start = buttonVisual.localPosition;
        float totalDist = Vector3.Distance(start, dest);
        float duration = Mathf.Max(0.0001f, totalDist / buttonSpeed);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = buttonEase.Evaluate(Mathf.Clamp01(t));
            buttonVisual.localPosition = Vector3.Lerp(start, dest, k);
            yield return null;
        }

        if (snapAtEnd)
            buttonVisual.localPosition = dest;
    }
}

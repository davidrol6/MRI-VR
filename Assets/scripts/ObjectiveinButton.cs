/* * SCRIPT SUMMARY:
 * This script turns a GameObject into a physical button that reacts to direct contact (like a VR finger poke).
 * * * How it works:
 * - It requires a Collider marked as "IsTrigger" to detect when another object enters its volume.
 * - When poked (OnTriggerEnter), it visually depresses (animates) and triggers a specific game Objective.
 * - It includes "debounce" logic (cooldowns) to prevent accidental double-presses or jittery inputs common in VR physics.
 */

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class PhysicalPokeObjectiveTrigger : MonoBehaviour
{
    public enum PressMode { Toggle, Hold }

    [Header("Objective Logic")]
    public ObjectiveManager objectiveManager; // Reference to the core game logic
    public string objectiveDescription;       // The specific task ID to complete

    [Header("Button Behavior")]
    public PressMode pressMode = PressMode.Toggle;
    public float minHoldBeforeRelease = 0.1f; // Minimum time the button must be held to register a release
    public float inputCooldown = 0.15f;       // Time in seconds to ignore new inputs after a press (Debouncing)

    [Header("Visual Feedback (optional)")]
    public bool animateButton = true;
    public Transform buttonVisual;            // The mesh that visually moves
    public Vector3 pressOffset = new Vector3(0, -0.01f, 0); // Direction and distance of the push
    [Min(0.001f)] public float buttonSpeed = 0.2f;
    public AnimationCurve buttonEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool snapAtEnd = true;

    // Internal state tracking
    private bool _isPressed;
    private float _pressStamp;
    private float _lastInputTime;
    private Vector3 _btnRestLocalPos;
    private Vector3 _btnPressedLocalPos;
    private Coroutine _buttonRoutine;

    private void Awake()
    {
        // specific logic to ensure the collider is a Trigger (detects entry without physical resistance)
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        // Calculate visual positions relative to the parent
        if (buttonVisual != null)
        {
            _btnRestLocalPos = buttonVisual.localPosition;
            _btnPressedLocalPos = _btnRestLocalPos + pressOffset;
        }
    }

    // Unity Physics Event: Called when a collider (e.g., player finger) touches the button
    private void OnTriggerEnter(Collider other)
    {
        // Note: You can add a tag check here (e.g., if (other.CompareTag("Hand"))) to filter inputs
        OnPress();
    }

    // Unity Physics Event: Called when the collider stops touching the button
    private void OnTriggerExit(Collider other)
    {
        OnRelease();
    }

    // ---------------- Press / Release Logic ----------------

    public void OnPress()
    {
        // Ignore input if already pressed or if we are inside the cooldown window
        if (_isPressed) return;
        if (Time.time - _lastInputTime < inputCooldown) return;

        _isPressed = true;
        _pressStamp = Time.time;
        _lastInputTime = Time.time;

        // Visual feedback: Push the button in
        if (animateButton && buttonVisual != null)
            StartMoveButton(_btnPressedLocalPos);

        Debug.Log($"Physical poke detected on {gameObject.name}");

        // Game Logic
        if (pressMode == PressMode.Toggle)
        {
            // Trigger once per press
            TriggerObjective();
        }
        else if (pressMode == PressMode.Hold)
        {
            // Trigger repeatedly while held
            StartCoroutine(HoldRoutine());
        }
    }

    public void OnRelease()
    {
        if (!_isPressed) return;
        
        // Prevent "flicker" releases if the physical interaction was too short
        if (Time.time - _pressStamp < minHoldBeforeRelease) return;

        _isPressed = false;
        _lastInputTime = Time.time;

        // Visual feedback: Return button to rest position
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

    // Coroutine for Hold mode: triggers the objective repeatedly (e.g., every 0.5s)
    private IEnumerator HoldRoutine()
    {
        while (_isPressed)
        {
            TriggerObjective();
            yield return new WaitForSeconds(0.5f); 
        }
    }

    // ---------------- Button Animation ----------------

    private void StartMoveButton(Vector3 dest)
    {
        if (_buttonRoutine != null) StopCoroutine(_buttonRoutine);
        _buttonRoutine = StartCoroutine(MoveButton(dest));
    }

    // Smoothly interpolates the visual mesh position
    private IEnumerator MoveButton(Vector3 dest)
    {
        Vector3 start = buttonVisual.localPosition;
        float totalDist = Vector3.Distance(start, dest);
        
        // Avoid division by zero
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
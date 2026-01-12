/* * SCRIPT SUMMARY:
 * This script controls a "World Space" UI Canvas that appears when the player enters a specific trigger zone.
 * * * How it works:
 * - It uses a Trigger Collider to detect the player's presence.
 * - When the player enters, the Canvas fades in (using a CanvasGroup).
 * - In 'LateUpdate', the Canvas smoothly floats in front of the camera (Billboarding behavior), ensuring it is always readable.
 * - It supports "Yaw Only" rotation, meaning the UI stays upright even if the player looks up or down (preventing awkward tilting).
 * - When the player leaves the zone, the Canvas fades out automatically.
 */

using System.Collections;
using UnityEngine;

public class UIAreaFollowCanvas : MonoBehaviour
{
    [Header("References")]
    public RectTransform canvasRoot;    // The actual UI element to move (World Space Canvas)
    public Transform followTarget;      // usually the Camera; if null, auto-finds Camera.main
    public CanvasGroup canvasGroup;     // Used to control opacity for fade in/out effects

    [Header("Placement")]
    [Tooltip("Distance in front of the camera where the Canvas will appear (in meters).")]
    public float forwardDistance = 1.2f;
    
    [Tooltip("Vertical offset relative to the camera (in meters).")]
    public float verticalOffset = 0.0f; // Positive = higher, Negative = lower

    [Header("Smoothing")]
    [Tooltip("Position smoothing (higher = faster catch-up).")]
    public float positionLerpSpeed = 10f;
    
    [Tooltip("Rotation smoothing (higher = faster catch-up).")]
    public float rotationLerpSpeed = 12f;

    [Header("Facing")]
    [Tooltip("If true, the Canvas only rotates around Y (ignores head pitch/roll). Keeps text upright.")]
    public bool yawOnly = true;

    [Header("Fade")]
    public float fadeInTime = 0.15f;
    public float fadeOutTime = 0.12f;

    [Header("Debug")]
    public bool startHidden = true;

    // Internal state
    private Quaternion baseRotationOffset;
    private Collider _area;
    private bool _playerInside = false;
    private bool _isVisible = false;

    private void Reset()
    {
        // specific logic to ensure the collider on this object acts as a sensor (Trigger)
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void Awake()
    {
        _area = GetComponent<Collider>();
        if (!_area)
            Debug.LogWarning("[UIAreaFollowCanvas] This object needs a Collider set as Trigger.");

        // Auto-fetch canvas group if not assigned
        if (!canvasGroup && canvasRoot)
            canvasGroup = canvasRoot.GetComponent<CanvasGroup>();

        if (startHidden)
            SetVisibleImmediate(false);

        // Store initial rotation if needed (though usually overridden by billboard logic)
        if (canvasRoot)
            baseRotationOffset = canvasRoot.rotation;
    }

    private void OnEnable()
    {
        // Safety check: Find the camera if the target wasn't assigned manually
        if (!followTarget)
        {
            var cam = Camera.main;
            if (cam) followTarget = cam.transform;
        }
    }

    // LateUpdate is used for camera following to prevent jitter (happens after Camera movement calculations)
    private void LateUpdate()
    {
        if (!_isVisible || !canvasRoot || !followTarget) return;

        // 1. Calculate Target Position
        // Math: Camera Position + (Forward Vector * Distance) + (Up Vector * Offset)
        Vector3 desiredPos =
            followTarget.position +
            followTarget.forward * forwardDistance +
            Vector3.up * verticalOffset;

        // Smoothly interpolate current position to desired position
        canvasRoot.position = Vector3.Lerp(
            canvasRoot.position,
            desiredPos,
            Time.deltaTime * positionLerpSpeed
        );

        // 2. Calculate Target Rotation (Billboarding)
        Quaternion targetRot;
        if (yawOnly)
        {
            // Flatten the direction vector on the Y axis
            Vector3 toCam = canvasRoot.position - followTarget.position;
            toCam.y = 0f;
            
            if (toCam.sqrMagnitude < 0.0001f) return; // Prevent divide by zero
            targetRot = Quaternion.LookRotation(toCam);
        }
        else
        {
            // Look directly towards the camera (including pitch/roll)
            targetRot = Quaternion.LookRotation(canvasRoot.position - followTarget.position);
        }

        // Smoothly interpolate rotation
        canvasRoot.rotation = Quaternion.Slerp(
            canvasRoot.rotation,
            targetRot,
            Time.deltaTime * rotationLerpSpeed
        );
    }

    // --- Trigger detection ---
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering is the player/camera
        if (IsPlayerRoot(other.transform))
        {
            _playerInside = true;
            Show();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayerRoot(other.transform))
        {
            _playerInside = false;
            Hide();
        }
    }

    // Flexible check to detect the player object or VR camera rig
    private bool IsPlayerRoot(Transform t)
    {
        if (t.CompareTag("Player") || t.CompareTag("MainCamera")) return true;
        if (t.GetComponentInChildren<Camera>() != null) return true;
        if (t.GetComponentInParent<Camera>() != null) return true;
        return false;
    }

    // --- Visibility & Fade Logic ---

    public void Show()
    {
        if (_isVisible) return;
        _isVisible = true;
        StopAllCoroutines();
        StartCoroutine(FadeTo(1f, fadeInTime));
    }

    public void Hide()
    {
        if (!_isVisible) return;
        _isVisible = false;
        StopAllCoroutines();
        StartCoroutine(FadeTo(0f, fadeOutTime));
    }

    // Instantly sets visibility without animation (useful for initialization)
    private void SetVisibleImmediate(bool visible)
    {
        _isVisible = visible;

        if (canvasGroup)
            canvasGroup.alpha = visible ? 1f : 0f;

        if (canvasRoot)
            canvasRoot.gameObject.SetActive(true); // Keep active so LateUpdate runs logic if needed
    }

    // Coroutine to smoothly change alpha over time
    private IEnumerator FadeTo(float target, float time)
    {
        if (!canvasGroup)
        {
            // Fallback: If no CanvasGroup, just toggle active state abruptly
            if (canvasRoot)
                canvasRoot.gameObject.SetActive(target > 0.5f);
            yield break;
        }

        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / time);
            yield return null;
        }

        canvasGroup.alpha = target;
    }
}
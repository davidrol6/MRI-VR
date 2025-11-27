using System.Collections;
using UnityEngine;

public class UIAreaFollowCanvas : MonoBehaviour
{
    [Header("References")]
    public RectTransform canvasRoot;    // RectTransform of the Canvas (World Space)
    public Transform followTarget;      // Usually the camera; if null, uses Camera.main
    public CanvasGroup canvasGroup;     // For fade in/out (CanvasGroup on the Canvas root)

    [Header("Placement")]
    [Tooltip("Distance in front of the camera where the Canvas will appear (in meters).")]
    public float forwardDistance = 1.2f;
    [Tooltip("Vertical offset relative to the camera (in meters).")]
    public float verticalOffset = 0.0f;

    [Header("Smoothing")]
    [Tooltip("Position smoothing (higher = faster).")]
    public float positionLerpSpeed = 10f;
    [Tooltip("Rotation smoothing (higher = faster).")]
    public float rotationLerpSpeed = 12f;

    [Header("Facing")]
    [Tooltip("If true, the Canvas only rotates around Y (ignores head pitch/roll).")]
    public bool yawOnly = true;

    [Header("Fade")]
    public float fadeInTime = 0.15f;
    public float fadeOutTime = 0.12f;

    [Header("Debug")]
    public bool startHidden = true;

    private Quaternion baseRotationOffset;

    private Collider _area;
    private bool _playerInside = false;
    private bool _isVisible = false;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void Awake()
    {
        _area = GetComponent<Collider>();
        if (!_area)
            Debug.LogWarning("[UIAreaFollowCanvas] This object needs a Collider set as Trigger.");

        if (!canvasGroup && canvasRoot)
            canvasGroup = canvasRoot.GetComponent<CanvasGroup>();

        if (startHidden)
            SetVisibleImmediate(false);

        baseRotationOffset = canvasRoot.rotation;
    }

    private void OnEnable()
    {
        if (!followTarget)
        {
            // Try to find the main camera (VR or desktop)
            var cam = Camera.main;
            if (cam) followTarget = cam.transform;
        }
    }

    private void LateUpdate()
    {
        if (!_isVisible || !canvasRoot || !followTarget) return;

        // Target position: in front of the camera + vertical offset
        Vector3 desiredPos =
            followTarget.position +
            followTarget.forward * forwardDistance +
            Vector3.up * verticalOffset;

        canvasRoot.position = Vector3.Lerp(
            canvasRoot.position,
            desiredPos,
            Time.deltaTime * positionLerpSpeed
        );

        // Rotate to face the camera (billboard-style)
        Quaternion targetRot;
        if (yawOnly)
        {
            Vector3 toCam = canvasRoot.position - followTarget.position;
            toCam.y = 0f;
            if (toCam.sqrMagnitude < 0.0001f) return;
            targetRot = Quaternion.LookRotation(toCam);
        }
        else
        {
            // Look directly towards the camera
            targetRot = Quaternion.LookRotation(canvasRoot.position - followTarget.position);
        }

        canvasRoot.rotation = Quaternion.Slerp(
            canvasRoot.rotation,
            targetRot,
            Time.deltaTime * rotationLerpSpeed
        );
    }

    // --- Trigger detection ---
    private void OnTriggerEnter(Collider other)
    {
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

    private bool IsPlayerRoot(Transform t)
    {
        // Accepts Player or MainCamera tags or anything that has a Camera in children/parents
        if (t.CompareTag("Player") || t.CompareTag("MainCamera")) return true;
        if (t.GetComponentInChildren<Camera>() != null) return true;
        if (t.GetComponentInParent<Camera>() != null) return true;
        return false;
    }

    // --- Visibility & Fade ---
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

    private void SetVisibleImmediate(bool visible)
    {
        _isVisible = visible;

        if (canvasGroup)
            canvasGroup.alpha = visible ? 1f : 0f;

        if (canvasRoot)
            canvasRoot.gameObject.SetActive(true); // keep active so position/rotation can update
    }

    private IEnumerator FadeTo(float target, float time)
    {
        if (!canvasGroup)
        {
            // If there is no CanvasGroup, just toggle active state
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

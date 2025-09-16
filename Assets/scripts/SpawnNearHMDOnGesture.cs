using UnityEngine;
using Oculus.Interaction.Input;

public class SpawnNearHMDOnGesture : MonoBehaviour
{
    [Header("What to show")]
    public GameObject targetObject;

    [Header("Where to spawn")]
    public Transform spawnPoint;            // Empty under CenterEyeAnchor
    public bool followWhileVisible = true;

    [Header("HMD & Hand")]
    public Transform hmdTransform;          // Drag CenterEyeAnchor (or any XR camera) here
    public HandRef leftHand;                // Or right hand if preferred

    [Header("Placement")]
    public Vector3 extraLocalOffset = Vector3.zero;
    [Range(0f, 20f)] public float followLerp = 12f;
    public bool faceCamera = true;

    private bool _isActive;
    private bool _pinchLastFrame;

    void Awake()
    {
        if (targetObject != null) targetObject.SetActive(false);
        if (hmdTransform == null && Camera.main != null)
            hmdTransform = Camera.main.transform; // safety fallback
    }

    void Update()
    {
        bool isPinching = leftHand != null && leftHand.GetIndexFingerIsPinching();
        if (isPinching && !_pinchLastFrame)
        {
            Toggle();
        }
        _pinchLastFrame = isPinching;

        if (_isActive && followWhileVisible && targetObject != null && spawnPoint != null)
        {
            PoseToSpawnPoint(targetObject.transform, smooth: true);
        }
    }

    private void Toggle()
    {
        if (targetObject == null || spawnPoint == null) return;

        _isActive = !_isActive;
        targetObject.SetActive(_isActive);

        if (_isActive)
        {
            PoseToSpawnPoint(targetObject.transform, smooth: false);
        }
    }

    private void PoseToSpawnPoint(Transform t, bool smooth)
    {
        Vector3 desiredPos = spawnPoint.TransformPoint(extraLocalOffset);

        Quaternion desiredRot = spawnPoint.rotation;
        if (faceCamera && hmdTransform != null)
        {
            Vector3 toCam = hmdTransform.position - desiredPos;
            toCam.y = 0f;
            if (toCam.sqrMagnitude > 0.0001f)
                desiredRot = Quaternion.LookRotation(toCam.normalized, Vector3.up);
        }

        if (smooth)
        {
            t.position = Vector3.Lerp(t.position, desiredPos, Time.deltaTime * followLerp);
            t.rotation = Quaternion.Slerp(t.rotation, desiredRot, Time.deltaTime * followLerp);
        }
        else
        {
            t.position = desiredPos;
            t.rotation = desiredRot;
        }
    }

    public void Show() { if (!targetObject.activeSelf) Toggle(); }
    public void Hide() { if (targetObject.activeSelf) Toggle(); }
}

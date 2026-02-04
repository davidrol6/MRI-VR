/* * SCRIPT SUMMARY:
 * This script manages a "Head-Locked" or "Face-Floating" UI element (like a menu or inventory) 
 * that appears and disappears based on a hand gesture.
 * * * How it works:
 * - It tracks the user's hand (via OVRHand or Interaction SDK HandRef).
 * - When an "Index Pinch" gesture is detected, it toggles the 'targetObject' active/inactive.
 * - When active, the object smoothly floats towards a 'spawnPoint' (usually attached to the VR Camera).
 * - It includes "Billboarding" logic to ensure the object always rotates to face the user.
 */

using UnityEngine;
using Oculus.Interaction.Input;

public class SpawnNearHMDOnGesture : MonoBehaviour
{
    [Header("What to show")]
    public GameObject targetObject;     // The UI Canvas or 3D object to toggle

    [Header("Where to spawn")]
    public Transform spawnPoint;        // Usually an empty GameObject parented to the CenterEyeAnchor (Head)
    public bool followWhileVisible = true; // If true, the object floats with the head. If false, it stays where it spawned.

    [Header("HMD & Hand")]
    public Transform hmdTransform;      // The VR Camera (CenterEyeAnchor)
    public HandRef leftHand;            // Interaction SDK reference (Preferred)

    [Tooltip("OVRHand component from the hand-tracking rig (alternative to HandRef)")]
    public OVRHand ovrLeftHand;         // Native OVR Plugin reference (Fallback)


    [Header("Placement")]
    public Vector3 extraLocalOffset = Vector3.zero; // Fine-tuning position relative to the spawn point
    [Range(0f, 20f)] public float followLerp = 12f; // How fast the object catches up to the head movement (higher = snappier)
    public bool faceCamera = true;      // If true, the object rotates to look at the player

    // Internal state
    private bool _isActive;
    private bool _pinchLastFrame;

    void Awake()
    {
        // Ensure the menu starts hidden
        if (targetObject != null) targetObject.SetActive(false);
        
        // Auto-find main camera if HMD transform is missing
        if (hmdTransform == null && Camera.main != null)
            hmdTransform = Camera.main.transform; 
    }

    void Update()
    {
        // 1. Detect Gesture input
        // Supports both Interaction SDK (HandRef) and legacy OVRHand
        bool isPinching = false;
        if (leftHand != null)
            isPinching = leftHand.GetIndexFingerIsPinching();
        else if (ovrLeftHand != null && 
                ovrLeftHand.HandConfidence == OVRHand.TrackingConfidence.High)
            isPinching = ovrLeftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);

        // 2. Handle Toggle Logic (Edge Detection)
        // Only toggle on the specific frame the pinch starts (IsPinching changed from false to true)
        if (isPinching && !_pinchLastFrame)
        {
            Toggle();
        }
        _pinchLastFrame = isPinching;

        // 3. Handle Floating/Following Behavior
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

        // When opening, snap immediately to position (don't lerp in from zero)
        if (_isActive)
        {
            PoseToSpawnPoint(targetObject.transform, smooth: false);
        }
    }

    // Calculus for positioning the object relative to the head
    private void PoseToSpawnPoint(Transform t, bool smooth)
    {
        Vector3 desiredPos = spawnPoint.TransformPoint(extraLocalOffset);

        Quaternion desiredRot = spawnPoint.rotation;
        
        // Billboarding logic: Rotate object to face the camera
        if (faceCamera && hmdTransform != null)
        {
            Vector3 toCam = hmdTransform.position - desiredPos;
            
            // Flatten the vector on Y so the menu stays upright (doesn't tilt up/down)
            toCam.y = 0f; 
            
            if (toCam.sqrMagnitude > 0.0001f)
                desiredRot = Quaternion.LookRotation(toCam.normalized, Vector3.up);
        }

        // Apply movement
        if (smooth)
        {
            // Smoothly interpolate for a high-quality "floating" feel
            t.position = Vector3.Lerp(t.position, desiredPos, Time.deltaTime * followLerp);
            t.rotation = Quaternion.Slerp(t.rotation, desiredRot, Time.deltaTime * followLerp);
        }
        else
        {
            // Snap instantly
            t.position = desiredPos;
            t.rotation = desiredRot;
        }
    }

    // Public API for external events (e.g., Unity Events)
    public void Show() { if (!targetObject.activeSelf) Toggle(); }
    public void Hide() { if (targetObject.activeSelf) Toggle(); }
}
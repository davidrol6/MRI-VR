/* * SCRIPT SUMMARY:
 * This script adapts the standard OVR Hand Tracking pinch gesture into an 'IActiveState' signal 
 * for the Oculus Interaction SDK.
 * * * How it works:
 * - It implements the 'IActiveState' interface, which is the standard boolean signal used by the Interaction SDK to enable/disable components.
 * - It references the core 'OVRHand' component to access raw hand tracking data.
 * - It returns 'Active = true' only when the specified finger (e.g., Index) is pinching and tracking confidence is sufficient.
 * - This allows standard hand gestures to trigger Interaction SDK events or enable interactors.
 */

using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;


public class HandPinchActiveState : MonoBehaviour, IActiveState
{
    [Tooltip("Reference to the OVRHand (e.g. LeftHandAnchor > OVRHand).")]
    public OVRHand Hand; // Reference to the low-level hand tracking script provided by Oculus/Meta

    [Tooltip("Which finger must perform the pinch gesture.")]
    public OVRHand.HandFinger Finger = OVRHand.HandFinger.Index; // Default is Index finger

    [Tooltip("Tracking confidence threshold (0-1). If below this, the gesture won't activate.")]
    [Range(0f, 1f)]
    public float MinConfidence = 0.3f;

    // --- IActiveState Implementation ---
    // The Interaction SDK polls this property to know if this "state" is currently valid.
    public bool Active => IsPinching();

    private bool IsPinching()
    {
        // Safety check to prevent errors if the hand reference is missing
        if (Hand == null) 
            return false;

        // Basic confidence check: ignore gesture if tracking is too weak or lost
        // This prevents "ghost" pinches when the hand is partially occluded
        if (Hand.HandConfidence < OVRHand.TrackingConfidence.High && MinConfidence > 0.5f)
            return false;

        // Return whether the selected finger is currently pinching according to the OVR Plugin
        return Hand.GetFingerIsPinching(Finger);
    }
}

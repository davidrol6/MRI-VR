using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;


public class HandPinchActiveState : MonoBehaviour, IActiveState
{
    [Tooltip("Reference to the OVRHand (e.g. LeftHandAnchor > OVRHand).")]
    public OVRHand Hand;

    [Tooltip("Which finger must perform the pinch gesture.")]
    public OVRHand.HandFinger Finger = OVRHand.HandFinger.Index;

    [Tooltip("Tracking confidence threshold (0-1). If below this, the gesture won't activate.")]
    [Range(0f, 1f)]
    public float MinConfidence = 0.3f;

    // IActiveState implementation
    public bool Active => IsPinching();

    private bool IsPinching()
    {
        if (Hand == null) 
            return false;

        // Basic confidence check: ignore gesture if tracking is too weak
        if (Hand.HandConfidence < OVRHand.TrackingConfidence.High && MinConfidence > 0.5f)
            return false;

        // Return whether the selected finger is currently pinching
        return Hand.GetFingerIsPinching(Finger);
    }
}

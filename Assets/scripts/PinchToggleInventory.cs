using UnityEngine;

public class PinchToggleInventory : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("OVRHand component from LeftHand (hand-tracking rig)")]
    public OVRHand leftHand;
    [Tooltip("Optional: HandGrabInteractor from left hand; prevents toggle while grabbing")]
    public Oculus.Interaction.HandGrab.HandGrabInteractor leftHandGrabInteractor;
    [Tooltip("Your InventoryVR script")]
    public InventoryVR inventory;

    [Header("Pinch thresholds (hysteresis)")]
    [Range(0f, 1f)] public float pinchOn  = 0.85f;
    [Range(0f, 1f)] public float pinchOff = 0.60f;

    [Header("Safety checks")]
    public bool requireHighConfidence = true;
    public bool requireHandTracked = true;
    public bool blockWhileGrabbing = true;
    public float minCooldownSeconds = 0.25f;

    private bool _pinching;
    private float _lastToggleTime;

    void Update()
    {
        if (leftHand == null || inventory == null) return;

        // Hand state guards
        if (requireHandTracked && !leftHand.IsTracked) return;
        if (requireHighConfidence && leftHand.HandConfidence != OVRHand.TrackingConfidence.High) return;

        if (blockWhileGrabbing && leftHandGrabInteractor != null)
        {
            if (leftHandGrabInteractor.HasSelectedInteractable) return; // don’t toggle while holding an item
        }

        // Read pinch strength (0..1) for index finger
        float s = leftHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);

        // Rising edge: pinch begins
        if (!_pinching && s >= pinchOn)
        {
            if (Time.time - _lastToggleTime >= minCooldownSeconds)
            {
                inventory.Toggle();
                _lastToggleTime = Time.time;
            }
            _pinching = true;
            return;
        }

        // Falling edge: pinch ends
        if (_pinching && s <= pinchOff)
        {
            _pinching = false;
        }
    }
}

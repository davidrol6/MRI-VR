using UnityEngine;

public class SimplePinchToggle : MonoBehaviour
{
    public OVRHand leftHand;       // Drag your OVRHand here
    public GameObject targetObject; // The object you want to toggle

    private bool isActive = false;
    private bool pinchLastFrame = false;

    void Update()
    {
        if (leftHand == null || targetObject == null) return;

        // Get pinch state (true when index finger is pinching)
        bool isPinching = leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);

        // Detect the *moment* the pinch starts (rising edge)
        if (isPinching && !pinchLastFrame)
        {
            isActive = !isActive;                   // flip state
            targetObject.SetActive(isActive);       // toggle object

            if (isActive)
            {
                var poser = targetObject.GetComponent<InventoryGestureController>();
                if (poser != null)
                    poser.ReposeNow();
            }

        }

        pinchLastFrame = isPinching;
    }
}

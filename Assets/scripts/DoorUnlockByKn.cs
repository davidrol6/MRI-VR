using UnityEngine;

public class KnobRotationUnlock : MonoBehaviour
{
    [Header("Assign objects")]
    public Transform knobTransform;       // The rotating knob
    public LockDoorTransform door;        // Door script that handles lock/unlock

    [Header("Settings")]
    public float unlockAngleThreshold = 30f; 
    public bool alreadyUnlocked = false;

    void Update()
    {
        // Check if the door is fully in the locked position (not open)
        bool doorAtOriginalPos = door.IsAtLockedPosition();

        // Current rotation of the knob on the X axis
        float angle = knobTransform.localEulerAngles.x;

        // Convert 0–360° to a signed range (-180, 180)
        if (angle > 180f) angle -= 360f;

        // If the knob rotates past the threshold: unlock once
        if (Mathf.Abs(angle) >= unlockAngleThreshold && !alreadyUnlocked)
        {
            door.Unlock();
            alreadyUnlocked = true;
            Debug.Log("Door unlocked by knob rotation");
        }
        // If the knob returns to neutral AND the door is physically back in locked position: re-lock
        else if (Mathf.Abs(angle) < unlockAngleThreshold && alreadyUnlocked && doorAtOriginalPos)
        {
            alreadyUnlocked = false;
            door.Lock();
            Debug.Log("Door locked because the knob returned to its neutral position and the door is closed");
        }
    }
}

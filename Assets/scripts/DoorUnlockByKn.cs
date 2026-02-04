/* * SCRIPT SUMMARY:
 * This script controls the locking mechanism of a door based on the rotation of a physical knob or handle.
 * * * How it works:
 * - It continuously checks the local rotation (X-axis) of the assigned knob.
 * - When the knob rotates past a specific threshold (e.g., 30 degrees), it signals the door to Unlock.
 * - If the knob returns to the neutral position AND the door is physically closed, it triggers the Lock command.
 */

using UnityEngine;

public class KnobRotationUnlock : MonoBehaviour
{
    [Header("Assign objects")]
    public Transform knobTransform;       // Reference to the visual object of the knob
    public LockDoorTransform door;        // Reference to the logic script controlling the door's state

    [Header("Settings")]
    public float unlockAngleThreshold = 30f; // Degrees of rotation required to trigger the unlock
    public bool alreadyUnlocked = false;     // Internal state tracker

    void Update()
    {
        // Safety check: ensure the door is physically in the closed position before allowing a re-lock
        bool doorAtOriginalPos = door.IsAtLockedPosition();

        // Get the current rotation of the knob on the X axis
        float angle = knobTransform.localEulerAngles.x;

        // Convert Unity's 0-360 degree format to a signed -180 to 180 format 
        // This makes it easier to detect rotation in either direction (up or down)
        if (angle > 180f) angle -= 360f;

        // CHECK 1: UNLOCK LOGIC
        // If the knob is rotated beyond the threshold (positive or negative)
        if (Mathf.Abs(angle) >= unlockAngleThreshold && !alreadyUnlocked)
        {
            door.Unlock();
            alreadyUnlocked = true;
            Debug.Log("Door unlocked by knob rotation");
        }
        // CHECK 2: RE-LOCK LOGIC
        // If the knob returns to neutral, was previously unlocked, AND the door is fully closed
        else if (Mathf.Abs(angle) < unlockAngleThreshold && alreadyUnlocked && doorAtOriginalPos)
        {
            alreadyUnlocked = false;
            door.Lock();
            Debug.Log("Door locked because the knob returned to its neutral position and the door is closed");
        }
    }
}
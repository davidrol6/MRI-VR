/* * SCRIPT SUMMARY:
 * This script enforces a physical "Lock" state on a door or object by overriding its transform every frame.
 * * * How it works:
 * - When `Lock()` is called (or at Start), it records the object's current position and rotation.
 * - In `LateUpdate`, if the object is locked, it continuously forces the object back to those recorded coordinates.
 * - This effectively overrides any physics forces or user attempts to open the door while it is locked.
 * - It provides an `Unlock()` method to release this constraint and a helper to check if the door is currently closed.
 */

using UnityEngine;

public class LockDoorTransform : MonoBehaviour
{
    private Vector3 lockedPosition;    // Stores the position where the door is locked
    private Quaternion lockedRotation; // Stores the rotation where the door is locked
    private bool isLocked = false;     // Internal state flag

    private void Start()
    {
        // Automatically lock the door at the start of the scene (assuming it starts closed)
        Lock();
        Debug.Log("Door locked on start.");
    }

    // Call this to freeze the door in its current position
    public void Lock()
    {
        // Store the current transform as the locked pose
        lockedPosition = transform.position;
        lockedRotation = transform.rotation;
        isLocked = true;

        Debug.Log("Door locked");
    }

    // Call this to release the door (allowing physics or user interaction to move it)
    public void Unlock()
    {
        isLocked = false;
        Debug.Log("Door unlocked");
    }

    // LateUpdate runs after Update, ensuring we override any movement applied during the frame
    private void LateUpdate()
    {
        // If locked, force the door back to its stored transform
        // This overrides physics or hand interactions attempting to pull it open
        if (isLocked)
        {
            transform.position = lockedPosition;
            transform.rotation = lockedRotation;
        }
    }

    // Helper method: Returns true if the door is close enough to the saved locked rotation
    // Useful for logic that checks if the door is "shut" before re-locking
    public bool IsAtLockedPosition(float tolerance = 0.01f)
    {
        return Quaternion.Angle(transform.rotation, lockedRotation) <= tolerance;
    }
}
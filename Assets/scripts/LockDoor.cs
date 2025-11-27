using UnityEngine;

public class LockDoorTransform : MonoBehaviour
{
    private Vector3 lockedPosition;
    private Quaternion lockedRotation;
    private bool isLocked = false;

    private void Start()
    {
        Lock();
        Debug.Log("Door locked on start.");
    }

    public void Lock()
    {
        // Store the current transform as the locked pose
        lockedPosition = transform.position;
        lockedRotation = transform.rotation;
        isLocked = true;

        Debug.Log("Door locked");
    }

    public void Unlock()
    {
        isLocked = false;
        Debug.Log("Door unlocked");
    }

    private void LateUpdate()
    {
        // If locked, force the door back to its stored transform
        if (isLocked)
        {
            transform.position = lockedPosition;
            transform.rotation = lockedRotation;
        }
    }

    // Returns true if the door is close enough to the saved locked rotation
    public bool IsAtLockedPosition(float tolerance = 0.01f)
    {
        return Quaternion.Angle(transform.rotation, lockedRotation) <= tolerance;
    }
}

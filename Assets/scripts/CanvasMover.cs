/* * SCRIPT SUMMARY:
 * This script handles the smooth movement and rotation of a UI object (Canvas) between two positions.
 * * * How it works:
 * It defines a 'Spawn Point' (start) and a 'Target Point' (destination).
 * The Update loop continuously interpolates the object's position towards the currently selected target.
 * This is used to create smooth sliding effects for menus or information panels entering/exiting the view.
 */

using UnityEngine;

public class CanvasMover : MonoBehaviour
{
    [Header("Canvas movement points")]
    public Transform spawnPoint;  // The starting location and rotation
    public Transform targetPoint; // The destination location and rotation

    [Header("Movement settings")]
    public float moveSpeed = 5f; // Controls the interpolation speed (smoothness)

    private Transform currentTarget; // Internal reference to the active destination

    void Start()
    {
        // specific logic to snap the object to the start position immediately
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
        currentTarget = null; // Ensure no movement happens automatically at start
    }

    void Update()
    {
        // Only calculate movement if a target has been explicitly set
        if (currentTarget != null)
        {
            // Smooth movement logic using Linear Interpolation (Lerp)
            // Time.deltaTime ensures the speed is consistent regardless of frame rate
            transform.position = Vector3.Lerp(transform.position, currentTarget.position, Time.deltaTime * moveSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, currentTarget.rotation, Time.deltaTime * moveSpeed);
        }
    }

    // Call this method (e.g., from a "Back" button) to return to the start
    public void MoveToSpawn()
    {
        currentTarget = spawnPoint;
    }

    // Call this method (e.g., from a "Open Menu" button) to move to the destination
    public void MoveToTarget()
    {
        currentTarget = targetPoint;
    }
}
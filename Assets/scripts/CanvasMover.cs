using UnityEngine;

public class CanvasMover : MonoBehaviour
{
    [Header("Canvas movement points")]
    public Transform spawnPoint;
    public Transform targetPoint;

    [Header("Movement settings")]
    public float moveSpeed = 5f; // how fast it moves

    private Transform currentTarget;

    void Start()
    {
        // Start at spawn point
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
        currentTarget = null;
    }

    void Update()
    {
        if (currentTarget != null)
        {
            // Smooth movement
            transform.position = Vector3.Lerp(transform.position, currentTarget.position, Time.deltaTime * moveSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, currentTarget.rotation, Time.deltaTime * moveSpeed);
        }
    }

    // Call this from a button
    public void MoveToSpawn()
    {
        currentTarget = spawnPoint;
    }

    // Call this from another button
    public void MoveToTarget()
    {
        currentTarget = targetPoint;
    }
}

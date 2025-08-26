using UnityEngine;

public class LockDoorTransform : MonoBehaviour
{
    private Vector3 lockedPosition;
    private Quaternion lockedRotation;
    private bool isLocked = false;

    public void Start()
    {
        Lock();
        Debug.Log("---- PÙERTA BLOQUEADA --------");
    }

    public void Lock()
    {
        lockedPosition = transform.position;
        lockedRotation = transform.rotation;
        isLocked = true;
        Debug.Log("🚪 Puerta bloqueada");
    }

    public void Unlock()
    {
        isLocked = false;
        Debug.Log("🔓 Puerta desbloqueada");
    }

    void LateUpdate()
    {
        if (isLocked)
        {
            transform.position = lockedPosition;
            transform.rotation = lockedRotation;
        }
    }

    public bool IsAtLockedPosition(float tolerance = 0.01f)
    {
        //Debug.Log("---------------- COMPROBANDO ANGULOS -----------------" + transform.rotation + " " + lockedRotation + "-" + Quaternion.Angle(transform.rotation, lockedRotation)+ "-"+tolerance);
         return Quaternion.Angle(transform.rotation, lockedRotation) <= tolerance;
    }
}
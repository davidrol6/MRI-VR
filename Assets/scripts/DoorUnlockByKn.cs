using UnityEngine;

public class KnobRotationUnlock : MonoBehaviour
{
    [Header("Asignar objetos")]
    public Transform knobTransform; 
    public LockDoorTransform door;  

    [Header("Ajustes")]
    public float unlockAngleThreshold = 30f; 
    public bool alreadyUnlocked = false;

    void Update()
    {
        bool doorAtOriginalPos = door.IsAtLockedPosition();
        float angle = knobTransform.localEulerAngles.x;

        if (angle > 180f) angle -= 360f;
        //Debug.Log("-////////////// EL ANGULO ////////////" + angle.ToString("F6"));
        if (Mathf.Abs(angle) >= unlockAngleThreshold && !alreadyUnlocked)
        {
            door.Unlock();
            alreadyUnlocked = true;
            Debug.Log("🔓 Puerta desbloqueada por giro del pomo");
        } else if (Mathf.Abs(angle) < unlockAngleThreshold && alreadyUnlocked && doorAtOriginalPos)
        {
            alreadyUnlocked = false;
            door.Lock();
            Debug.Log(doorAtOriginalPos + "🔒 Puerta bloqueada porque el pomo volvió a la posición inicial y la puerta está cerrada");
        }
    }
}

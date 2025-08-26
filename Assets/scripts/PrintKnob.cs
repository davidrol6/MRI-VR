using UnityEngine;

public class PrintKnobRotation : MonoBehaviour
{
    void Update()
    {
        Vector3 euler = transform.localEulerAngles;

        // Corrige los valores para que estén entre -180 y 180
        if (euler.x > 180) euler.x -= 360;
        if (euler.y > 180) euler.y -= 360;
        if (euler.z > 180) euler.z -= 360;

        Debug.Log($"Rotación local del pomo: X={euler.x:F2}, Y={euler.y:F2}, Z={euler.z:F2}");
    }
}
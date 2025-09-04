using UnityEngine;
using Oculus.Interaction.Input;

public class ToggleWithPinch : MonoBehaviour
{
    public GameObject objeto;
    public HandRef leftHand;  // arrastra aquí tu mano izquierda desde la escena
    private bool isActive = false;
    private bool pinchLastFrame = false;

    void Update()
    {
        bool isPinching = leftHand.GetIndexFingerIsPinching();

        // Detectar el inicio del gesto (cuando empieza el pinch, no cuando se mantiene)
        if (isPinching && !pinchLastFrame)
        {
            isActive = !isActive;
            objeto.SetActive(isActive);
        }

        pinchLastFrame = isPinching;
    }
}

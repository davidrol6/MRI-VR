using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;

/// <summary>
/// ActiveState que se activa cuando un OVRHand hace pinch con un dedo.
/// Funciona igual que OVRButtonActiveState pero con gestos de mano.
/// </summary>
public class HandPinchActiveState : MonoBehaviour, IActiveState
{
    [Tooltip("Referencia a la mano OVR (ej: LeftHandAnchor > OVRHand)")]
    public OVRHand Hand;

    [Tooltip("Qué dedo debe pinchar para activar el estado")]
    public OVRHand.HandFinger Finger = OVRHand.HandFinger.Index;

    [Tooltip("Umbral de confianza (0-1). Si la mano no está trackeada con suficiente confianza, no se activa.")]
    [Range(0f, 1f)]
    public float MinConfidence = 0.3f;

    public bool Active => IsPinching();

    private bool IsPinching()
    {
        if (Hand == null) return false;

        // Comprueba que el tracking de la mano es válido
        if (Hand.HandConfidence < OVRHand.TrackingConfidence.High && MinConfidence > 0.5f)
        {
            return false;
        }

        // Devuelve si el dedo seleccionado está en "pinching"
        return Hand.GetFingerIsPinching(Finger);
    }
}

using System.Collections;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class KnobResetOnRelease : MonoBehaviour
{
    [Header("Referencias")]
    public Transform knobTransform;
    public HandGrabInteractable handGrabInteractable;

    private Quaternion initialRotation;

    public OneGrabRotateTransformer currentTransformer;

    void Start()
    {
        initialRotation = knobTransform.localRotation;

        currentTransformer = GetComponent<OneGrabRotateTransformer>();
        if (currentTransformer == null)
        {
            Debug.LogError("No se encontró OneGrabRotateTransformer en el objeto");
        }
    }

    public void ResetRotation()
    {
       knobTransform.localRotation = initialRotation;
    }


}

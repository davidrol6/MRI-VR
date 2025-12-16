using UnityEngine;
using UnityEngine.UI;

public class SelectorTransformChangerComplex : MonoBehaviour
{
    [Header("Object to Move")]
    public Transform target;

    [Header("Trigger Toggle (C)")]
    [Tooltip("Toggle that executes the action")]
    public Toggle triggerToggleC; 

    [Header("Selector A")]
    public Toggle toggleA;
    public Transform referenceA; 

    [Header("Selector B")]
    public Toggle toggleB;
    public Transform referenceB;

    [Header("Options")]
    public bool changePosition = true;
    public bool changeRotation = true;
    public bool changeScale = false;

    private Transform currentSelectedReference;

    void Start()
    {
        
        if (toggleA != null)
        {
            toggleA.onValueChanged.AddListener((isOn) => 
            { 
                if (isOn) currentSelectedReference = referenceA; 
            });
        }

        if (toggleB != null)
        {
            toggleB.onValueChanged.AddListener((isOn) => 
            { 
                if (isOn) currentSelectedReference = referenceB; 
            });
        }

        if (triggerToggleC != null)
        {
            triggerToggleC.onValueChanged.AddListener(OnTriggerChanged);
        }

        if (toggleA != null && toggleA.isOn) currentSelectedReference = referenceA;
        else if (toggleB != null && toggleB.isOn) currentSelectedReference = referenceB;
    }

    void OnTriggerChanged(bool isOn)
    {
        if (isOn && currentSelectedReference != null)
        {
            ApplyTransform(currentSelectedReference);
        }
    }

    void ApplyTransform(Transform refTrans)
    {
        if (!target || !refTrans) return;

        if (changePosition)
            target.position = refTrans.position;

        if (changeRotation)
            target.rotation = refTrans.rotation;

        if (changeScale)
            target.localScale = refTrans.localScale;
    }
}
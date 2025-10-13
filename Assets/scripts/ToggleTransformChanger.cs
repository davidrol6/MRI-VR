using UnityEngine;
using UnityEngine.UI;

public class ToggleTransformChanger : MonoBehaviour
{
    [Header("UI Toggle")]
    public Toggle toggle; 

    [Header("Object to Move/Rotate/Scale")]
    public Transform target;

    [Header("Reference Transform (desired position/rotation/scale)")]
    public Transform reference;

    [Header("Options")]
    public bool changePosition = true;
    public bool changeRotation = true;
    public bool changeScale = false;

    void Start()
    {
        if (toggle != null)
            toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            ApplyTransform();
        }
    }

    void ApplyTransform()
    {
        if (!target || !reference) return;

        if (changePosition)
            target.position = reference.position;

        if (changeRotation)
            target.rotation = reference.rotation;

        if (changeScale)
            target.localScale = reference.localScale;
    }
}

/* * SCRIPT SUMMARY:
 * This script links a UI Toggle to a physical transformation in the scene.
 * * * How it works:
 * - It listens for the 'OnValueChanged' event of a UI Toggle.
 * - When the toggle is switched ON, it instantly snaps a 'Target' object to match the position, rotation, and/or scale of a 'Reference' object.
 * - It allows selective application (e.g., only copy rotation, but leave position alone).
 * - Useful for resetting objects, opening doors instantly for debugging, or swapping view positions.
 */

using UnityEngine;
using UnityEngine.UI;

public class ToggleTransformChanger : MonoBehaviour
{
    [Header("UI Toggle")]
    public Toggle toggle; // The UI checkbox/switch that triggers the change

    [Header("Object to Move/Rotate/Scale")]
    public Transform target; // The object that will be moved

    [Header("Reference Transform (desired position/rotation/scale)")]
    public Transform reference; // An empty GameObject placed in the scene where the target should go

    [Header("Options")]
    public bool changePosition = true;
    public bool changeRotation = true;
    public bool changeScale = false;

    void Start()
    {
        // Setup listener to detect when the user clicks the toggle
        if (toggle != null)
            toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    // Callback fired by the UI system
    void OnToggleChanged(bool isOn)
    {
        // Only apply the transform when the toggle is turned ON.
        // (Logic for turning it OFF is not defined in this script).
        if (isOn)
        {
            ApplyTransform();
        }
    }

    // Copies the properties from the Reference to the Target
    void ApplyTransform()
    {
        if (!target || !reference) return;

        // Apply World Space position
        if (changePosition)
            target.position = reference.position;

        // Apply World Space rotation
        if (changeRotation)
            target.rotation = reference.rotation;

        // Apply Local Space scale (Scale is almost always handled locally in Unity hierarchy)
        if (changeScale)
            target.localScale = reference.localScale;
    }
}
/* * SCRIPT SUMMARY:
 * This script links a UI Button to a physical transformation in the scene.
 * * * How it works:
 * - It listens for the 'OnClick' event of a Unity UI Button.
 * - When the button is pressed, the 'Target' object instantly matches the position, rotation, and/or scale of a 'Reference' object.
 * - It allows selective application (e.g., only copy rotation, but leave position alone).
 * - Useful for resetting objects, jumping to specific camera angles, or instant repositioning during debugging.
 */

using UnityEngine;
using UnityEngine.UI;

public class ButtonTransformChanger : MonoBehaviour
{
    [Header("UI Button")]
    public Button actionButton; // The UI button that triggers the change

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
        // Setup listener to detect when the user clicks the button
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogWarning("Ali, remember to assign the Button in the Inspector for " + gameObject.name);
        }
    }

    // Callback fired by the UI system
    void OnButtonClicked()
    {
        ApplyTransform();
    }

    // Copies the properties from the Reference to the Target
    void ApplyTransform()
    {
        // Null check to prevent errors
        if (target == null || reference == null) 
        {
            Debug.LogError("Target or Reference Transform is missing in the inspector.");
            return;
        }

        // Apply World Space position
        if (changePosition)
            target.position = reference.position;

        // Apply World Space rotation
        if (changeRotation)
            target.rotation = reference.rotation;

        // Apply Local Space scale (Scale is almost always handled locally in Unity hierarchy)
        if (changeScale)
            target.localScale = reference.localScale;
        
        Debug.Log("Transform applied successfully via Button click.");
    }

    // Clean up listeners when the object is destroyed to prevent memory leaks
    void OnDestroy()
    {
        if (actionButton != null)
            actionButton.onClick.RemoveListener(OnButtonClicked);
    }
}
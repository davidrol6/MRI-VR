/* * SCRIPT SUMMARY:
 * This script allows the user to choose between two different destinations (A or B) 
 * and then trigger a movement to the selected destination using a third toggle (C).
 * * * How it works:
 * - It monitors two "Selector" Toggles (A and B). Selecting one updates the `currentSelectedReference`.
 * - It monitors a third "Trigger" Toggle (C).
 * - When Toggle C is activated, the Target object snaps to whichever reference (A or B) was last selected.
 * - This creates a "Select -> Execute" workflow (e.g., "Select Floor 1", "Select Floor 2", "Go").
 */

using UnityEngine;
using UnityEngine.UI;

public class SelectorTransformChangerComplex : MonoBehaviour
{
    [Header("Object to Move")]
    public Transform target; // The object that will be moved (e.g., the Player Rig or a Camera)

    [Header("Trigger Toggle (C)")]
    [Tooltip("Toggle that executes the action")]
    public Toggle triggerToggleC; // The "Execute" button

    [Header("Selector A")]
    public Toggle toggleA;        // Button to select Destination A
    public Transform referenceA;  // The position of Destination A

    [Header("Selector B")]
    public Toggle toggleB;        // Button to select Destination B
    public Transform referenceB;  // The position of Destination B

    [Header("Options")]
    public bool changePosition = true;
    public bool changeRotation = true;
    public bool changeScale = false;

    // Internal state to remember which destination is currently picked
    private Transform currentSelectedReference;

    void Start()
    {
        // 1. Setup Listener for Selector A
        if (toggleA != null)
        {
            toggleA.onValueChanged.AddListener((isOn) => 
            { 
                if (isOn) currentSelectedReference = referenceA; 
            });
        }

        // 2. Setup Listener for Selector B
        if (toggleB != null)
        {
            toggleB.onValueChanged.AddListener((isOn) => 
            { 
                if (isOn) currentSelectedReference = referenceB; 
            });
        }

        // 3. Setup Listener for the Trigger (C)
        if (triggerToggleC != null)
        {
            triggerToggleC.onValueChanged.AddListener(OnTriggerChanged);
        }

        // 4. Initialize default selection based on what is checked in the editor
        if (toggleA != null && toggleA.isOn) currentSelectedReference = referenceA;
        else if (toggleB != null && toggleB.isOn) currentSelectedReference = referenceB;
    }

    // Called when the "Execute" toggle is flipped
    void OnTriggerChanged(bool isOn)
    {
        // Only execute on switch ON, and only if a valid destination is selected
        if (isOn && currentSelectedReference != null)
        {
            ApplyTransform(currentSelectedReference);
        }
    }

    // Moves the target to the chosen reference
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
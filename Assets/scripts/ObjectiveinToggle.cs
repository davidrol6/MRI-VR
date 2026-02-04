/* * SCRIPT SUMMARY:
 * This script bridges standard Unity UI components (Buttons and Toggles) with the simulation's Objective System.
 * * * How it works:
 * - It listens for events from a UI Button (onClick) or a UI Toggle (onValueChanged).
 * - When the user interacts with the UI (e.g., clicking "Start Scan" or toggling "Contrast"), it signals the ObjectiveManager.
 * - This allows 2D interface actions to complete game objectives just like physical interactions.
 */

using UnityEngine;
using UnityEngine.UI;

public class UIButtonObjectiveTrigger : MonoBehaviour
{
    [Header("Reference to ObjectiveManager")]
    public ObjectiveManager objectiveManager; // Reference to the core logic tracking progress

    [Header("Exact objective description")]
    public string objectiveDescription;       // The ID string that matches the objective in the Manager

    [Header("For toggles (optional)")]
    public Toggle toggleButton;

    [Header("For regular buttons (optional)")]
    public Button normalButton;

    private void Start()
    {
        // 1. Setup Toggle Listener
        // Triggers when the checkbox/switch state changes
        if (toggleButton != null)
        {
            toggleButton.onValueChanged.AddListener(OnToggleChanged);
        }

        // 2. Setup Button Listener
        // Triggers immediately when clicked
        if (normalButton != null)
        {
            normalButton.onClick.AddListener(OnButtonClicked);
        }
    }

    // Callback for Toggle components
    private void OnToggleChanged(bool isOn)
    {
        // Only complete the objective if the toggle is switched ON (ignore turning it off)
        if (isOn)
        {
            Debug.Log($"Toggle activated for objective: {objectiveDescription}");
            
            if (objectiveManager != null)
            {
                objectiveManager.CompleteObjective(objectiveDescription);
                objectiveManager.PrintObjectivesStatus();
            }
        }
    }

    // Callback for standard Button components
    private void OnButtonClicked()
    {
        Debug.Log("Button clicked: " + objectiveDescription);
        
        if (objectiveManager != null)
        {
            objectiveManager.CompleteObjective(objectiveDescription);
        }
    }
}
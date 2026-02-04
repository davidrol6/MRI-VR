/* * SCRIPT SUMMARY:
 * This script is a lightweight controller for a single entry in the Objective List UI.
 * * * How it works:
 * - It sits on a prefab (e.g., a "Task Row") containing a text label and a checkmark image.
 * - The 'Setup' method initializes the text description and current state.
 * - The 'Refresh' method updates the visual state (showing/hiding the checkmark) without reloading the text.
 */

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIObjectiveItem : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text descriptionText; // Reference to the TextMeshPro component for the task name
    public Image checkIcon;          // Reference to the checkmark image (enabled = complete)

    // Initializes the UI element with data from the Objective class
    public void Setup(Objective obj)
    {
        if (descriptionText != null)
            descriptionText.text = obj.description;
            
        if (checkIcon != null)
            checkIcon.enabled = obj.isCompleted; // Activates tick only if the task is already done
    }

    // Called to update the visual state (e.g., when the task is just completed)
    public void Refresh(Objective obj)
    {
        if (checkIcon != null)
            checkIcon.enabled = obj.isCompleted;
    }
}
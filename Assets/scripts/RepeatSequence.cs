/* * SCRIPT SUMMARY:
 * This script controls the "Repeat Sequence" button functionality in the Technician's UI.
 * * * How it works:
 * - It connects to a UI Toggle. When activated, it checks if the current scan result is "Blurred".
 * - If the image is blurred, it finds the corresponding image set in the ScanResultDisplay and swaps the blurred version for the sharp version.
 * - It then updates the global ClinicalCaseGenerator state to reflect that the error has been corrected.
 * - If the image is already clear, it prevents the action and resets the toggle.
 */

using UnityEngine;
using UnityEngine.UI;

public class RepeatSequenceToggle : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drop here the object with script ScanResultDisplay")]
    public ScanResultDisplay displayController; // Reference to the script handling the image GameObjects
    
    private Toggle myToggle; // The UI component attached to this GameObject

    private void Start()
    {
        myToggle = GetComponent<Toggle>();
        
        // Ensure the toggle starts unchecked (OFF) when the scene loads
        if (myToggle != null)
            myToggle.SetIsOnWithoutNotify(false);
    }

    // Connect this method to the Toggle's "OnValueChanged" event in the Inspector
    public void OnRepeatToggleChanged(bool isOn)
    {
        // Only trigger logic when switching ON (ignore switching OFF)
        if (!isOn) return;

        var generator = ClinicalCaseGenerator.Instance;

        // Safety check for dependencies
        if (generator == null || displayController == null)
        {
            Debug.LogWarning("Missing references in RepeatSequenceToggle");
            return;
        }
        
        // LOGIC: Only allow a repeat if the current case is actually blurred
        if (generator.isImageBlurred)
        {
            Debug.Log("Repeating sequence...");

            // Find the active image set corresponding to the current patient case
            foreach (var config in displayController.allImages)
            {
                if (config.bodyPart == generator.currentBodyPart)
                {
                    // 1. Visual Fix: Swap the blurred object for the sharp object
                    if (config.blurredImage != null) config.blurredImage.SetActive(false);
                    if (config.sharpImage != null) config.sharpImage.SetActive(true);

                    // 2. Logic Fix: Update global state so the exam can now be finished
                    generator.isImageBlurred = false;

                    // 3. UI Feedback: Disable the button so it cannot be pressed again
                    if (myToggle != null) myToggle.interactable = false;

                    return; // Exit loop once handled
                }
            }
        }
        else
        {
            // If the image is already sharp, the user shouldn't be repeating the sequence
            Debug.Log("User tried to repeat but image was already sharp.");
            
            // Reset the toggle to OFF immediately without triggering this function again
            if (myToggle != null)
            {
                myToggle.SetIsOnWithoutNotify(false);
            }
        }
    }
}
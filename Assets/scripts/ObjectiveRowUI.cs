/* * SCRIPT SUMMARY:
 * This script controls the visual representation of a single objective in the user interface list.
 * * * How it works:
 * - It is a UI controller attached to a prefab (representing a single row in a task list).
 * - It has a `Bind` method to link it to a specific `Objective` data object.
 * - It updates the text, toggle state, and colors (e.g., green for complete, strikethrough text) to reflect the current status of that objective.
 * - It serves as a passive display; the user cannot interact with the toggle directly (it is read-only).
 */

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveRowUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;     // The main label showing the objective description
    public TextMeshProUGUI pointsText;    // Optional label showing points value (e.g., "10 pts")
    public Toggle toggle;                 // The checkbox component

    [Header("Colors & Style")]
    public Color pendingTextColor = Color.white;
    public Color completedTextColor = Color.green;

    public Color pendingCheckColor = Color.white;
    public Color completedCheckColor = Color.green;

    public bool useStrikethroughOnComplete = true; // Visual style preference

    // Internal references
    private Objective boundObjective; // The data object this row is displaying
    private Image checkmarkImage;     // Reference to the checkmark graphic inside the toggle

    private void Awake()
    {
        // 1. Auto-assign references if missing
        if (toggle == null)
            toggle = GetComponentInChildren<Toggle>();

        if (titleText == null)
            titleText = GetComponentInChildren<TextMeshProUGUI>();

        // 2. Configure Toggle behavior
        if (toggle != null)
        {
            var checkmark = toggle.graphic;
            checkmarkImage = checkmark as Image;
            
            // IMPORTANT: Disable interaction so the user can't manually check the box.
            // It must only change via code when the objective is actually completed.
            toggle.interactable = false; 
        }
    }

    // Called by the ObjectiveManager (or UI List generator) to set up this row
    public void Bind(Objective objective)
    {
        boundObjective = objective;

        // Set initial text
        if (titleText != null)
            titleText.text = objective.description;

        if (pointsText != null)
            pointsText.text = $"{objective.points} pts";

        // Refresh visual state
        UpdateState();
    }

    // Helper to check identity
    public bool MatchesObjective(Objective obj)
    {
        return boundObjective == obj;
    }

    // Refresh the visual style based on the objective's current data
    public void UpdateState()
    {
        if (boundObjective == null) return;

        bool completed = boundObjective.isCompleted;

        // 1. Update Toggle State (Checkbox)
        if (toggle != null)
            toggle.isOn = completed;

        // 2. Update Checkmark Color
        if (checkmarkImage != null)
            checkmarkImage.color = completed ? completedCheckColor : pendingCheckColor;

        // 3. Update Text Styling
        if (titleText != null)
        {
            // Change color
            titleText.color = completed ? completedTextColor : pendingTextColor;

            // Apply strikethrough if enabled
            if (useStrikethroughOnComplete)
            {
                titleText.fontStyle = completed
                    ? FontStyles.Strikethrough | FontStyles.Bold
                    : FontStyles.Normal;
            }
            else
            {
                titleText.fontStyle = FontStyles.Normal;
            }
        }

        // 4. Update Points Label Color
        if (pointsText != null)
        {
            pointsText.color = completed ? completedTextColor : pendingTextColor;
        }
    }
}
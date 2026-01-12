/* * SCRIPT SUMMARY:
 * This script manages the presentation of the final performance review at the end of the simulation.
 * * * How it works:
 * - It retrieves the final score and maximum possible score from the ObjectiveManager.
 * - It calculates the completion percentage and determines a qualitative feedback message (e.g., "Excellent", "Acceptable").
 * - It updates the UI text elements and activates the results panel to show the user their performance.
 */

using UnityEngine;
using TMPro;

public class FinalEvaluationUI : MonoBehaviour
{
    [Header("References")]
    public ObjectiveManager objectiveManager; // Reference to the core logic tracking progress

    [Tooltip("Panel or Canvas that contains the final evaluation UI")]
    public GameObject evaluationPanel; // The container object to show/hide

    [Tooltip("Text displaying the final score (e.g., '75 / 100 (75%)')")]
    public TextMeshProUGUI scoreText;

    [Tooltip("Text displaying the evaluation message")]
    public TextMeshProUGUI messageText;

    void Start()
    {
        // Ensure the evaluation panel is hidden when the scene starts
        // It should only appear when the user clicks "Finish"
        if (evaluationPanel != null)
            evaluationPanel.SetActive(false);
    }

    // This function is intended to be linked to the "Finish Exam" button (OnClick event)
    public void ShowFinalEvaluation()
    {
        // Fallback: Try to find the manager if not manually assigned
        if (objectiveManager == null)
            objectiveManager = ObjectiveManager.Instance;

        if (objectiveManager == null)
        {
            Debug.LogError("[FinalEvaluationUI] No ObjectiveManager assigned.");
            return;
        }

        // 1. Retrieve results
        int obtained = objectiveManager.GetFinalScore();
        int maxScore = objectiveManager.GetMaxScore();
        
        // Calculate percentage (Protect against division by zero if maxScore is 0)
        float percentage = (maxScore > 0) ? (obtained * 100f / maxScore) : 0f;

        // 2. Update UI Text
        if (scoreText != null)
            scoreText.text = $"{obtained} / {maxScore} ({percentage:F0}%)";

        if (messageText != null)
            messageText.text = GetEvaluationMessage(percentage);

        // 3. Reveal the results panel
        if (evaluationPanel != null)
            evaluationPanel.SetActive(true);

        Debug.Log($"[FinalEvaluationUI] Final evaluation: {obtained}/{maxScore} ({percentage:F0}%)");
    }

    // Returns a verbal evaluation string based on the calculated percentage
    private string GetEvaluationMessage(float percentage)
    {
        if (percentage >= 90f)
            return "Excellent execution of the procedure.";
        else if (percentage >= 75f)
            return "Good execution, with some details to improve.";
        else if (percentage >= 50f)
            return "Acceptable performance. Review the skipped steps.";
        else
            return "You should repeat the simulation and review the protocol.";
    }
}
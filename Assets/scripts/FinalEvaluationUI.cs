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
        if (objectiveManager == null)
            objectiveManager = ObjectiveManager.Instance;

        if (objectiveManager == null) {
            Debug.LogError("¡Ojo! ObjectiveManager sigue siendo NULL");
            return;
        }

        int obtained = objectiveManager.GetFinalScore();
        int maxScore = objectiveManager.GetMaxScore();
        
        Debug.Log($"Calculando: Obtenido={obtained}, Max={maxScore}");

        float percentage = (maxScore > 0) ? (obtained * 100f / maxScore) : 0f;

        if (scoreText != null) {
            scoreText.text = $"{obtained} / {maxScore} ({percentage:F0}%)";
            Debug.Log("Texto de puntuación asignado: " + scoreText.text);
        }

        if (messageText != null) {
            messageText.text = GetEvaluationMessage(percentage);
        }

        if (evaluationPanel != null)
            evaluationPanel.SetActive(true);
    }

    public void ResetUI()
    {
        if (evaluationPanel != null)
            evaluationPanel.SetActive(false);

        if (scoreText != null)
            scoreText.text = "";

        if (messageText != null)
            messageText.text = "";
        
        Debug.Log("[FinalEvaluationUI] Interfaz de evaluación reseteada.");
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
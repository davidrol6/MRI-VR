using UnityEngine;
using TMPro;

public class FinalEvaluationUI : MonoBehaviour
{
    [Header("References")]
    public ObjectiveManager objectiveManager;

    [Tooltip("Panel or Canvas that contains the final evaluation UI")]
    public GameObject evaluationPanel;

    [Tooltip("Text displaying the final score (e.g., '75 / 100 (75%)')")]
    public TextMeshProUGUI scoreText;

    [Tooltip("Text displaying the evaluation message")]
    public TextMeshProUGUI messageText;

    void Start()
    {
        // Optional: hide the panel at the start
        if (evaluationPanel != null)
            evaluationPanel.SetActive(false);
    }

    // This function is linked to the Finish button (OnClick)
    public void ShowFinalEvaluation()
    {
        if (objectiveManager == null)
            objectiveManager = ObjectiveManager.Instance;

        if (objectiveManager == null)
        {
            Debug.LogError("[FinalEvaluationUI] No ObjectiveManager assigned.");
            return;
        }

        // Retrieve results as weighted score 
        int obtained = objectiveManager.GetFinalScore();
        int maxScore = objectiveManager.GetMaxScore();
        float percentage = (maxScore > 0) ? (obtained * 100f / maxScore) : 0f;

        // Update UI text
        if (scoreText != null)
            scoreText.text = $"{obtained} / {maxScore} ({percentage:F0}%)";

        if (messageText != null)
            messageText.text = GetEvaluationMessage(percentage);

        // Show panel
        if (evaluationPanel != null)
            evaluationPanel.SetActive(true);

        Debug.Log($"[FinalEvaluationUI] Final evaluation: {obtained}/{maxScore} ({percentage:F0}%)");
    }

    // Returns a verbal evaluation based on total percentage
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

using UnityEngine;
using TMPro;   // Si usas TextMeshPro

public class FinalEvaluationUI : MonoBehaviour
{
    [Header("Referencias")]
    public ObjectiveManager objectiveManager;

    [Tooltip("Panel o Canvas que contiene la evaluación final")]
    public GameObject evaluationPanel;

    [Tooltip("Texto donde se muestra la puntuación, ej: '75 / 100 (75%)'")]
    public TextMeshProUGUI scoreText;

    [Tooltip("Texto donde se muestra el mensaje de valoración")]
    public TextMeshProUGUI messageText;

    void Start()
    {
        // Opcional: ocultar el panel al inicio
        if (evaluationPanel != null)
            evaluationPanel.SetActive(false);
    }

    // Esta función la enlazas al botón Finish (OnClick)
    public void ShowFinalEvaluation()
    {
        if (objectiveManager == null)
            objectiveManager = ObjectiveManager.Instance;

        if (objectiveManager == null)
        {
            Debug.LogError("[FinalEvaluationUI] No hay ObjectiveManager asignado.");
            return;
        }

        int obtained = objectiveManager.GetFinalScore();
        int maxScore = objectiveManager.GetMaxScore();
        float percentage = (maxScore > 0) ? (obtained * 100f / maxScore) : 0f;

        // Actualizar UI
        if (scoreText != null)
        {
            scoreText.text = $"{obtained} / {maxScore} ({percentage:F0}%)";
        }

        if (messageText != null)
        {
            messageText.text = GetEvaluationMessage(percentage);
        }

        if (evaluationPanel != null)
            evaluationPanel.SetActive(true);

        Debug.Log($"[FinalEvaluationUI] Evaluación final: {obtained}/{maxScore} ({percentage:F0}%)");
    }

    // Mensaje de texto según el porcentaje
    private string GetEvaluationMessage(float percentage)
    {
        if (percentage >= 90f)
            return "Excelente ejecución del procedimiento.";
        else if (percentage >= 75f)
            return "Buena ejecución, algunos detalles a mejorar.";
        else if (percentage >= 50f)
            return "Ejecución aceptable, revisa los pasos omitidos.";
        else
            return "Debes repetir la simulación y revisar el protocolo.";
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveRowUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI titleText;         // Descripción
    public TextMeshProUGUI pointsText;        // 🔹 Nuevo campo opcional para mostrar puntos
    public Toggle toggle;

    [Header("Colores y estilo")]
    public Color pendingTextColor = Color.white;
    public Color completedTextColor = Color.green;

    public Color pendingCheckColor = Color.white;
    public Color completedCheckColor = Color.green;

    public bool useStrikethroughOnComplete = true;

    // Referencias internas
    private Objective boundObjective;
    private Image checkmarkImage;

    private void Awake()
    {
        if (toggle == null)
            toggle = GetComponentInChildren<Toggle>();

        if (titleText == null)
            titleText = GetComponentInChildren<TextMeshProUGUI>();

        if (toggle != null)
        {
            var checkmark = toggle.graphic;
            checkmarkImage = checkmark as Image;
            toggle.interactable = false;
        }
    }

    public void Bind(Objective objective)
    {
        boundObjective = objective;

        // 🔹 Opción A: Description + puntos en una única línea
        // titleText.text = $"{objective.description}  <color=#888888>({objective.points} pts)</color>";

        // 🔹 Opción B: Description arriba, puntos en un TMP aparte
        if (titleText != null)
            titleText.text = objective.description;

        if (pointsText != null)
            pointsText.text = $"{objective.points} pts";

        UpdateState();
    }

    public bool MatchesObjective(Objective obj)
    {
        return boundObjective == obj;
    }

    public void UpdateState()
    {
        if (boundObjective == null) return;

        bool completed = boundObjective.isCompleted;

        // 1) Toggle encendido/apagado
        if (toggle != null)
            toggle.isOn = completed;

        // 2) Color del check
        if (checkmarkImage != null)
            checkmarkImage.color = completed ? completedCheckColor : pendingCheckColor;

        // 3) Texto principal
        if (titleText != null)
        {
            titleText.color = completed ? completedTextColor : pendingTextColor;

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

        // 4) 🔹 Color del texto de puntos (si existe)
        if (pointsText != null)
        {
            pointsText.color = completed ? completedTextColor : pendingTextColor;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveRowUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;     // Objective description
    public TextMeshProUGUI pointsText;    // Optional points label
    public Toggle toggle;

    [Header("Colors & Style")]
    public Color pendingTextColor = Color.white;
    public Color completedTextColor = Color.green;

    public Color pendingCheckColor = Color.white;
    public Color completedCheckColor = Color.green;

    public bool useStrikethroughOnComplete = true;

    // Internal references
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
            toggle.interactable = false; // UI is for display only
        }
    }

    public void Bind(Objective objective)
    {
        boundObjective = objective;

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

        // Toggle visual
        if (toggle != null)
            toggle.isOn = completed;

        // Checkmark color
        if (checkmarkImage != null)
            checkmarkImage.color = completed ? completedCheckColor : pendingCheckColor;

        // Main text styling
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

        // Points label color
        if (pointsText != null)
        {
            pointsText.color = completed ? completedTextColor : pendingTextColor;
        }
    }
}

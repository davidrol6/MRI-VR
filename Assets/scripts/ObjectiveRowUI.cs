using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveRowUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI titleText;
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
        // Autoconfigurar si no se ha hecho en el Inspector
        if (toggle == null)
            toggle = GetComponentInChildren<Toggle>();

        if (titleText == null)
            titleText = GetComponentInChildren<TextMeshProUGUI>();

        if (toggle != null)
        {
            // El checkmark suele estar en Toggle/Background/Checkmark
            var checkmark = toggle.graphic; // gráfico principal del Toggle
            checkmarkImage = checkmark as Image;
        }

        // El jugador no debe cambiar los objetivos a mano
        if (toggle != null)
            toggle.interactable = false;
    }

    public void Bind(Objective objective)
    {
        boundObjective = objective;

        if (titleText != null)
            titleText.text = objective.description;

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

        // 3) Color y estilo del texto
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
    }
}

using UnityEngine;
using UnityEngine.UI;

public class UIButtonObjectiveTrigger : MonoBehaviour
{
    [Header("Referencia al ObjectiveManager")]
    public ObjectiveManager objectiveManager;

    [Header("Descripción exacta del objetivo")]
    public string objectiveDescription;

    [Header("Solo para toggles (opcional)")]
    public Toggle toggleButton; 

    [Header("Solo para botones (opcional)")]
    public Button normalButton;

    void Start()
    {
        // Si es un toggle
        if (toggleButton != null)
        {
            toggleButton.onValueChanged.AddListener(OnToggleChanged);
        }

        // Si es un botón normal
        if (normalButton != null)
        {
            normalButton.onClick.AddListener(OnButtonClicked);
        }
    }

    void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            Debug.Log($"🟢 Toggle pulsado para objetivo: {objectiveDescription}");
            objectiveManager.CompleteObjective(objectiveDescription);
            objectiveManager.PrintObjectivesStatus(); // Para ver toda la lista en consola
        }
    }

    void OnButtonClicked()
    {
        Debug.Log("Botón pulsado: " + objectiveDescription);
        objectiveManager.CompleteObjective(objectiveDescription);
    }
}

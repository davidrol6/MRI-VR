using UnityEngine;
using UnityEngine.UI;

public class UIButtonObjectiveTrigger : MonoBehaviour
{
    [Header("Reference to ObjectiveManager")]
    public ObjectiveManager objectiveManager;

    [Header("Exact objective description")]
    public string objectiveDescription;

    [Header("For toggles (optional)")]
    public Toggle toggleButton;

    [Header("For regular buttons (optional)")]
    public Button normalButton;

    private void Start()
    {
        // Toggle UI event
        if (toggleButton != null)
        {
            toggleButton.onValueChanged.AddListener(OnToggleChanged);
        }

        // Regular button UI event
        if (normalButton != null)
        {
            normalButton.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            Debug.Log($"Toggle activated for objective: {objectiveDescription}");
            objectiveManager.CompleteObjective(objectiveDescription);
            objectiveManager.PrintObjectivesStatus();
        }
    }

    private void OnButtonClicked()
    {
        Debug.Log("Button clicked: " + objectiveDescription);
        objectiveManager.CompleteObjective(objectiveDescription);
    }
}

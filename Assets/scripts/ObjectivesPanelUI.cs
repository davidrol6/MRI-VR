using UnityEngine;
using System.Collections.Generic;

public class ObjectivesPanelUI : MonoBehaviour
{
    [Header("References")]
    public ObjectiveManager objectiveManager;
    public Transform contentParent;          // ScrollView/Viewport/Content
    public GameObject objectiveRowPrefab;    // Objective row prefab

    private List<ObjectiveRowUI> rows = new List<ObjectiveRowUI>();

    private void OnEnable()
    {
        if (objectiveManager == null)
            objectiveManager = ObjectiveManager.Instance;

        if (objectiveManager != null)
        {
            // Subscribe to objective completion event
            objectiveManager.OnObjectiveCompleted += HandleObjectiveCompleted;
        }

        // Build the list when the panel is enabled
        BuildList();
    }

    private void OnDisable()
    {
        if (objectiveManager != null)
        {
            objectiveManager.OnObjectiveCompleted -= HandleObjectiveCompleted;
        }
    }

    private void BuildList()
    {
        // Clear previous rows
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        rows.Clear();

        if (objectiveManager == null) return;

        // Create one row per objective
        foreach (var obj in objectiveManager.objectives)
        {
            GameObject rowGO = Instantiate(objectiveRowPrefab, contentParent);
            ObjectiveRowUI rowUI = rowGO.GetComponent<ObjectiveRowUI>();

            if (rowUI != null)
            {
                rowUI.Bind(obj);
                rows.Add(rowUI);
            }
        }
    }

    // Called when an objective is completed
    private void HandleObjectiveCompleted(Objective completedObjective)
    {
        foreach (var row in rows)
        {
            if (row.MatchesObjective(completedObjective))
            {
                row.UpdateState();
                break;
            }
        }
    }
}

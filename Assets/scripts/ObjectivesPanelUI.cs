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
            objectiveManager.OnObjectiveCompleted += HandleObjectiveCompleted;
            
            objectiveManager.OnObjectivesListUpdated += BuildList;
        }

        BuildList();
    }

    private void OnDisable()
    {
        if (objectiveManager != null)
        {
            objectiveManager.OnObjectiveCompleted -= HandleObjectiveCompleted;
            objectiveManager.OnObjectivesListUpdated -= BuildList;
        }
    }

    private void BuildList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        rows.Clear();

        if (objectiveManager == null) return;

        foreach (var obj in objectiveManager.objectives)
        {

            if (!obj.isActive) continue; 

            GameObject rowGO = Instantiate(objectiveRowPrefab, contentParent);
            ObjectiveRowUI rowUI = rowGO.GetComponent<ObjectiveRowUI>();

            if (rowUI != null)
            {
                rowUI.Bind(obj);


                if (rowUI.titleText != null)
                {
                    rowUI.titleText.text = obj.description
                    .Replace(" [H]", "")
                    .Replace(" [F]", "")
                    .Replace(" [Y]", "")  
                    .Replace(" [N]", ""); 
                }

                rows.Add(rowUI);
            }
        }
    }

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
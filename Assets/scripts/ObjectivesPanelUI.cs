/* * SCRIPT SUMMARY:
 * This script dynamically generates and manages the User Interface list of active objectives.
 * * * How it works:
 * - It subscribes to events from the ObjectiveManager to know when tasks change or the list needs refreshing.
 * - In 'BuildList', it instantiates a UI row prefab for every *active* objective found in the manager.
 * - It cleans up internal naming conventions (removing tags like [H] or [F]) so the user sees clean text.
 * - When an objective is completed, it efficiently updates the specific UI row without rebuilding the entire list.
 */

using UnityEngine;
using System.Collections.Generic;

public class ObjectivesPanelUI : MonoBehaviour
{
    [Header("References")]
    public ObjectiveManager objectiveManager;    // Reference to the core data source
    public Transform contentParent;              // The parent object (usually Viewport/Content) where rows are spawned
    public GameObject objectiveRowPrefab;        // The prefab to instantiate for each task

    // Internal tracking list to update specific rows later
    private List<ObjectiveRowUI> rows = new List<ObjectiveRowUI>();

    private void OnEnable()
    {
        // Auto-find manager if missing
        if (objectiveManager == null)
            objectiveManager = ObjectiveManager.Instance;

        if (objectiveManager != null)
        {
            // Subscribe to events:
            // 1. Update specific row when a task is done
            objectiveManager.OnObjectiveCompleted += HandleObjectiveCompleted;
            
            // 2. Rebuild the whole list if the scenario changes (e.g., new clinical case generated)
            objectiveManager.OnObjectivesListUpdated += BuildList;
        }

        // Initial build
        BuildList();
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks or errors when the menu is closed
        if (objectiveManager != null)
        {
            objectiveManager.OnObjectiveCompleted -= HandleObjectiveCompleted;
            objectiveManager.OnObjectivesListUpdated -= BuildList;
        }
    }

    // Clears the current UI and regenerates it based on the active objectives
    private void BuildList()
    {
        // 1. Clear existing items
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        rows.Clear();

        if (objectiveManager == null) return;

        // 2. Loop through all objectives in the database
        foreach (var obj in objectiveManager.objectives)
        {
            // Skip objectives that are not relevant to the current generated case
            if (!obj.isActive) continue; 

            // 3. Create the UI row
            GameObject rowGO = Instantiate(objectiveRowPrefab, contentParent);
            ObjectiveRowUI rowUI = rowGO.GetComponent<ObjectiveRowUI>();

            if (rowUI != null)
            {
                rowUI.Bind(obj);

                // 4. Format the text for display
                // Remove internal logic tags (like [H] for Head-First or [Y] for Yes-Contrast)
                // so the user sees a clean instruction.
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

    // Callback when a single objective is finished
    private void HandleObjectiveCompleted(Objective completedObjective)
    {
        // Find the specific UI row that corresponds to the completed data object
        foreach (var row in rows)
        {
            if (row.MatchesObjective(completedObjective))
            {
                // Refresh just this row (e.g., turn text green, check the box)
                row.UpdateState(); 
                break;
            }
        }
    }
}
/* * SCRIPT SUMMARY:
 * This script manages the high-level UI display of the checklist/objectives panel.
 * * * How it works:
 * - It acts as a View component in the MVC pattern, listening to the ObjectiveManager (Model).
 * - On initialization, it generates the list of tasks relevant to the current user Role (Patient vs Technician).
 * - It maintains a Dictionary (objectiveUIMap) to map logical Objective IDs to their UI GameObjects, allowing O(1) lookups for updates.
 * - It cleans raw data strings (removing logic tags like [H]) to present user-friendly text.
 */

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIObjectiveList : MonoBehaviour
{
    [Header("References")]
    public ObjectiveManager objectiveManager; // Reference to the core logic
    public Transform contentParent;           // The ScrollView Content object where rows are spawned
    public GameObject objectiveTemplate;      // The prefab for a single row

    [Header("Settings")]
    public bool showPoints = true;            // Toggle to display the score value of tasks

    // Internal cache: Maps the Description String -> The UI GameObject
    // This optimization prevents us from looping through the UI list every time a task updates.
    private Dictionary<string, GameObject> objectiveUIMap = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (objectiveManager == null) 
            objectiveManager = ObjectiveManager.Instance;
    }

    private void OnEnable()
    {
        if (objectiveManager != null)
        {
            // Subscribe to events:
            // 1. When a specific task is done, update just that row.
            objectiveManager.OnObjectiveCompleted += RefreshObjective;
            
            // 2. When the scenario changes (e.g., new patient), rebuild the whole list.
            objectiveManager.OnObjectivesListUpdated += InitializeUI;
        }
        
        // Build immediately when the menu is opened
        InitializeUI();
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent errors/memory leaks
        if (objectiveManager != null)
        {
            objectiveManager.OnObjectiveCompleted -= RefreshObjective;
            objectiveManager.OnObjectivesListUpdated -= InitializeUI;
        }
    }

    // Rebuilds the entire UI list from scratch
    public void InitializeUI()
    {
        // 1. Clear existing UI elements
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        objectiveUIMap.Clear();

        if (objectiveManager == null) return;

        // 2. Iterate through the database
        foreach (Objective obj in objectiveManager.objectives)
        {
            // Filter: Only show active objectives that belong to the current user's role
            if (obj.role == objectiveManager.roleProvider.role && obj.isActive)
            {
                // Instantiate the row
                GameObject go = Instantiate(objectiveTemplate, contentParent);
                go.SetActive(true);

                // Find child components by name (Requires specific hierarchy naming)
                TMP_Text text = go.transform.Find("DescriptionText").GetComponent<TMP_Text>();
                Image tick = go.transform.Find("TickIcon").GetComponent<Image>();

                // Clean the text: Remove logic tags (e.g., [H] for Head-First) for display
                string cleanDescription = obj.description
                    .Replace(" [H]", "")
                    .Replace(" [F]", "")
                    .Replace(" [Y]", "")
                    .Replace(" [N]", "");

                // Format the label
                if (showPoints)
                {
                    text.text = $"{cleanDescription} <color=#888888>({obj.points} pts)</color>";
                }
                else
                {
                    text.text = cleanDescription;
                }
                
                // Set initial state
                tick.enabled = obj.isCompleted;
                text.color = obj.isCompleted ? Color.green : Color.white;

                // Cache the reference for quick updates later
                objectiveUIMap[obj.description] = go;
            }
        }
    }

    // Efficiently updates a single row without rebuilding the list
    public void RefreshObjective(Objective obj)
    {
        // Lookup the GameObject in our Dictionary
        if (objectiveUIMap.TryGetValue(obj.description, out GameObject go))
        {
            // Update the Tick Icon
            Image tick = go.transform.Find("TickIcon").GetComponent<Image>();
            tick.enabled = obj.isCompleted;

            // Update the Text Color
            TMP_Text text = go.transform.Find("DescriptionText").GetComponent<TMP_Text>();
            text.color = obj.isCompleted ? Color.green : Color.white;
        }
    }
}
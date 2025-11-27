using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

// UI list for objectives of the current role
public class UIObjectiveList : MonoBehaviour
{
    public ObjectiveManager objectiveManager;
    public Transform contentParent;
    public GameObject objectiveTemplate;

    private Dictionary<string, GameObject> objectiveUIMap = new Dictionary<string, GameObject>();

    private void Start()
    {
        InitializeUI();
    }

    public void InitializeUI()
    {
        // Create UI elements once based on the current role
        foreach (Objective obj in objectiveManager.objectives)
        {
            if (obj.role == objectiveManager.roleProvider.role)
            {
                GameObject go = Instantiate(objectiveTemplate, contentParent);
                go.SetActive(true);

                TMP_Text text = go.transform.Find("DescriptionText").GetComponent<TMP_Text>();
                Image tick = go.transform.Find("TickIcon").GetComponent<Image>();

                text.text = $"{obj.description}  <color=#888888>({obj.points} pts)</color>";
                tick.enabled = obj.isCompleted;

                // Store reference for quick updates
                objectiveUIMap[obj.description] = go;
            }
        }
    }

    // Called only when a single objective changes (not in a loop)
    public void RefreshObjective(Objective obj)
    {
        if (objectiveUIMap.TryGetValue(obj.description, out GameObject go))
        {
            Image tick = go.transform.Find("TickIcon").GetComponent<Image>();
            tick.enabled = obj.isCompleted;

            TMP_Text text = go.transform.Find("DescriptionText").GetComponent<TMP_Text>();
            text.color = obj.isCompleted ? Color.green : Color.white;
        }
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIObjectiveList : MonoBehaviour
{
    public ObjectiveManager objectiveManager;
    public Transform contentParent;
    public GameObject objectiveTemplate;

    [Header("Settings")]
    public bool showPoints = true;
    private Dictionary<string, GameObject> objectiveUIMap = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (objectiveManager == null) objectiveManager = ObjectiveManager.Instance;
    }

    private void OnEnable()
    {
        if (objectiveManager != null)
        {
            objectiveManager.OnObjectiveCompleted += RefreshObjective;
            objectiveManager.OnObjectivesListUpdated += InitializeUI;
        }
        InitializeUI();
    }

    private void OnDisable()
    {
        if (objectiveManager != null)
        {
            objectiveManager.OnObjectiveCompleted -= RefreshObjective;
            objectiveManager.OnObjectivesListUpdated -= InitializeUI;
        }
    }

    public void InitializeUI()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        objectiveUIMap.Clear();

        if (objectiveManager == null) return;

        foreach (Objective obj in objectiveManager.objectives)
        {
            if (obj.role == objectiveManager.roleProvider.role && obj.isActive)
            {
                GameObject go = Instantiate(objectiveTemplate, contentParent);
                go.SetActive(true);

                TMP_Text text = go.transform.Find("DescriptionText").GetComponent<TMP_Text>();
                Image tick = go.transform.Find("TickIcon").GetComponent<Image>();

                string cleanDescription = obj.description
                    .Replace(" [H]", "")
                    .Replace(" [F]", "")
                    .Replace(" [Y]", "")
                    .Replace(" [N]", "");

                if (showPoints)
                {
                    text.text = $"{cleanDescription} <color=#888888>({obj.points} pts)</color>";
                }
                else
                {
                    text.text = cleanDescription;
                }
                
                tick.enabled = obj.isCompleted;
                text.color = obj.isCompleted ? Color.green : Color.white;

                objectiveUIMap[obj.description] = go;
            }
        }
    }

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
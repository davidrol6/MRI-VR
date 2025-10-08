using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIObjectiveList : MonoBehaviour
{
    public ObjectiveManager objectiveManager;
    public Transform contentParent;
    public GameObject objectiveTemplate; 

    private Dictionary<string, GameObject> objectiveUIMap = new Dictionary<string, GameObject>();

    void Start()
    {
        InitializeUI();
    }

    public void InitializeUI()
    {
        // Crea los elementos una sola vez
        foreach (Objective obj in objectiveManager.objectives)
        {
            if (obj.role == objectiveManager.roleProvider.role)
            {
                GameObject go = Instantiate(objectiveTemplate, contentParent);
                go.SetActive(true);

                TMP_Text text = go.transform.Find("DescriptionText").GetComponent<TMP_Text>();
                Image tick = go.transform.Find("TickIcon").GetComponent<Image>();

                text.text = obj.description;
                tick.enabled = obj.isCompleted;

                // Guarda referencia para actualizar rápido
                objectiveUIMap[obj.description] = go;
            }
        }
    }

    // Se llama solo cuando cambia un objetivo (no en bucle)
    public void RefreshObjective(Objective obj)
    {
        if (objectiveUIMap.TryGetValue(obj.description, out GameObject go))
        {
            Image tick = go.transform.Find("TickIcon").GetComponent<Image>();
            tick.enabled = obj.isCompleted;

            // Color verde si está completado
            TMP_Text text = go.transform.Find("DescriptionText").GetComponent<TMP_Text>();
            text.color = obj.isCompleted ? Color.green : Color.white;
        }
    }
}

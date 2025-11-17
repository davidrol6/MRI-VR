using UnityEngine;
using System.Collections.Generic;

public class ObjectivesPanelUI : MonoBehaviour
{
    [Header("Referencias")]
    public ObjectiveManager objectiveManager;
    public Transform contentParent;          // ScrollView/Viewport/Content
    public GameObject objectiveRowPrefab;    // Prefab de la fila

    private List<ObjectiveRowUI> rows = new List<ObjectiveRowUI>();

    private void OnEnable()
    {
        if (objectiveManager == null)
            objectiveManager = ObjectiveManager.Instance;

        if (objectiveManager != null)
        {
            // Nos suscribimos al evento
            objectiveManager.OnObjectiveCompleted += HandleObjectiveCompleted;
        }

        // Construimos la lista solo una vez (o cada vez que se habilite, según lo que necesites)
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
        // Limpia filas previas si las hubiera
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        rows.Clear();

        if (objectiveManager == null) return;

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

    // Se llama cuando se completa un objetivo
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

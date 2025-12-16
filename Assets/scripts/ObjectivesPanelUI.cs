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
            // Suscribirse a cuando se completa uno (para tacharlo/poner check)
            objectiveManager.OnObjectiveCompleted += HandleObjectiveCompleted;
            
            // IMPORTANTE: Suscribirse a cuando se genera un CASO NUEVO
            // Esto es necesario para borrar la lista vieja y poner la nueva al hacer Login
            objectiveManager.OnObjectivesListUpdated += BuildList;
        }

        // Construir la lista inicial
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
        // 1. Limpiar filas anteriores (Reset visual)
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        rows.Clear();

        if (objectiveManager == null) return;

        // 2. Crear una fila por cada objetivo ACTIVO
        foreach (var obj in objectiveManager.objectives)
        {
            // --- FILTRO CLAVE ---
            // Si el objetivo no es necesario para este caso (ej. Contraste en un caso sin contraste),
            // lo saltamos para que no aparezca en la lista.
            if (!obj.isActive) continue; 

            GameObject rowGO = Instantiate(objectiveRowPrefab, contentParent);
            ObjectiveRowUI rowUI = rowGO.GetComponent<ObjectiveRowUI>();

            if (rowUI != null)
            {
                // A. Pasamos los datos lógicos (aquí rowUI pone el texto con "[H]")
                rowUI.Bind(obj);

                // B. EL TRUCO VISUAL:
                // Como titleText es público en tu ObjectiveRowUI, lo sobrescribimos aquí mismo
                // para borrar las "pistas" antes de que el usuario las vea.
                if (rowUI.titleText != null)
                {
                    rowUI.titleText.text = obj.description
                    .Replace(" [H]", "")
                    .Replace(" [F]", "")
                    .Replace(" [Y]", "")  // <--- Añadir esto
                    .Replace(" [N]", ""); // <--- Añadir esto
                }

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
                row.UpdateState(); // Esto actualiza el color verde y el tachado
                break;
            }
        }
    }
}
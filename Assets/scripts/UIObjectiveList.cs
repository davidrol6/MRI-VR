using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIObjectiveList : MonoBehaviour
{
    public ObjectiveManager objectiveManager;
    public Transform contentParent;
    public GameObject objectiveTemplate;

    // Diccionario para encontrar rápido el UI Row usando la descripción original
    private Dictionary<string, GameObject> objectiveUIMap = new Dictionary<string, GameObject>();

    private void Awake()
    {
        // Aseguramos la referencia si no está asignada
        if (objectiveManager == null) objectiveManager = ObjectiveManager.Instance;
    }

    private void OnEnable()
    {
        if (objectiveManager != null)
        {
            // Nos suscribimos al evento de completar
            objectiveManager.OnObjectiveCompleted += RefreshObjective;
            // Nos suscribimos al evento de "Nuevo Caso Generado" para rehacer la lista
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
        // 1. LIMPIEZA: Borrar elementos antiguos por si se regenera el caso
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        objectiveUIMap.Clear();

        if (objectiveManager == null) return;

        // 2. CREACIÓN: Solo creamos los activos
        foreach (Objective obj in objectiveManager.objectives)
        {
            // FILTRO DE ROL Y ESTADO ACTIVO
            if (obj.role == objectiveManager.roleProvider.role && obj.isActive)
            {
                GameObject go = Instantiate(objectiveTemplate, contentParent);
                go.SetActive(true);

                TMP_Text text = go.transform.Find("DescriptionText").GetComponent<TMP_Text>();
                Image tick = go.transform.Find("TickIcon").GetComponent<Image>();

                // --- EL TRUCO VISUAL ---
                // Quitamos las etiquetas internas para que el usuario no vea "[H]"
                string cleanDescription = obj.description
                    .Replace(" [H]", "")
                    .Replace(" [F]", "");

                text.text = $"{cleanDescription} <color=#888888>({obj.points} pts)</color>";
                
                // Configuración inicial del estado
                tick.enabled = obj.isCompleted;
                text.color = obj.isCompleted ? Color.green : Color.white;

                // Guardamos en el diccionario usando la descripción ORIGINAL (con [H]) como clave
                // para que el sistema pueda encontrarlo luego.
                objectiveUIMap[obj.description] = go;
            }
        }
    }

    // Se llama cuando se completa un objetivo individual
    public void RefreshObjective(Objective obj)
    {
        // Buscamos en el diccionario usando la descripción original (que sí tiene el [H] o [F])
        if (objectiveUIMap.TryGetValue(obj.description, out GameObject go))
        {
            Image tick = go.transform.Find("TickIcon").GetComponent<Image>();
            tick.enabled = obj.isCompleted;

            TMP_Text text = go.transform.Find("DescriptionText").GetComponent<TMP_Text>();
            text.color = obj.isCompleted ? Color.green : Color.white;
        }
    }
}
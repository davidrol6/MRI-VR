using UnityEngine;
using System;
using System.Collections.Generic;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    [Header("Role provider reference")]
    public RoleProvider roleProvider;

    [Header("Objective list")]
    public List<Objective> objectives = new List<Objective>();

    public event Action<Objective> OnObjectiveCompleted;
    // Evento extra para avisar a la UI de que actualice la lista cuando cambiamos el caso
    public event Action OnObjectivesListUpdated; 

    private void Awake()
    {
        Instance = this;
    }

    // Método para activar/desactivar objetivos según su nombre
    public void SetObjectiveStatus(string exactDescription, bool isActive)
    {
        // Buscamos ignorando mayúsculas/minúsculas para evitar errores tontos
        Objective obj = objectives.Find(o => o.description.Equals(exactDescription, StringComparison.OrdinalIgnoreCase));
        
        if (obj != null)
        {
            obj.isActive = isActive;
            obj.isCompleted = false; // Reiniciamos por si acaso
        }
        else
        {
            Debug.LogWarning($"[ObjectiveManager] No se encontró el objetivo: {exactDescription}");
        }
    }

    // Método para refrescar la UI después de generar el caso
    public void RefreshObjectives()
    {
        OnObjectivesListUpdated?.Invoke();
    }

    public void CompleteObjective(string description)
    {
        Role currentRole = roleProvider.role;

        // Buscamos el objetivo que coincida Y que esté ACTIVO
        Objective obj = objectives.Find(o => o.description == description && o.role == currentRole);

        if (obj != null)
        {
            // SI EL OBJETIVO NO ESTÁ ACTIVO EN ESTE CASO, LO IGNORAMOS
            if (!obj.isActive) 
            {
                Debug.Log($"El objetivo '{description}' existe pero no está activo en este caso clínico.");
                return; 
            }

            if (!obj.isCompleted)
            {
                obj.isCompleted = true;
                Debug.Log($"Objective completed: '{obj.description}'");
                OnObjectiveCompleted?.Invoke(obj);
            }
        }
        else
        {
            Debug.LogWarning($"Objective '{description}' not found for role {currentRole}.");
        }
    }

    // Calcula puntuación máxima SOLO de los objetivos activos
    public int GetMaxScore()
    {
        int total = 0;
        foreach (var obj in objectives)
        {
            if (obj.isActive) total += obj.points;
        }
        return total;
    }

    // Calcula puntuación actual
    public int GetFinalScore()
    {
        int score = 0;
        foreach (var obj in objectives)
        {
            // Solo sumamos si está completado (y por ende estaba activo)
            if (obj.isCompleted && obj.isActive) 
                score += obj.points;
        }
        return score;
    }

    public void PrintObjectivesStatus()
    {
        Debug.Log("---- ESTADO DEL CASO CLÍNICO ----");
        foreach (var obj in objectives)
        {
            if (obj.isActive)
            {
                string estado = obj.isCompleted ? "<color=green>COMPLETADO</color>" : "<color=red>PENDIENTE</color>";
                Debug.Log($"[ACTIVO] {obj.description} -> {estado}");
            }
            else
            {
                Debug.Log($"<color=grey>[INACTIVO] {obj.description} (No requerido en este caso)</color>");
            }
        }
        Debug.Log("----------------------------------");
    }
}